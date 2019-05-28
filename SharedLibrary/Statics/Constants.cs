using System.Text;

namespace SharedLibrary.Statics {
    public static class Constants {
        public static int SERVER_PORT = 45230;
        public static int CLIENT_PORT = 45231;


        public static int BROADCAST_PORT_CLIENT = 55230;
        public static int BROADCAST_PORT_SERVER = 55231;

        public static int SOCKET_TIMEOUT = 5000;
        public static Encoding USED_ENCODING = Encoding.UTF8;
    }
}