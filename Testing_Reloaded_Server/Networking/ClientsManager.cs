using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using static SharedLibrary.Statics.Statics;
using Testing_Reloaded_Server.Models;

namespace Testing_Reloaded_Server.Networking {
    public class ClientsManager {
        private List<Client> clients { get; set; }

        public List<Client> Clients => clients;

        private TcpListener tcpListener;
        private bool running = true;

        private Thread clientsThread;

        public delegate string ReceivedMessageFromClientDelegate(Client c, JObject message);

        public event ReceivedMessageFromClientDelegate ReceivedMessageFromClient;

        public ClientsManager() {
            clients = new List<Client>();
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
                    if (connectedClient == null || (connectedClient.TestState.State != UserTestState.UserState.Finishing && stream.DataAvailable)) {
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
                        // get next id 
                        int nextId = clients.Count == 0 ? 0 : clients.Max(ob => ob.Id) + 1;

                        connectedClient =
                            new Client(nextId, data["User"].ToObject<User>())
                                {TestState = new UserTestState() {State = UserTestState.UserState.Connected}};

                        int messagePort = (int) data["MessagePort"];

                        // start control connection
                        try {
                            messageClient.Connect((client.Client.RemoteEndPoint as IPEndPoint).Address, messagePort);
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

                    if (data["Action"].ToString() == "Disconnect") {
                        connectedClient.ControlConnection.Close(); // close control connection

                        if (connectedClient.TestState.State != UserTestState.UserState.Finished) {
                            connectedClient.TestState.State = UserTestState.UserState.Crashed; // set user state
                        }

                        // update ui
                        this.ReceivedMessageFromClient?.Invoke(connectedClient, data);

                        break;
                    }

                    // send response
                    string response = this.ReceivedMessageFromClient?.Invoke(connectedClient, data);

                    if (response != null) {
                        sWriter.WriteLine(response);
                        sWriter.Flush();
                    }
                } catch (Exception e) {
                    // max 2 errors before a client is declared crashed
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
                System.Diagnostics.Debug.WriteLine(
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