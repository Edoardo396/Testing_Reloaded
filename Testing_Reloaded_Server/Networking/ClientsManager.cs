using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary;

namespace Testing_Reloaded_Server.Networking {
    public class ClientsManager {
        private List<Client> clients { get; set; }

        public IReadOnlyList<Client> Clients => new ReadOnlyCollection<Client>(clients);

        private TcpListener tcpListener;
        private bool running = true;

        private Thread clientsThread;

        public delegate string ReceivedMessageFromClientDelegate(Client c, JObject message);

        public event ReceivedMessageFromClientDelegate ReceivedMessageFromClient;

        public ClientsManager() {
            clients = new List<Client>();
        }

        public void Start() {
            tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, SharedLibrary.Constants.SERVER_PORT));
            clientsThread = new Thread(LoopClients);
            clientsThread.Start();
        }

        private void LoopClients() {
            tcpListener.Start();

            while (running) {
                var newClient = tcpListener.AcceptTcpClient();
                var thread = new Thread(new ParameterizedThreadStart(HandleClient));
                thread.Start(newClient);
            }
        }

        private void HandleClient(object o) {
            TcpClient client = (TcpClient) o;

            var sWriter = new StreamWriter(client.GetStream(), SharedLibrary.Constants.USED_ENCODING);
            var sReader = new StreamReader(client.GetStream(), SharedLibrary.Constants.USED_ENCODING);

            Client connectedClient = null;

            while (client.Connected) {
                System.Diagnostics.Debug.WriteLine(client.GetStream().DataAvailable);
                var json = sReader.ReadLine();

                JObject data = JObject.Parse(json);

                if (data["Action"].ToString() == "Connect") {
                    connectedClient = new Client(JsonConvert.DeserializeObject<User>(data["User"].ToString()), client);
                    clients.Add(connectedClient);
                }

                if (connectedClient == null) {
                    sWriter.WriteLine(JsonConvert.SerializeObject(new {Status = "ERROR", Code = "SYNFIRST", Message = "Client must call Connect first"}));
                    sWriter.Flush();
                }

                string response = this.ReceivedMessageFromClient?.Invoke(connectedClient, data);

                if (response != null) {
                    sWriter.WriteLine(response);
                    sWriter.Flush();
                }
            }
        }
    }
}