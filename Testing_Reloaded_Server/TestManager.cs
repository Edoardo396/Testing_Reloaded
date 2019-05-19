using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary;
using Testing_Reloaded_Server.Networking;

namespace Testing_Reloaded_Server {
    public class TestManager {
        private ServerTest currentTest;
        private ClientsManager clientsManager;

        public BindingList<Client> ConnectedClients => clientsManager.Clients;

        public delegate void ClientStatusUpdatedDelegate(Client c);
        public event ClientStatusUpdatedDelegate ClientStatusUpdated;


        private byte[] documentationZip;

        private byte[] DocumentationZip {
            get {
                if (documentationZip != null) return documentationZip;

                var stream = new MemoryStream();
                var zip = new FastZip();

                zip.CreateZip(stream, currentTest.DocumentationDirectory, true, null, null);

                return stream.ToArray();
            }
        }


        public TestManager(ServerTest test) {
            this.currentTest = test;
            clientsManager = new ClientsManager();
            clientsManager.ReceivedMessageFromClient += ClientsManagerOnReceivedMessageFromClient;

            clientsManager.Start();
        }

        private string ClientsManagerOnReceivedMessageFromClient(Client c, JObject message) {
            if (message["Action"].ToString() == "GetTestInfo") {
                return JsonConvert.SerializeObject(new {Status = "OK", Test = currentTest as Test});
            }

            if (message["Action"].ToString() == "GetTestDocs") {
                if (currentTest.State == Test.TestState.NotStarted)
                    return JsonConvert.SerializeObject(new
                        {Status = "ERROR", Code = "TSTNSTART", Message = "Test is not started, cannot get docs"});

                c.TestState.State = UserTestState.UserState.DownloadingDocs;

                if (string.IsNullOrEmpty(currentTest.DocumentationDirectory))
                    return JsonConvert.SerializeObject(new {Status = "OK", FileType = "nodata", Size = 0});

                clientsManager.SendBytes(c, DocumentationZip);

                return JsonConvert.SerializeObject(new {Status = "OK"});
               
            }

            if (message["Action"].ToString() == "StateUpdate") {
                c.TestState = JsonConvert.DeserializeObject<UserTestState>(message["State"].ToString());
            }

            ClientStatusUpdated?.Invoke(c);

            return null;
        }

        public async Task StartTest() {
            currentTest.State = Test.TestState.Started;
            await clientsManager.SendMessageToClients(JsonConvert.SerializeObject(new {Action = "TestStarted"}), false);
        }
    }
}