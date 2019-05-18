using SharedLibrary;
using Testing_Reloaded_Server.Networking;

namespace Testing_Reloaded_Server {
    public class TestManager {

        private Test currentTest;
        private ClientsManager clientsManager;
        

        public TestManager(Test test) {
            this.currentTest = test;
            clientsManager = new ClientsManager();
        }








    }
}