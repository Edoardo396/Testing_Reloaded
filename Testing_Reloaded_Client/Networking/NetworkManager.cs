using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary.Networking;
using SharedLibrary.Statics;

namespace Testing_Reloaded_Client.Networking {
    public class NetworkManager {
        private TcpClient tcpConnection;
        private Server currentServer;


        private NetworkStream networkStream;
        private StreamReader netReader;
        private StreamWriter netWriter;

        private Thread messageThread;

        public bool ListenMessages { get; set; } = false;
        public bool ListeningForMessages => messageThread.ThreadState == ThreadState.Running;

        public delegate string ReceivedMessageFromServerDelegate(Server s, JObject message);

        public event ReceivedMessageFromServerDelegate ReceivedMessageFromServer;

        public bool Connected => netReader != null && tcpConnection.Connected;

        public NetworkManager(Server server) {
            this.currentServer = server;
            messageThread = new Thread(MessagePool) {Name = "MessageThread", IsBackground =  true};
        }

        public async Task ConnectToServer() {
            tcpConnection = new TcpClient(AddressFamily.InterNetwork);
            await tcpConnection.ConnectAsync(currentServer.IP, Constants.SERVER_PORT); // try catch
            Thread.Sleep(1000);

            networkStream = tcpConnection.GetStream();
            netReader = new StreamReader(networkStream, Constants.USED_ENCODING);
            netWriter = new StreamWriter(networkStream, Constants.USED_ENCODING);
        }

        public async Task WriteLine(string data) {
            await netWriter.WriteLineAsync(data);
            await netWriter.FlushAsync();
        }

        public async Task<string> ReadLine() {
            return await netReader.ReadLineAsync();
        }

        public async Task<MemoryStream> ReadData() {
            // get data info
            var strData = await this.ReadLine();
            var dataInfo = JObject.Parse(strData);
            int size = (int) dataInfo["Size"];

            if (dataInfo["FileType"].ToString() == "nodata") return null;

            return await NetworkUtils.ReadNetworkBytes(networkStream, size,
                tcpConnection.ReceiveBufferSize);
        }

        public void SendBytes(byte[] bytes) {
            var stream = tcpConnection.GetStream();
            var wStream = new StreamWriter(stream);

            wStream.WriteLine(JsonConvert.SerializeObject(new {Status = "OK", FileType = "zip", Size = bytes.Length}));
            wStream.Flush();

            foreach (byte b in bytes) {
                stream.WriteByte(b);
            }

            stream.Flush();
        }

        public void StartListeningForMessages() {
            messageThread.Start();
        }

        private void MessagePool() {
            while (true) {
                if (tcpConnection.Available == 0 || !ListenMessages) {
                    Thread.Sleep(500);
                    continue;
                }

                string message = ReadLine().Result;
                var json = JObject.Parse(message);

                string response = ReceivedMessageFromServer?.Invoke(currentServer, json);

                if (response != null) {
                    WriteLine(response).Wait();
                }
            }

        }
    }
}