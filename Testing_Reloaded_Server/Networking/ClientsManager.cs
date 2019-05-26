using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary.Models;
using SharedLibrary.Statics;
using SharedLibrary.UI;
using Testing_Reloaded_Server.Models;

namespace Testing_Reloaded_Server.Networking {
    public class ClientsManager {
        private Thread clientsThread;
        private readonly bool running = true;

        private TcpListener tcpListener;

        public ClientsManager() {
            clients = new ThreadedBindingList<Client>();
        }

        private BindingList<Client> clients { get; }

        public BindingList<Client> Clients => clients;

        public event ReceivedMessageFromClientDelegate ReceivedMessageFromClient;

        public void Start() {
            tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, Constants.SERVER_PORT));
            clientsThread = new Thread(LoopClients) {Name = "LoopingThread", IsBackground = true};
            clientsThread.Start();
        }

        private void LoopClients() {
            tcpListener.Start();

            while (running) {
                var newClient = tcpListener.AcceptTcpClient();
                var thread = new Thread(HandleClient) {IsBackground = true};
                thread.Start(newClient);
            }
        }

        private void HandleClient(object o) {
            var client = (TcpClient) o;

            var stream = client.GetStream();

            var sWriter = new StreamWriter(stream, Constants.USED_ENCODING);
            var sReader = new StreamReader(stream, Constants.USED_ENCODING);

            Client connectedClient = null;

            var errorCount = 0;

            while (client.Connected) {
                var json = "";

                try {
                    if (stream.DataAvailable) {
                        lock (tcpListener) {
                            json = sReader.ReadLine();
                        }
                    } else {
                        Thread.Sleep(50);
                        continue;
                    }


                    Debug.WriteLine($"{Thread.CurrentThread.Name}: Received from " +
                                    $"{((IPEndPoint) client.Client.RemoteEndPoint).Address}:{((IPEndPoint) client.Client.RemoteEndPoint).Port} message {json}");

                    var data = JObject.Parse(json);


                    if (data["Action"].ToString() == "Connect") {
                        var nextId = clients.Count == 0 ? 0 : clients.Max(ob => ob.Id) + 1;

                        connectedClient =
                            new Client(nextId, JsonConvert.DeserializeObject<User>(data["User"].ToString()), client)
                                {TestState = new UserTestState {State = UserTestState.UserState.Connected}};

                        Thread.CurrentThread.Name = $"{connectedClient.Surname}Thread";
                        clients.Add(connectedClient);
                        sWriter.WriteLine(JsonConvert.SerializeObject(new {Status = "OK"}));
                        sWriter.Flush();
                    }

                    if (connectedClient == null) {
                        sWriter.WriteLine(JsonConvert.SerializeObject(new
                            {Status = "ERROR", Code = "SYNFIRST", Message = "Client must call Connect first"}));
                        sWriter.Flush();
                        continue;
                    }

                    if (data["Action"].ToString() == "Disconnect") {
                        if (connectedClient.TestState.State != UserTestState.UserState.Finished)
                            connectedClient.TestState.State = UserTestState.UserState.Crashed;

                        ReceivedMessageFromClient?.Invoke(connectedClient, data);

                        break;
                    }

                    var response = ReceivedMessageFromClient?.Invoke(connectedClient, data);

                    if (response != null) {
                        sWriter.WriteLine(response);
                        sWriter.Flush();
                    }
                } catch (Exception e) {
                    Debug.WriteLine($"{++errorCount} fatal error with client {connectedClient}: {e.Message}");
                    if (errorCount > 2) {
                        connectedClient.TestState.State = UserTestState.UserState.Crashed;
                        client.Close();
                        break;
                    }
                }
            }

            if (connectedClient.TestState.State != UserTestState.UserState.Finished)
                connectedClient.TestState.State = UserTestState.UserState.Crashed;
        }

        public async Task SendBytes(Client client, byte[] bytes) {
            var stream = client.TcpClient.GetStream();
            var wStream = new StreamWriter(stream);

            await wStream.WriteLineAsync(JsonConvert.SerializeObject(new
                {Status = "OK", FileType = "zip", Size = bytes.Length}));
            await wStream.FlushAsync();

            foreach (var b in bytes) stream.WriteByte(b);

            await stream.FlushAsync();
        }

        public async Task SendMessageToClients(string message) {
            foreach (var client in Clients) {
                var bytes = Constants.USED_ENCODING.GetBytes($"{message}\r\n");

                await client.TcpClient.GetStream().WriteAsync(bytes, 0, bytes.Length);
                await client.TcpClient.GetStream().FlushAsync();
            }
        }

        public delegate string ReceivedMessageFromClientDelegate(Client c, JObject message);
    }
}