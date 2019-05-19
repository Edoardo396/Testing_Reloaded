using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary;

namespace Testing_Reloaded_Client {
    public class TestManager {
        private User me;
        private NetworkManager netManager;

        private Test currentTest;
        public UserTestState TestState;

        public Test CurrentTest => currentTest;

        public TestManager(Server server, User me) {
            this.me = me;
            this.netManager = new NetworkManager(server);
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
            TestState = new UserTestState { RemainingTime = currentTest.Time, State = UserTestState.UserState.Waiting};
        }

        public async Task WaitForTestStart() {
            if (currentTest.State == Test.TestState.Started) return;

            while (true) {
                string response = await netManager.ReadLine();

                var jsonO = JObject.Parse(response);
                if (jsonO["Action"].ToString() != "TestStarted") continue;

                currentTest.State = Test.TestState.Started;
                break;
            }
        }

        public async Task DownloadTestDocumentation() {
            TestState.State = UserTestState.UserState.DownloadingDocs;

            string path = ResolvePath(currentTest.ClientTestPath);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);

            var packet = new {Action = "GetTestDocs", PacketID = Statics.GenerateRandomPacketId()};


            await netManager.WriteLine(JsonConvert.SerializeObject(packet));

            var fastZip = new FastZip();

            var fileBytes = await netManager.ReadData();

            if (fileBytes == null) return;

            Directory.CreateDirectory(Path.Combine(path, "Documentation"));

            var file = new MemoryStream(fileBytes);

            fastZip.ExtractZip(file, Path.Combine(path, "Documentation"), FastZip.Overwrite.Always, null, "",
                null, false, true);
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
    }
}