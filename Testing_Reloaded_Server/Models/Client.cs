using System.Net;
using System.Net.Sockets;
using SharedLibrary.Models;

namespace Testing_Reloaded_Server.Models {
    public class Client : User {


        public int Id { get; set; }
        public TcpClient TcpClient { get; set; }
        public UserTestState TestState { get; set; }

        public IPAddress IP => (TcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address;

        public Client(int id, User user, TcpClient client) : base(user.Name, user.Surname, user.PCHostname) {
            this.TcpClient = client;
            this.Id = id;
        }
    }
}