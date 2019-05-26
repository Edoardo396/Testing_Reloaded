using System;
using System.Net.Sockets;

namespace Testing_Reloaded_Server.Exceptions {
    public class NetworkReadTimeoutException : TimeoutException {

        public NetworkStream Stream { get; set; }

        public NetworkReadTimeoutException(NetworkStream stream) {
            this.Stream = stream;
        }
    }
}