using System;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary.Models;
using Testing_Reloaded_Client.Networking;
using static SharedLibrary.Models.UserTestState;

namespace Testing_Reloaded_Client {
    public class TestManager {
        private readonly User me;
        private readonly NetworkManager netManager;
        public UserTestState TestState;

        public TestManager(Server server, User me) {
            this.me = me;
            netManager = new NetworkManager(server);
            netManager.ReceivedMessageFromServer += ReceivedServerMessage;
        }

        public Test CurrentTest { get; private set; }

        public string ResolvedTestPath => ResolvePath(CurrentTest.ClientTestPath);

        public event Action ReloadUI;

        private string ReceivedServerMessage(Server s, JObject message) {
            if (message["Action"].ToString() == "UpdateTest") {
                CurrentTest = (Test) message["Test"].ToObject(typeof(Test));

                if (CurrentTest.State == Test.TestState.OnHold)
                    TestState.State = UserState.OnHold;

                SendStateUpdate();
                ReloadUI?.Invoke();
                return null;
            }


            return null;
        }

        public string ResolvePath(string path) {
            return Environment.ExpandEnvironmentVariables(path).Replace("$surname", me.Surname)
                .Replace("$test_name", CurrentTest.TestName);
        }

        public async Task Connect() {
            await netManager.ConnectToServer();
            await netManager.WriteLine(JsonConvert.SerializeObject(new {Action = "Connect", User = me}));
            await netManager.ReadLine();
        }

        public async Task DownloadTestData() {
            var packet = new {Action = "GetTestInfo"};

            var str = JsonConvert.SerializeObject(packet);

            await netManager.WriteLine(str);

            var response = await netManager.ReadLine();
            var jsonResponse = JObject.Parse(response);

            CurrentTest = JsonConvert.DeserializeObject<Test>(jsonResponse["Test"].ToString());
            TestState = new UserTestState {RemainingTime = CurrentTest.Time, State = UserState.Waiting};
        }

        public async Task WaitForTestStart() {
            if (CurrentTest.State != Test.TestState.NotStarted) return;

            await SendStateUpdate();

            while (true) {
                var response = await netManager.ReadLine();

                var jsonO = JObject.Parse(response);
                if (jsonO["Action"].ToString() != "TestStarted") continue;

                CurrentTest.State = Test.TestState.Started;
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
            netManager.ProcessMessages = false;
            TestState.State = UserState.DownloadingDocs;

            // await SendStateUpdate();

            var path = ResolvePath(CurrentTest.ClientTestPath);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);

            var packet = new {Action = "GetTestDocs"};


            await netManager.WriteLine(JsonConvert.SerializeObject(packet));

            var fastZip = new FastZip();

            var file = await netManager.ReadData();

            if (file != null) {
                Directory.CreateDirectory(Path.Combine(path, "Documentation"));

                fastZip.ExtractZip(file, Path.Combine(path, "Documentation"), FastZip.Overwrite.Always, null, "",
                    null, false, true);

                var result = await netManager.ReadLine();
            }

            // netManager.StartListeningForMessages();
        }

        public async Task SendStateUpdate() {
            var json = JsonConvert.SerializeObject(new {Action = "StateUpdate", State = TestState});

            await netManager.WriteLine(json);
        }

        // must be called every 1 second
        public void TimeElapsed(uint seconds) {
            if (CurrentTest.State == Test.TestState.Started)
                TestState.RemainingTime -= TimeSpan.FromSeconds(seconds);

            if (TestState.RemainingTime.Seconds == 0)
                SendStateUpdate();
        }

        public async Task Handover() {
            netManager.ProcessMessages = false;

            await netManager.WriteLine(JsonConvert.SerializeObject(new {Action = "TestHandover"}));

            var fastZip = new FastZip();

            var stream = new MemoryStream();

            fastZip.CreateZip(stream, ResolvedTestPath, true, null, @"-bin$;-obj$;-Documentation$");

            await netManager.SendBytes(stream.ToArray());

            var json = JObject.Parse(await netManager.ReadLine());

            if (json["Status"].ToString() != "OK") throw new Exception("Server error");

            if (CurrentTest.DeleteFilesAfterEnd) Directory.Delete(ResolvedTestPath, true);
        }

        public void TestRunning() {
            // netManager.ProcessMessages = true;
        }
    }
}