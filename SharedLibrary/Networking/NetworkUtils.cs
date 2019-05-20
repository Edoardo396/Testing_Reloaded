using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SharedLibrary.Networking {
    public static class NetworkUtils {

        public static async Task<MemoryStream> ReadNetworkBytes(NetworkStream network, long bytesToRead, long receiveSize = 1024) {
            var mStream = new MemoryStream();

            while (mStream.Length < bytesToRead) {
                
                byte[] buffer = new byte[receiveSize];
                int bRead = network.Read(buffer, 0, (int)receiveSize);

                await mStream.WriteAsync(buffer, 0, bRead);
            }

            return mStream;
        }

        
    }
}