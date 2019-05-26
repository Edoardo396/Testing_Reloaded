using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary.Models;
using SharedLibrary.Networking;
using SharedLibrary.Statics;

namespace Testing_Reloaded_Client.Networking {
    public class NetworkManager {
        private Server currentServer;

        private TcpClient mainTcpConnection;

        private Thread messageThread;

        private NetworkStream networkStream;
        private StreamReader netReader;
        private StreamWriter netWriter;

        public bool ProcessMessages { get; set; } = false;

        public delegate string ReceivedMessageFromServerDelegate(Server s, JObject message);

        public event ReceivedMessageFromServerDelegate ReceivedMessageFromServer;

        public bool Connected => netReader != null && mainTcpConnection.Connected;

        public NetworkManager(Server server) {
            this.currentServer = server;
            messageThread = new Thread(new ParameterizedThreadStart(MessageLoop))
                {Name = "MessageThread", IsBackground = true};
        }

        public async Task ConnectToServer(User user) {
            int listenPort = SharedLibrary.Networking.NetworkUtils.GetAvailablePort(10000);
            var messageListener = new TcpListener(IPAddress.Any, listenPort);


            // set up main connection to transmit data
            mainTcpConnection = new TcpClient(AddressFamily.InterNetwork);
            await mainTcpConnection.ConnectAsync(currentServer.IP, Constants.SERVER_PORT); // try catch
            Thread.Sleep(1000);

            networkStream = mainTcpConnection.GetStream();
            netReader = new StreamReader(networkStream, Constants.USED_ENCODING);
            netWriter = new StreamWriter(networkStream, Constants.USED_ENCODING);

            await WriteLine(
                JsonConvert.SerializeObject(new {Action = "Connect", User = user, MessagePort = listenPort}));
            messageListener.Start(0);
            var messageConnection = await messageListener.AcceptTcpClientAsync();

            var response = JObject.Parse(await ReadLine());

            if (response["Status"].ToString() != "OK") {
                messageConnection.Close();
                messageListener.Stop();
                mainTcpConnection.Close();
                throw new Exception("Cannot establish message connection");
            }

            messageThread.Start(messageConnection);
            messageListener.Stop();
        }

        private void MessageLoop(object o) {
            TcpClient client = (TcpClient) o;

            var read = new StreamReader(client.GetStream(), SharedLibrary.Statics.Constants.USED_ENCODING);

            try {
                while (true) {
                    if (client.Available <= 0) {
                        Thread.Sleep(50);
                        continue;
                    }

                    try {
                        var message = JObject.Parse(read.ReadLine());


                        ReceivedMessageFromServer?.Invoke(currentServer, message);
                    } catch (Exception e) {
                        // ignored
                    }
                }
            } catch (ThreadAbortException ex) {

            }

            client.Close();
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
                mainTcpConnection.ReceiveBufferSize);
        }

        public async Task SendBytes(byte[] bytes) {
            var stream = mainTcpConnection.GetStream();
            var wStream = new StreamWriter(stream);

            await wStream.WriteLineAsync(JsonConvert.SerializeObject(new
                {Status = "OK", FileType = "zip", Size = bytes.Length}));
            await wStream.FlushAsync();

            long sent = 0;

            while (sent < bytes.Length) {
                long toSend = (bytes.Length - sent) < mainTcpConnection.SendBufferSize
                    ? (bytes.Length - sent)
                    : mainTcpConnection.SendBufferSize;

                await stream.WriteAsync(bytes, (int) sent, (int) toSend);
                sent += toSend;

                await stream.FlushAsync();
            }
        }

        public async Task Disconnect() {
            await WriteLine(JsonConvert.SerializeObject(new {Action = "Disconnect"}));
            mainTcpConnection.Close();
            messageThread.Abort();
        }
    }
}