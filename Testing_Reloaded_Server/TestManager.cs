using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary;
using Testing_Reloaded_Server.Networking;

namespace Testing_Reloaded_Server {
    public class TestManager {

        private Test currentTest;
        private ClientsManager clientsManager;
        

        public TestManager(Test test) {
            this.currentTest = test;
            clientsManager = new ClientsManager();
            clientsManager.ReceivedMessageFromClient += ClientsManagerOnReceivedMessageFromClient;

            clientsManager.Start();
        }

        private string ClientsManagerOnReceivedMessageFromClient(Client c, JObject message) {

            if (message["Action"].ToString() == "GetTestInfo") {
                return JsonConvert.SerializeObject(new {Status = "OK", Test = currentTest});
            }


            return null;
        }
    }
}