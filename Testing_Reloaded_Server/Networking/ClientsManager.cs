using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary;
using SharedLibrary.Models;
using SharedLibrary.Statics;
using SharedLibrary.UI;
using static SharedLibrary.Statics.Statics;
using Testing_Reloaded_Server.Models;
using Timer = System.Threading.Timer;

namespace Testing_Reloaded_Server.Networking {
    public class ClientsManager {
        private List<Client> clients { get; set; }

        public List<Client> Clients => clients;

        private TcpListener tcpListener;
        private bool running = true;

        private Thread clientsThread;

        private Timer pollTimer;

        public delegate string ReceivedMessageFromClientDelegate(Client c, JObject message);

        public event ReceivedMessageFromClientDelegate ReceivedMessageFromClient;

        public ClientsManager() {
            clients = new List<Client>();
        }

        public void Start() {
            tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, Constants.SERVER_PORT));
            clientsThread = new Thread(LoopClients) {Name = "LoopingThread", IsBackground = true};
            pollTimer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(Constants.POLL_TIME));
            clientsThread.Start();
        }

        private void TimerCallback(object state) {
            foreach (var client in clients) {
                lock (client) {
                    if (client.TestState.OK && Math.Abs((client.LastUpdate - Statics.ApplicationTime).TotalSeconds) >
                        Constants.CRASH_DECLARED_TIME)
                        client.TestState.State = UserTestState.UserState.Crashed;

                    ReceivedMessageFromClient?.Invoke(client, null);
                }


            }
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

            var sWriter = new StreamWriter(stream, Constants.USED_ENCODING) {AutoFlush = true};
            var sReader = new StreamReader(stream, Constants.USED_ENCODING);

            Client connectedClient = null;

            int errorCount = 0;

            while (client.Connected) {
                string json = "";

                try {

                    if (connectedClient != null && connectedClient.TestState.State == UserTestState.UserState.Crashed) break;

                    if (connectedClient == null ||
                        (connectedClient.TestState.State != UserTestState.UserState.Finishing &&
                         stream.DataAvailable)) {
                        lock (tcpListener) {
                            json = sReader.ReadLine();
                        }
                    } else {
                        Thread.Sleep(50);
                        continue;
                    }

                    Debug.WriteLine($"{Thread.CurrentThread.Name}: Received from " +
                                    $"{((IPEndPoint) client.Client.RemoteEndPoint).Address}:{((IPEndPoint) client.Client.RemoteEndPoint).Port} message {json}");

                    JObject data = JObject.Parse(json);

                    
                    if (data["Action"].ToString() == "Connect") {
                        // get next id 
                        int nextId = clients.Count == 0 ? 0 : clients.Max(ob => ob.Id) + 1;

                        connectedClient =
                            new Client(nextId, data["User"].ToObject<User>()) {
                                TestState = new UserTestState() {State = UserTestState.UserState.Connected},
                                ClientAppVersion = Version.Parse(data["AppVersion"].ToString())
                            };


                        var reconnectCheck = clients.FirstOrDefault(c =>
                            c.Name == connectedClient.Name && c.Surname == connectedClient.Surname &&
                            c.TestState.State != UserTestState.UserState.Crashed);

                        // check connection name
                        if (reconnectCheck != null) {
                            sWriter.WriteLine(GetJson(new {Status = "Error", ErrorCode = "RCNNV", Message = $"Cannot reconnect to a non-crashed client. Disconnect from {reconnectCheck.Surname} first"}));
                            break;
                        }

                        // check version match
                        if (connectedClient.ClientAppVersion.Major !=
                            Constants.APPLICATION_VERSION.Major ||
                            connectedClient.ClientAppVersion.Minor !=
                            Constants.APPLICATION_VERSION.Minor) {
                            sWriter.WriteLine(GetJson(new {
                                Status = "Error", ErrorCode = "VRSMM",
                                ServerVersion = Constants.APPLICATION_VERSION.ToString()
                            }));
                            continue;
                        }

                        sWriter.WriteLine(GetJson(new {Status = "OK"}));

                        int messagePort = (int) data["MessagePort"];

                        // start control connection
                        try {
                            messageClient.Connect((client.Client.RemoteEndPoint as IPEndPoint).Address,
                                messagePort);
                        } catch (SocketException ex) {
                            sWriter.WriteLine(GetJson(new {
                                Status = "ERROR", ErrorCode = "MCNOP", Message = "Could not open message connection"
                            }));
                        }


                        sWriter.WriteLine(GetJson(new {Status = "OK"}));

                        connectedClient.ControlConnection = messageClient;
                        connectedClient.DataConnection = client;

                        Thread.CurrentThread.Name = $"{connectedClient.Surname}Thread";

                        var reconnectedClient = clients.FirstOrDefault(c =>
                            c.Name == connectedClient.Name && c.Surname == connectedClient.Surname);

                        // call reconnect function to reconnect the client
                        if (reconnectedClient != null && reconnectedClient.TestState.State ==
                                                      UserTestState.UserState.Crashed
                                                      && ReconnectClient(ref connectedClient, ref reconnectedClient,
                                                          sReader, sWriter)) {
                            sWriter.WriteLine(GetJson(new {Status = "OK"}));
                            continue;
                        }

                        // client cant or doesnt want to reconnect, add as new
                        clients.Add(connectedClient);
                        sWriter.WriteLine(GetJson(new {Status = "OK"}));
                    }

                    // cannot use following functions if not yet connected
                    if (connectedClient == null) {
                        sWriter.WriteLine(JsonConvert.SerializeObject(new
                            {Status = "ERROR", Code = "SYNFIRST", Message = "Client must call Connect first"}));
                        sWriter.Flush();
                        continue;
                    }

                    lock (connectedClient) { // make sure client is modified my others (PollTimer)
                        connectedClient.LastUpdate = ApplicationTime;

                        if (data["Action"].ToString() == "Disconnect") {

                            if (connectedClient.TestState.State != UserTestState.UserState.Finished) {
                                connectedClient.TestState.State = UserTestState.UserState.Crashed; // set user state
                            }

                            // update ui
                            ReceivedMessageFromClient?.Invoke(connectedClient, data);

                            break;
                        }

                        // send response
                        string response = ReceivedMessageFromClient?.Invoke(connectedClient, data);

                        if (response != null) {
                            sWriter.WriteLine(response);
                            sWriter.Flush();
                        }
                    }
                } catch (Exception e) {
                    // max 2 errors before a client is declared crashed
                    Debug.WriteLine(
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

            if (connectedClient.DataConnection != null && connectedClient.DataConnection.Connected) {
                connectedClient.DataConnection.GetStream().Close();
                connectedClient.DataConnection.Close();
            }

            if (connectedClient.DataConnection != null && connectedClient.ControlConnection.Connected) {
                connectedClient.ControlConnection.GetStream().Close();
                connectedClient.ControlConnection.Close();
            }
        }

        private bool ReconnectClient(ref Client connectedClient,
            ref Client reconnectedClient,
            StreamReader sr,
            StreamWriter sw) {
            sw.WriteLine(GetJson(new {
                Status = "WARN", Code = "RCN", Message = "Client with same name already connected, want to reconnect"
            }));

            JObject response = JObject.Parse(sr.ReadLine());

            if (response["Action"].ToString() == "Ignore") {
                return false;
            }


            sw.WriteLine(GetJson(new {Action = "Sync", UserState = reconnectedClient.TestState}));

            response = JObject.Parse(sr.ReadLine());

            if (response["Status"].ToString() != "OK") {
                Debug.WriteLine(
                    $"{Thread.CurrentThread.Name}: Client {reconnectedClient} reconnection has failed");
                return false;
            }

            reconnectedClient.ControlConnection = connectedClient.ControlConnection;
            reconnectedClient.DataConnection = connectedClient.DataConnection;
            reconnectedClient.TestState = connectedClient.TestState;
            // reconnectedClient.Id = connectedClient.Id;
            reconnectedClient.PCHostname = connectedClient.PCHostname;


            connectedClient = reconnectedClient;
            return true;
        }

        public async Task SendBytes(Client client, byte[] bytes) {
            await SharedLibrary.Networking.NetworkUtils.SendBytesToNetwork(client.DataConnection.GetStream(),
                new MemoryStream(bytes));
        }

        public async Task SendMessageToClients(string message) {
            foreach (Client client in Clients) {
                byte[] bytes = Constants.USED_ENCODING.GetBytes($"{message}\r\n");

                await client.DataConnection.GetStream().WriteAsync(bytes, 0, bytes.Length);
                await client.DataConnection.GetStream().FlushAsync();
            }
        }

        public async Task SendControlMessageToClients(string message, Predicate<Client> predicate) {
            foreach (Client client in Clients) {
                if (!predicate.Invoke(client)) continue;

                byte[] bytes = Constants.USED_ENCODING.GetBytes($"{message}\r\n");

                await client.ControlConnection.GetStream().WriteAsync(bytes, 0, bytes.Length);
                await client.ControlConnection.GetStream().FlushAsync();
            }
        }

        public async Task SendControlMessageToClient(string message, Client client) {
            byte[] bytes = Constants.USED_ENCODING.GetBytes($"{message}\r\n");

            await client.ControlConnection.GetStream().WriteAsync(bytes, 0, bytes.Length);
            await client.ControlConnection.GetStream().FlushAsync();
        }
    }
}