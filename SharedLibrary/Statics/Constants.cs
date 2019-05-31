using System;
using System.Text;

namespace SharedLibrary.Statics {
    public static class Constants {
        public static int SERVER_PORT = 45230;
        public static int CLIENT_PORT = 45231;


        public static int BROADCAST_PORT_CLIENT = 55230;
        public static int BROADCAST_PORT_SERVER = 55231;

        public static int DEFAULT_BUFFER = 1024;

        public static int SOCKET_TIMEOUT = 50000;
        public static Encoding USED_ENCODING = Encoding.UTF8;

        public static Version APPLICATION_VERSION = new Version(0, 5, 0);
    }
}