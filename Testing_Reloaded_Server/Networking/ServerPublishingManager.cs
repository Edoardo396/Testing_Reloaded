using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary.Models;

namespace Testing_Reloaded_Server.Networking {
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
            listenClient = new UdpClient(new IPEndPoint(IPAddress.Any, SharedLibrary.Statics.Constants.BROADCAST_PORT_SERVER));
            listenClient.Client.ReceiveTimeout = 2000;

            try {
                while (true) {
                    var ep = new IPEndPoint(IPAddress.Any, SharedLibrary.Statics.Constants.BROADCAST_PORT_CLIENT);

                    var sendBytes = SharedLibrary.Statics.Constants.USED_ENCODING.GetBytes(JsonConvert.SerializeObject(new { Action = "Report", Hostname = Environment.MachineName }));
                    listenClient.Send(sendBytes, sendBytes.Length, new IPEndPoint(IPAddress.Broadcast, SharedLibrary.Statics.Constants.BROADCAST_PORT_CLIENT));

                    byte[] bytes;

                    try {
                        bytes = listenClient.Receive(ref ep);
                    } catch(Exception e) {
                        continue;
                    }

                    var message = SharedLibrary.Statics.Constants.USED_ENCODING.GetString(bytes);

                    if (!AllowClientsOnHold) continue;

                    var json = JObject.Parse(message);
                    var response = GetResponse(json);
                    var responseBytes = SharedLibrary.Statics.Constants.USED_ENCODING.GetBytes(response);
                    listenClient.Send(responseBytes, responseBytes.Length,
                        new IPEndPoint(ep.Address, SharedLibrary.Statics.Constants.CLIENT_PORT));
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