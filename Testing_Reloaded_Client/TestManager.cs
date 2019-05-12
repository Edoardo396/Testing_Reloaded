using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
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

        public TestManager(Server server) {
            this.server = server;
            tcpClient = new TcpClient(AddressFamily.InterNetwork);
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

            string response;
            JObject jsonResponse;

            while (true) {
                response = await netReader.ReadLineAsync();
                jsonResponse = JObject.Parse(response);

                if ((int)jsonResponse.GetValue("PacketID") == packet.PacketID) break;
            }




        }
        
    }
}