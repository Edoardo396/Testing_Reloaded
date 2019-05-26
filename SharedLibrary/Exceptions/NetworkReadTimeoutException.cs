using System;
using System.Net.Sockets;

namespace Testing_Reloaded_Server.Exceptions {
    public class NetworkReadTimeoutException : TimeoutException {
        public NetworkReadTimeoutException(NetworkStream stream) {
            Stream = stream;
        }

        public NetworkStream Stream { get; set; }
    }
}