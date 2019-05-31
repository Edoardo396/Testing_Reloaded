using System;
using System.Net;
using System.Net.Sockets;
using SharedLibrary.Models;

namespace Testing_Reloaded_Server.Models {
    public class Client : User {


        public int Id { get; set; }
        public TcpClient ControlConnection { get; set; }
        public TcpClient DataConnection { get; set; }
        public UserTestState TestState { get; set; }

        public Version ClientAppVersion { get; set; }

        public IPAddress IP => (DataConnection.Client.RemoteEndPoint as IPEndPoint)?.Address;

        public Client(int id, User user) : base(user.Name, user.Surname, user.PCHostname) {
            this.Id = id;
        }
    }
}