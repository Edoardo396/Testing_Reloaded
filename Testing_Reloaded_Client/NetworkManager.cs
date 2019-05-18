﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
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

            networkStream = tcpConnection.GetStream();
            netReader = new StreamReader(networkStream);
            netWriter = new StreamWriter(networkStream);
        }

        public async Task WriteLine(string data) {
            await netWriter.WriteLineAsync(data);
        }

        public async Task<string> ReadLine() {
            return await netReader.ReadLineAsync();
        }

        public async Task<byte[]> ReadData() {
            // get data info
            var dataInfo = JObject.Parse(await ReadLine());
            int size = (int) dataInfo["Size"];

            if (size == 0) return null;

            var bytes = new byte[size];
            var bReader = new BinaryReader(networkStream, Encoding.Default);

            bytes = bReader.ReadBytes(bytes.Length);

            return bytes;
        }
    }
}