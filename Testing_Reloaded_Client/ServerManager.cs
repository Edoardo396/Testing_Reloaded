using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Testing_Reloaded_Client {
    public class ServerManager {

        private List<Server> foundServers;

        public List<Server> Servers => foundServers;

        public ServerManager() {
            foundServers = new List<Server>();
        }

        public async Task GetServers() {

            var client = new UdpClient(SharedLibrary.Constants.CLIENT_PORT);
            client.Client.ReceiveTimeout = 5000;

            var json = SharedLibrary.Constants.USED_ENCODING.GetBytes(JsonConvert.SerializeObject(new { Action = "discover" }));

            client.Send(json, json.Length, new IPEndPoint(IPAddress.Broadcast, SharedLibrary.Constants.SERVER_PORT));

            

            await Task.WhenAny(Task.Run(async () => {

                while (true) {
                    var received = await client.ReceiveAsync();

                    var jobj = JObject.Parse(SharedLibrary.Constants.USED_ENCODING.GetString(received.Buffer));

                    foundServers.Add(new Server() {
                        Hostname = jobj["Hostname"].ToString(),
                        IP = received.RemoteEndPoint.Address
                    });
                   
                }

            }), Task.Delay(5000));
        }
    }
}