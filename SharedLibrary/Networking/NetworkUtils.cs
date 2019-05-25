using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Testing_Reloaded_Server.Exceptions;

namespace SharedLibrary.Networking {
    public static class NetworkUtils {

        public static async Task<MemoryStream> ReadNetworkBytes(NetworkStream network, long bytesToRead, long receiveSize = 1024) {
            var mStream = new MemoryStream();

            while (mStream.Length < bytesToRead) {
                
                byte[] buffer = new byte[receiveSize];
                int bRead;

                    bRead = network.Read(buffer, 0,
                        bytesToRead - mStream.Length > (int) receiveSize
                            ? (int) receiveSize
                            : (int) (bytesToRead - mStream.Length));
   
                await mStream.WriteAsync(buffer, 0, bRead);
            }

            return mStream;
        }

        
    }
}