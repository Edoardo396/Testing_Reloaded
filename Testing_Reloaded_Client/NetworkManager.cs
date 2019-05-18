using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Testing_Reloaded_Client {
    public class NetworkManager {
        private TcpClient tcpConnection;
        private Server currentServer;


        private NetworkStream networkStream;
        private StreamReader netReader;
        private StreamWriter netWriter;

        public bool Connected => netReader != null && tcpConnection.Connected;

        public NetworkManager(Server server) {
            this.currentServer = server;
        }

        public async Task ConnectToServer() {
            tcpConnection = new TcpClient(AddressFamily.InterNetwork);
            await tcpConnection.ConnectAsync(currentServer.IP, SharedLibrary.Constants.SERVER_PORT); // try catch
            Thread.Sleep(1000);

            networkStream = tcpConnection.GetStream();
            netReader = new StreamReader(networkStream, SharedLibrary.Constants.USED_ENCODING);
            netWriter = new StreamWriter(networkStream, SharedLibrary.Constants.USED_ENCODING);
        }

        public async Task WriteLine(string data) {
            await netWriter.WriteLineAsync(data);
            await netWriter.FlushAsync();
        }

        public async Task<string> ReadLine() {
            return await netReader.ReadLineAsync();
        }

        public async Task<byte[]> ReadData() {
            // get data info
            var strData = await this.ReadLine();
            var dataInfo = JObject.Parse(strData);
            int size = (int) dataInfo["Size"];

            if (size == 0) return null;

            var bytes = new byte[size];
            var bReader = new BinaryReader(networkStream, Encoding.Default);

            bytes = bReader.ReadBytes(bytes.Length);

            return bytes;
        }
    }
}