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
        private Server server;

        private TcpClient tcpClient;
        private NetworkStream dataStream;
        private StreamWriter netWriter;
        private StreamReader netReader;

        private Test currentTest;

        public TestManager(Server server) {
            this.server = server;
            tcpClient = new TcpClient(AddressFamily.InterNetwork);
        }

        public static string ResolvePath(string path) {
            return Environment.ExpandEnvironmentVariables(path).Replace("$cognome", Me.Surname);
        }

        public async Task ConnectToServer() {
            await tcpClient.ConnectAsync(server.IP, SharedLibrary.Constants.SERVER_PORT);
            dataStream = tcpClient.GetStream();
            netWriter = new StreamWriter(dataStream);
            netReader = new StreamReader(dataStream);
        }

        public async Task DownloadTestData() {
            if (dataStream == null) return; // TODO

            var packet = new {Action = "GetTestInfo", PacketID = Statics.GenerateRandomPacketId()};

            var str = (JsonConvert.SerializeObject(packet));

            await netWriter.WriteLineAsync(str);

            var response = await netReader.ReadLineAsync();
            var jsonResponse = JObject.Parse(response);

            this.currentTest = JsonConvert.DeserializeObject<Test>(jsonResponse["Test"].ToString());
        }

        public async Task DownloadTestDocumentation() {
            if (dataStream == null || currentTest == null) return; // TODO

            string path = ResolvePath(currentTest.DataDownloadPath);
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);

            var packet = new { Action = "GetTestDocs", PacketID = Statics.GenerateRandomPacketId() };

            await netWriter.WriteLineAsync(JsonConvert.SerializeObject(packet));
            var response = await netReader.ReadLineAsync();

            var jsonResponse = JObject.Parse(response);

            if (jsonResponse["FileType"].ToString() == "archive") {

                int fileSize = (int)jsonResponse.GetValue("FileSize");  // 2GB Limit
                var bytes = new byte[fileSize];

                await dataStream.ReadAsync(bytes, 0, bytes.Length);           

                var fastZip = new FastZip();

                fastZip.ExtractZip(new MemoryStream(bytes), path, FastZip.Overwrite.Always, null, null, null, true, true);
            }
        }
    }
}