using System;
using System.Text;

namespace SharedLibrary.Statics {
    public static class Constants {
        public const int SERVER_PORT = 45230;

        public const int BROADCAST_PORT_CLIENT = 55230;
        public const int BROADCAST_PORT_SERVER = 55231;

        public const int DEFAULT_BUFFER = 1024;

        public const int SOCKET_TIMEOUT = 50000;
        public static Encoding USED_ENCODING = Encoding.UTF8;

        public const int POLL_TIME = 30;
        public const int CRASH_DECLARED_TIME = 60;

        public static Version APPLICATION_VERSION = new Version(0, 5, 0);
        public static bool DEBUG = true;
    }
}