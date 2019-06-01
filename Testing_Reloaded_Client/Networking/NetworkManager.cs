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
using Testing_Reloaded_Server.Exceptions;

namespace Testing_Reloaded_Client.Networking {
    public class NetworkManager {
        private Server currentServer;

        public Server CurrentServer => currentServer;

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
            mainTcpConnection = new TcpClient(AddressFamily.InterNetwork)
                {ReceiveTimeout = SharedLibrary.Statics.Constants.SOCKET_TIMEOUT, NoDelay = true};
            await mainTcpConnection.ConnectAsync(currentServer.IP, Constants.SERVER_PORT); // try catch
            Thread.Sleep(1000);

            networkStream = mainTcpConnection.GetStream();
            netReader = new StreamReader(networkStream, Constants.USED_ENCODING);
            netWriter = new StreamWriter(networkStream, Constants.USED_ENCODING);

            await WriteLine(
                JsonConvert.SerializeObject(new {
                    Action = "Connect", User = user, MessagePort = listenPort,
                    AppVersion = SharedLibrary.Statics.Constants.APPLICATION_VERSION.ToString()
                }));

            // wait for ok, version check
            var versionResponse = JObject.Parse(await ReadLine());

            if (versionResponse["Status"].ToString() != "OK") {
                mainTcpConnection.Close();

                if (versionResponse["ErrorCode"].ToString() == "VRSMM")
                    throw new VersionMismatchException(
                        "Client server version mismatch, make sure they are running the same version") {
                        ClientVersion = SharedLibrary.Statics.Constants.APPLICATION_VERSION,
                        ServerVersion = Version.Parse(versionResponse["ServerVersion"].ToString())
                    };

                throw new Exception("server returned error during version validation, server message: " +
                                    (versionResponse.ContainsKey("ErrorMessage")
                                        ? versionResponse["ErrorMessage"].ToString()
                                        : "undefined"));
            }


            messageListener.Start(0);
            var messageConnection = await messageListener.AcceptTcpClientAsync();
            messageConnection.ReceiveTimeout = SharedLibrary.Statics.Constants.SOCKET_TIMEOUT;

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
            await SharedLibrary.Networking.NetworkUtils.SendBytesToNetwork(mainTcpConnection.GetStream(),
                new MemoryStream(bytes));
        }

        public async Task Disconnect() {
            await WriteLine(JsonConvert.SerializeObject(new {Action = "Disconnect"}));
            mainTcpConnection.Close();
            messageThread.Abort();
        }
    }
}