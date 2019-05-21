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
                this.currentTest = (Test)message["Test"].ToObject(typeof(Test));

                if (currentTest.State == Test.TestState.OnHold)
                    this.TestState.State = UserTestState.UserState.OnHold;

                SendStateUpdate();
                ReloadUI?.Invoke();
                return null;
            }

            




            return null;
        }

        public string ResolvePath(string path) {
            return Environment.ExpandEnvironmentVariables(path).Replace("$surname", me.Surname)
                .Replace("$test_name", currentTest.TestName);
        }

        public async Task Connect() {
            await netManager.ConnectToServer();
            await netManager.WriteLine(JsonConvert.SerializeObject(new {Action = "Connect", User = me}));
            await netManager.ReadLine();
        }

        public async Task DownloadTestData() {
            var packet = new {Action = "GetTestInfo", PacketID = Statics.GenerateRandomPacketId()};

            var str = (JsonConvert.SerializeObject(packet));

            await netManager.WriteLine(str);

            var response = await netManager.ReadLine();
            var jsonResponse = JObject.Parse(response);

            this.currentTest = JsonConvert.DeserializeObject<Test>(jsonResponse["Test"].ToString());
            TestState = new UserTestState {RemainingTime = currentTest.Time, State = UserTestState.UserState.Waiting};
        }

        public async Task WaitForTestStart() {
            if (currentTest.State != Test.TestState.NotStarted) return;

            await SendStateUpdate();

            while (true) {
                string response = await netManager.ReadLine();

                var jsonO = JObject.Parse(response);
                if (jsonO["Action"].ToString() != "TestStarted") continue;

                currentTest.State = Test.TestState.Started;
                break;
            }
        }

        public void TestStarted() {
            netManager.ListenMessages = true;
        }

        public async Task DownloadTestDocumentation() {
            TestState.State = UserTestState.UserState.DownloadingDocs;

            await SendStateUpdate();

            string path = ResolvePath(currentTest.ClientTestPath);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);

            var packet = new {Action = "GetTestDocs", PacketID = Statics.GenerateRandomPacketId()};


            await netManager.WriteLine(JsonConvert.SerializeObject(packet));

            var fastZip = new FastZip();

            var file = await netManager.ReadData();

            if (file == null) return;

            Directory.CreateDirectory(Path.Combine(path, "Documentation"));

            fastZip.ExtractZip(file, Path.Combine(path, "Documentation"), FastZip.Overwrite.Always, null, "",
                null, false, true);

            netManager.StartListeningForMessages();
        }

        public async Task SendStateUpdate() {
            string json = JsonConvert.SerializeObject(new {Action = "StateUpdate", State = TestState});

            await netManager.WriteLine(json);
        }

        // must be called every 1 second
        public void TimeElapsed(uint seconds) {
            if (currentTest.State == Test.TestState.Started)
                TestState.RemainingTime -= TimeSpan.FromSeconds(seconds);

            if (TestState.RemainingTime.Seconds == 0)
                SendStateUpdate();
        }

        public async Task Handover() {
            await this.SendStateUpdate();

            await netManager.WriteLine(JsonConvert.SerializeObject(new {Action = "TestHandover"}));

            var fastZip = new FastZip();

            var stream = new MemoryStream();

            fastZip.CreateZip(stream, ResolvedTestPath, true, null, @"-bin$;-obj$;-Documentation$");

            netManager.SendBytes(stream.ToArray());

            var json = JObject.Parse(await netManager.ReadLine());

            if (json["Status"].ToString() != "OK") {
                throw new Exception("Server returned wrong message");
            }

            if (CurrentTest.DeleteFilesAfterEnd) {
                Directory.Delete(ResolvedTestPath, true);
            }
        }
    }
}