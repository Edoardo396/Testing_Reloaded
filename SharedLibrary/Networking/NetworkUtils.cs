using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Testing_Reloaded_Server.Exceptions;

namespace SharedLibrary.Networking {
    public static class NetworkUtils {
        public static async Task<MemoryStream> ReadNetworkBytes(NetworkStream network,
            long bytesToRead,
            long receiveSize = 1024) {
            var mStream = new MemoryStream();

            while (mStream.Length < bytesToRead) {
                byte[] buffer = new byte[receiveSize];
                int toBeRead = (int)(bytesToRead - mStream.Length > receiveSize ? receiveSize : bytesToRead - mStream.Length);

                int bRead = await network.ReadAsync(buffer, 0,(int) toBeRead);

                await mStream.WriteAsync(buffer, 0, bRead);
            }

            return mStream;
        }

        public static async Task<MemoryStream> ReadNetworkBytes(NetworkStream network) {
            var reader = new StreamReader(network, SharedLibrary.Statics.Constants.USED_ENCODING);

            var dataInfo = JObject.Parse(reader.ReadLine());

            int size = (int)dataInfo["Size"];

            return await ReadNetworkBytes(network, size);
        }

        public static async Task SendBytesToNetwork(NetworkStream network, MemoryStream bytes) {

            var sWriter = new StreamWriter(network);
            await sWriter.WriteLineAsync(Statics.Statics.GetJson(new { Status = "OK", Size = bytes.Length, FileType = "zip" }));
            await sWriter.FlushAsync();

            int sent = 0;
            int bufferL = SharedLibrary.Statics.Constants.DEFAULT_BUFFER;

            while (sent < bytes.Length) {
                byte[] buffer = new byte[1024];
                int toBeRead = (int)(bytes.Length - sent > bufferL ? bufferL : bytes.Length - sent);


                int read = await bytes.ReadAsync(buffer, 0, toBeRead);

                await network.WriteAsync(buffer, 0, read);

                sent += read;
            }

            await network.FlushAsync();
        }

        public static int GetAvailablePort(int startingPort) {
            var portArray = new List<int>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            // Ignore active connections
            var connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            // Ignore active tcp listners
            var endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            // Ignore active UDP listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (var i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }
    }
}