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

        public TestManager(Server server, User me) {
            
            this.me = me;
            this.netManager = new NetworkManager(server);
           
        }

        public static string ResolvePath(string path) {
            return Environment.ExpandEnvironmentVariables(path).Replace("$cognome", me.Surname);
        }

        public async Task Connect() {
            await netManager.ConnectToServer();
        }
     
        public async Task DownloadTestData() {

            var packet = new {Action = "GetTestInfo", PacketID = Statics.GenerateRandomPacketId()};

            var str = (JsonConvert.SerializeObject(packet));

            await netManager.WriteLine(str);

            var response = await netManager.ReadLine();
            var jsonResponse = JObject.Parse(response);

            this.currentTest = JsonConvert.DeserializeObject<Test>(jsonResponse["Test"].ToString());
        }

        public async Task DownloadTestDocumentation() {
            
            string path = ResolvePath(currentTest.DataDownloadPath);
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);

            var packet = new { Action = "GetTestDocs", PacketID = Statics.GenerateRandomPacketId() };


            await netManager.WriteLine(JsonConvert.SerializeObject(packet));

            var fastZip = new FastZip();

            MemoryStream file = await netManager.ReadFile();

            fastZip.ExtractZip(file, ResolvePath(currentTest.DataDownloadPath), FastZip.Overwrite.Always, null, "", null, false, true);
        }
    }
}