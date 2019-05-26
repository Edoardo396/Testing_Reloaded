using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary.Models;
using SharedLibrary.Statics;

namespace Testing_Reloaded_Server.Networking {
    public class ServerPublishingManager {
        private readonly Test currentTest;
        private UdpClient listenClient;
        private readonly Thread udpListenThread;

        public ServerPublishingManager(Test currentTest) {
            this.currentTest = currentTest;
            udpListenThread = new Thread(ListenForNewUdpClients) {Name = "UDPListenThread", IsBackground = true};
            udpListenThread.Start();
        }

        public bool AllowClientsOnHold { get; set; } = true;

        private void ListenForNewUdpClients() {
            listenClient = new UdpClient(new IPEndPoint(IPAddress.Any, Constants.BROADCAST_PORT_SERVER));
            listenClient.Client.ReceiveTimeout = 2000;

            try {
                while (true) {
                    var ep = new IPEndPoint(IPAddress.Any, Constants.BROADCAST_PORT_CLIENT);

                    var sendBytes = Constants.USED_ENCODING.GetBytes(
                        JsonConvert.SerializeObject(new {Action = "Report", Hostname = Environment.MachineName}));
                    listenClient.Send(sendBytes, sendBytes.Length,
                        new IPEndPoint(IPAddress.Broadcast, Constants.BROADCAST_PORT_CLIENT));

                    byte[] bytes;

                    try {
                        bytes = listenClient.Receive(ref ep);
                    } catch (Exception e) {
                        continue;
                    }

                    var message = Constants.USED_ENCODING.GetString(bytes);

                    if (!AllowClientsOnHold) continue;

                    var json = JObject.Parse(message);
                    var response = GetResponse(json);
                    var responseBytes = Constants.USED_ENCODING.GetBytes(response);
                    listenClient.Send(responseBytes, responseBytes.Length,
                        new IPEndPoint(ep.Address, Constants.CLIENT_PORT));
                }
            } catch (ThreadAbortException e) {
            }
        }

        private string GetResponse(JObject json) {
            if (json["Action"].Value<string>() == "discover")
                return JsonConvert.SerializeObject(new
                    {Action = "Report", Hostname = Environment.MachineName});

            return JsonConvert.SerializeObject(new {Action = "Error", Error = "invalid request"});
        }
    }
}