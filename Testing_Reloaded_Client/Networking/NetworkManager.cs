using System;
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

        public bool ProcessMessages { get; set; } = false;
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
            System.Diagnostics.Debug.WriteLine($"Sending {data}");
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

        public async Task SendBytes(byte[] bytes) {
            var stream = tcpConnection.GetStream();
            var wStream = new StreamWriter(stream);

            await wStream.WriteLineAsync(JsonConvert.SerializeObject(new {Status = "OK", FileType = "zip", Size = bytes.Length}));
            await wStream.FlushAsync();

            long sent = 0;

            while(sent < bytes.Length) {

                long toSend = (bytes.Length - sent) < tcpConnection.SendBufferSize ? (bytes.Length - sent) : tcpConnection.SendBufferSize;

                await stream.WriteAsync(bytes, (int)sent, (int)toSend);
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

                string message = ReadLine().Result;
                var json = JObject.Parse(message);

                string response = ReceivedMessageFromServer?.Invoke(currentServer, json);

                if (response != null) {
                    WriteLine(response).Wait();
                }
            }

        }

        public async Task Disconnect()
        {
            await WriteLine(JsonConvert.SerializeObject(new { Action = "Disconnect" }));
            tcpConnection.Close();
        }
    }
}