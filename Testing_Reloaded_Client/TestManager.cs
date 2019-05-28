using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary;
using SharedLibrary.Models;
using SharedLibrary.Statics;
using Testing_Reloaded_Client.Networking;
using static SharedLibrary.Models.UserTestState;
using static SharedLibrary.Statics.Statics;

namespace Testing_Reloaded_Client {
    public class TestManager {
        private User me;
        private NetworkManager netManager;

        private Test currentTest;
        public UserTestState TestState;

        public Test CurrentTest => currentTest;

        public string ResolvedTestPath => this.ResolvePath(this.currentTest.ClientTestPath);

        public event Action ReloadUI;

        public TestManager(Server server, User me) {
            this.me = me;
            this.netManager = new NetworkManager(server);
            netManager.ReceivedMessageFromServer += ReceivedServerMessage;
        }

        private string ReceivedServerMessage(Server s, JObject message) {
            if (message["Action"].ToString() == "UpdateTest") {
                var sentTest = (Test) message["Test"].ToObject(typeof(Test));
                this.currentTest.State = sentTest.State;

                switch (currentTest.State) {
                    case Test.TestState.OnHold:
                        this.TestState.State = UserState.OnHold;
                        break;
                    case Test.TestState.Started:
                        this.TestState.State = UserState.Testing;
                        break;
                }

                SendStateUpdate();
                ReloadUI?.Invoke();
                return null;
            }

            if (message["Action"].ToString() == "AddTime") {
                var time = (TimeSpan) message["TimeSpan"];
                this.TestState.RemainingTime += time;

                SendStateUpdate();
                ReloadUI?.Invoke();
                return null;
            }

            if (message["Action"].ToString() == "Sync") {
                this.TestState = message["UserState"].ToObject<UserTestState>();
                ReloadUI?.Invoke();
            }

            if (message["Action"].ToString() == "Pause") {
                this.TestState.State = UserTestState.UserState.OnHold;
                ReloadUI?.Invoke();
            }

            if (message["Action"].ToString() == "Resume") {
                this.TestState.State = UserTestState.UserState.Testing;
                ReloadUI?.Invoke();
            }

            if (message["Action"].ToString() == "Handover") {
                try {
                    this.Handover().Wait();
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine("Handover Failed");
                }
            }

            return null;
        }

        public string ResolvePath(string path) {
            return Environment.ExpandEnvironmentVariables(path).Replace("$surname", me.Surname)
                .Replace("$test_name", currentTest.TestName);
        }

        public async Task Connect() {
            await netManager.ConnectToServer(me);

            JObject response = JObject.Parse(await netManager.ReadLine());

            if (response["Status"].ToString() == "OK") // successfully connected, not further action needed
                return;

            // reconnect logic
            if (response["Code"].ToString() == "RCN") {
                await netManager.WriteLine(GetJson(new
                    {Action = "Reconnect"})); // TODO Defaulting to reconnect, ask for confirm

                JObject syncMessage = JObject.Parse(await netManager.ReadLine());

                ReceivedServerMessage(netManager.CurrentServer, syncMessage); // sync local state

                // TODO Download files

                await netManager.WriteLine(GetJson(new {Status = "OK"}));

                netManager.CurrentServer.ReconnectFlag = true;

                await netManager.ReadLine();
            }
        }

        public async Task DownloadTestData() {
            var packet = new {Action = "GetTestInfo"};

            var str = (JsonConvert.SerializeObject(packet));

            await netManager.WriteLine(str);

            var response = await netManager.ReadLine();
            var jsonResponse = JObject.Parse(response);

            this.currentTest = JsonConvert.DeserializeObject<Test>(jsonResponse["Test"].ToString());

            if (netManager.CurrentServer.ReconnectFlag) {
                TestState.State = MapDefaultTestState(CurrentTest.State);
            } else {
                TestState = new UserTestState {
                    RemainingTime = currentTest.Time, State = MapDefaultTestState(CurrentTest.State)
                };
            }
        }

        public async Task WaitForTestStart() {
            if (currentTest.State != Test.TestState.NotStarted) {
                this.TestState.State = UserState.Testing;
                return;
            }

            await SendStateUpdate();

            while (true) {
                string response = await netManager.ReadLine();

                var jsonO = JObject.Parse(response);
                if (jsonO["Action"].ToString() != "TestStarted") continue;

                currentTest.State = Test.TestState.Started;
                break;
            }
        }

        public async Task Disconnect() {
            await netManager.Disconnect();
        }

        public void TestStarted() {
            netManager.ProcessMessages = true;
        }

        public async Task DownloadTestDocumentation() {
            // netManager.ProcessMessages = false;
            TestState.State = UserState.DownloadingDocs;

            // await SendStateUpdate();

            string path = ResolvePath(currentTest.ClientTestPath);


            if (Directory.Exists(path) && !netManager.CurrentServer.ReconnectFlag)
                Directory.Delete(path, true);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            var packet = new {Action = "GetTestDocs"};


            await netManager.WriteLine(JsonConvert.SerializeObject(packet));

            var fastZip = new FastZip();

            var file = await netManager.ReadData();

            if (file != null) {
                if (Directory.Exists(Path.Combine(path, "Documentation")))
                    Directory.Delete(Path.Combine(path, "Documentation"), true);

                Directory.CreateDirectory(Path.Combine(path, "Documentation"));

                fastZip.ExtractZip(file, Path.Combine(path, "Documentation"), FastZip.Overwrite.Always, null, "",
                    null, false, true);

                var result = await netManager.ReadLine();
            }
        }

        public async Task SendStateUpdate() {
            string json = JsonConvert.SerializeObject(new {Action = "StateUpdate", State = TestState});

            await netManager.WriteLine(json);
        }

        // must be called every 1 second
        public async Task TimeElapsed(uint seconds) {
            if (TestState.State == UserTestState.UserState.Testing)
                TestState.RemainingTime -= TimeSpan.FromSeconds(seconds);

            try {
                if (TestState.RemainingTime.Seconds % 2 == 0)
                    await SendStateUpdate();
            } catch (Exception e) {
                TestState.State = UserState.OnHold;
                throw;
            }
        }

        public async Task Handover() {
            var fastZip = new FastZip();
            var stream = new MemoryStream();
            JObject json = null;
            
            try {

                fastZip.CreateZip(stream, ResolvedTestPath, true, null, @"-bin$;-obj$;-Documentation$");
                await netManager.SendBytes(stream.ToArray());
                json = JObject.Parse(await netManager.ReadLine());

            } catch (Exception e) {
                TestState.State = UserState.OnHold;
                throw;
            }

            if (json["Status"].ToString() != "OK") {
                throw new Exception("Server error");
            }

            if (CurrentTest.DeleteFilesAfterEnd) {
                Directory.Delete(ResolvedTestPath, true);
            }

            TestState.State = UserState.Finished;
        }
    }
}