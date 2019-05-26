using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary.Statics;

namespace Testing_Reloaded_Client.Networking {
    public class ServerManager {
        public ServerManager() {
            Servers = new List<Server>();
        }

        public List<Server> Servers { get; }

        public async Task GetServers() {
            var client = new UdpClient(new IPEndPoint(IPAddress.Any, Constants.BROADCAST_PORT_CLIENT));

            var json = Constants.USED_ENCODING.GetBytes(JsonConvert.SerializeObject(new {Action = "discover"}));

            client.Send(json, json.Length, new IPEndPoint(IPAddress.Broadcast, Constants.BROADCAST_PORT_SERVER));


            await Task.WhenAny(Task.Run(async () => {
                while (true) {
                    var received = await client.ReceiveAsync();

                    var jobj = JObject.Parse(Constants.USED_ENCODING.GetString(received.Buffer));

                    Servers.Clear();

                    Servers.Add(new Server {
                        Hostname = jobj["Hostname"].ToString(),
                        IP = received.RemoteEndPoint.Address
                    });
                }
            }), Task.Delay(2000));

            client.Close();
        }
    }
}