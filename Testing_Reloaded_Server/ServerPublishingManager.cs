using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Testing_Reloaded_Server;

namespace Testing_Reloaded_Server {
    public class ServerPublishingManager {
        private Thread udpListenThread;
        private UdpClient listenClient;

        private readonly Test currentTest;

        public bool AllowClientsOnHold { get; set; } = true;

        public ServerPublishingManager(Test currentTest) {
            this.currentTest = currentTest;
            udpListenThread = new Thread(ListenForNewUdpClients) {Name = "UDPListenThread", IsBackground = true};
            udpListenThread.Start();
        }

        private void ListenForNewUdpClients() {
            listenClient = new UdpClient(new IPEndPoint(IPAddress.Any, SharedLibrary.Constants.SERVER_PORT));

            try {
                while (true) {
                    var ep = new IPEndPoint(IPAddress.Any, SharedLibrary.Constants.CLIENT_PORT);
                    var bytes = listenClient.Receive(ref ep);
                    var message = SharedLibrary.Constants.USED_ENCODING.GetString(bytes);

                    if (!AllowClientsOnHold) continue;

                    var json = JObject.Parse(message);
                    var response = GetResponse(json);
                    var responseBytes = SharedLibrary.Constants.USED_ENCODING.GetBytes(response);
                    listenClient.Send(responseBytes, responseBytes.Length,
                        new IPEndPoint(ep.Address, SharedLibrary.Constants.CLIENT_PORT));
                }
            } catch (ThreadAbortException e) {
            }
        }

        private string GetResponse(JObject json) {
            if (json["Action"].Value<string>() == "discover") {
                return JsonConvert.SerializeObject(new
                    {Action = "Report", Hostname = Environment.MachineName});
            }

            return JsonConvert.SerializeObject(new {Action = "Error", Error = "invalid request"});
        }
    }
}