using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary;
using SharedLibrary.Models;
using SharedLibrary.UI;
using Testing_Reloaded_Server.Models;

namespace Testing_Reloaded_Server.Networking {
    public class ClientsManager {
        private BindingList<Client> clients { get; set; }

        public BindingList<Client> Clients => clients;

        private TcpListener tcpListener;
        private bool running = true;

        private Thread clientsThread;

        public delegate string ReceivedMessageFromClientDelegate(Client c, JObject message);

        public event ReceivedMessageFromClientDelegate ReceivedMessageFromClient;

        public ClientsManager() {
            clients = new ThreadedBindingList<Client>();
        }

        public void Start() {
            tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, SharedLibrary.Statics.Constants.SERVER_PORT));
            clientsThread = new Thread(LoopClients) {Name = "LoopingThread", IsBackground = true};
            clientsThread.Start();
        }

        private void LoopClients() {
            tcpListener.Start();

            while (running) {
                var newClient = tcpListener.AcceptTcpClient();
                var thread = new Thread(new ParameterizedThreadStart(HandleClient)) {IsBackground = true};
                thread.Start(newClient);
            }
        }

        private void HandleClient(object o) {
            TcpClient client = (TcpClient) o;

            TcpClient messageClient = new TcpClient();

            var stream = client.GetStream();

            var sWriter = new StreamWriter(stream, SharedLibrary.Statics.Constants.USED_ENCODING) {AutoFlush = true};
            var sReader = new StreamReader(stream, SharedLibrary.Statics.Constants.USED_ENCODING);

            Client connectedClient = null;

            int errorCount = 0;

            while (client.Connected) {
                string json = "";

                try {
                    if (stream.DataAvailable) {
                        lock (tcpListener) {
                            json = sReader.ReadLine();
                        }
                    } else {
                        Thread.Sleep(50);
                        continue;
                    }


                    System.Diagnostics.Debug.WriteLine($"{Thread.CurrentThread.Name}: Received from " +
                                                       $"{((IPEndPoint) client.Client.RemoteEndPoint).Address}:{((IPEndPoint) client.Client.RemoteEndPoint).Port} message {json}");

                    JObject data = JObject.Parse(json);

                    if (data["Action"].ToString() == "Connect") {
                        int nextId = clients.Count == 0 ? 0 : clients.Max(ob => ob.Id) + 1;

                        connectedClient =
                            new Client(nextId, JsonConvert.DeserializeObject<User>(data["User"].ToString()), client)
                                {TestState = new UserTestState() {State = UserTestState.UserState.Connected}};

                        Thread.CurrentThread.Name = $"{connectedClient.Surname}Thread";
                        clients.Add(connectedClient);

                        int messagePort = (int) data["MessagePort"];

                        try {
                            messageClient.Connect(connectedClient.IP, messagePort);
                        } catch (SocketException ex) {
                            sWriter.WriteLine(JsonConvert.SerializeObject(new {
                                Status = "ERROR", ErrorCode = "MCNOP", Message = "Could not open message connection"
                            }));
                        }

                        sWriter.WriteLine(JsonConvert.SerializeObject(new {Status = "OK"}));

                        connectedClient.ControlConnection = messageClient;
                    }

                    if (connectedClient == null) {
                        sWriter.WriteLine(JsonConvert.SerializeObject(new
                            {Status = "ERROR", Code = "SYNFIRST", Message = "Client must call Connect first"}));
                        sWriter.Flush();
                        continue;
                    }

                    if (data["Action"].ToString() == "Disconnect") {
                        if (connectedClient.TestState.State != UserTestState.UserState.Finished) {
                            connectedClient.TestState.State = UserTestState.UserState.Crashed;
                        }

                        this.ReceivedMessageFromClient?.Invoke(connectedClient, data);

                        break;
                    }

                    string response = this.ReceivedMessageFromClient?.Invoke(connectedClient, data);

                    if (response != null) {
                        sWriter.WriteLine(response);
                        sWriter.Flush();
                    }
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine(
                        $"{++errorCount} fatal error with client {connectedClient}: {e.Message}");
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
            var stream = client.DataConnection.GetStream();
            var wStream = new StreamWriter(stream);

            await wStream.WriteLineAsync(JsonConvert.SerializeObject(new
                {Status = "OK", FileType = "zip", Size = bytes.Length}));
            await wStream.FlushAsync();

            foreach (byte b in bytes) {
                stream.WriteByte(b);
            }

            await stream.FlushAsync();
        }

        public async Task SendMessageToClients(string message) {
            foreach (Client client in Clients) {
                byte[] bytes = SharedLibrary.Statics.Constants.USED_ENCODING.GetBytes($"{message}\r\n");

                await client.DataConnection.GetStream().WriteAsync(bytes, 0, bytes.Length);
                await client.DataConnection.GetStream().FlushAsync();
            }
        }

        public async Task SendControlMessageToClients(string message, Predicate<Client> predicate) {
            foreach (Client client in Clients) {
                if (!predicate.Invoke(client)) continue;

                byte[] bytes = SharedLibrary.Statics.Constants.USED_ENCODING.GetBytes($"{message}\r\n");

                await client.ControlConnection.GetStream().WriteAsync(bytes, 0, bytes.Length);
                await client.ControlConnection.GetStream().FlushAsync();
            }
        }

        public async Task SendControlMessageToClient(string message, Client client) {
            byte[] bytes = SharedLibrary.Statics.Constants.USED_ENCODING.GetBytes($"{message}\r\n");

            await client.ControlConnection.GetStream().WriteAsync(bytes, 0, bytes.Length);
            await client.ControlConnection.GetStream().FlushAsync();
        }
    }
}