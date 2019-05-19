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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary;

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
            tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, SharedLibrary.Constants.SERVER_PORT));
            clientsThread = new Thread(LoopClients) {Name = "LoopingThread", IsBackground = true };
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

            var stream = client.GetStream();

            var sWriter = new StreamWriter(stream, SharedLibrary.Constants.USED_ENCODING);
            var sReader = new StreamReader(stream, SharedLibrary.Constants.USED_ENCODING);

            Client connectedClient = null;

            while (client.Connected) {               
                string json = "";

                if (stream.DataAvailable) {
                    json = sReader.ReadLine();
                } else {
                   // Thread.Sleep(50);
                    continue;
                }


                JObject data = JObject.Parse(json);

                if (data["Action"].ToString() == "Connect") {
                    int nextId = clients.Count == 0 ? 0 : clients.Max(ob => ob.Id) + 1;

                    connectedClient = new Client(nextId, JsonConvert.DeserializeObject<User>(data["User"].ToString()), client) {TestState = new UserTestState() {State = UserTestState.UserState.Connected}};

                    Thread.CurrentThread.Name = $"{connectedClient.Surname}Thread";
                    clients.Add(connectedClient);
                    sWriter.WriteLine(JsonConvert.SerializeObject(new { Status = "OK" }));
                    sWriter.Flush();                    
                }

                if (connectedClient == null) {
                    sWriter.WriteLine(JsonConvert.SerializeObject(new {Status = "ERROR", Code = "SYNFIRST", Message = "Client must call Connect first"}));
                    sWriter.Flush();
                    continue;
                }

                string response = this.ReceivedMessageFromClient?.Invoke(connectedClient, data);

                if (response != null) {
                    sWriter.WriteLine(response);
                    sWriter.Flush();
                }
            }
        }

        public void SendBytes(Client client, byte[] bytes) {

            var stream = client.TcpClient.GetStream();
            var wStream = new StreamWriter(stream);

            wStream.WriteLine(JsonConvert.SerializeObject(new {Status = "OK", FileType = "zip", Size = bytes.Length}));
            wStream.Flush();

            foreach (byte b in bytes) {
                stream.WriteByte(b);
            }

            stream.Flush();
        }

        public async Task SendMessageToClients(string message, bool blocking = true) {
            foreach (Client client in Clients) {
                var stream = new StreamWriter(client.TcpClient.GetStream());

                if (blocking) {
                    await stream.WriteLineAsync(message);
                    await stream.FlushAsync();
                } else {
                    stream.WriteLineAsync(message);
                    stream.FlushAsync();
                }
            }
        }
    }
}