using System;

namespace Testing_Reloaded_Server.Exceptions {
    public class VersionMismatchException : Exception {

        public Version ServerVersion { get; set; }
        public Version ClientVersion { get; set; }

        public VersionMismatchException() : base() {
            
        }

        public VersionMismatchException(string message) : base(message) {
            
        }

    }
}