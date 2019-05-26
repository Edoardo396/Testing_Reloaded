using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary.Networking;
using SharedLibrary.Statics;
using ThreadState = System.Threading.ThreadState;

namespace Testing_Reloaded_Client.Networking {
    public class NetworkManager {
        public delegate string ReceivedMessageFromServerDelegate(Server s, JObject message);

        private readonly Server currentServer;

        private readonly Thread messageThread;
        private StreamReader netReader;


        private NetworkStream networkStream;
        private StreamWriter netWriter;
        private TcpClient tcpConnection;

        public NetworkManager(Server server) {
            currentServer = server;
            messageThread = new Thread(MessagePool) {Name = "MessageThread", IsBackground = true};
        }

        public bool ProcessMessages { get; set; } = false;
        public bool ListeningForMessages => messageThread.ThreadState == ThreadState.Running;

        public bool Connected => netReader != null && tcpConnection.Connected;

        public event ReceivedMessageFromServerDelegate ReceivedMessageFromServer;

        public async Task ConnectToServer() {
            tcpConnection = new TcpClient(AddressFamily.InterNetwork);
            await tcpConnection.ConnectAsync(currentServer.IP, Constants.SERVER_PORT); // try catch
            Thread.Sleep(1000);

            networkStream = tcpConnection.GetStream();
            netReader = new StreamReader(networkStream, Constants.USED_ENCODING);
            netWriter = new StreamWriter(networkStream, Constants.USED_ENCODING);
        }

        public async Task WriteLine(string data) {
            Debug.WriteLine($"Sending {data}");
            await netWriter.WriteLineAsync(data);
            await netWriter.FlushAsync();
        }

        public async Task<string> ReadLine() {
            return await netReader.ReadLineAsync();
        }

        public async Task<MemoryStream> ReadData() {
            // get data info
            var strData = await ReadLine();
            var dataInfo = JObject.Parse(strData);
            var size = (int) dataInfo["Size"];

            if (dataInfo["FileType"].ToString() == "nodata") return null;

            return await NetworkUtils.ReadNetworkBytes(networkStream, size,
                tcpConnection.ReceiveBufferSize);
        }

        public async Task SendBytes(byte[] bytes) {
            var stream = tcpConnection.GetStream();
            var wStream = new StreamWriter(stream);

            await wStream.WriteLineAsync(JsonConvert.SerializeObject(new
                {Status = "OK", FileType = "zip", Size = bytes.Length}));
            await wStream.FlushAsync();

            long sent = 0;

            while (sent < bytes.Length) {
                var toSend = bytes.Length - sent < tcpConnection.SendBufferSize
                    ? bytes.Length - sent
                    : tcpConnection.SendBufferSize;

                await stream.WriteAsync(bytes, (int) sent, (int) toSend);
                sent += toSend;

                await stream.FlushAsync();
            }
        }

        public void StartListeningForMessages() {
            messageThread.Start();
        }

        private void MessagePool() {
            while (true) {
                if (tcpConnection.Available == 0 || !ProcessMessages) {
                    Thread.Sleep(500);
                    continue;
                }

                var message = ReadLine().Result;
                var json = JObject.Parse(message);

                var response = ReceivedMessageFromServer?.Invoke(currentServer, json);

                if (response != null) WriteLine(response).Wait();
            }
        }

        public async Task Disconnect() {
            await WriteLine(JsonConvert.SerializeObject(new {Action = "Disconnect"}));
            tcpConnection.Close();
        }
    }
}