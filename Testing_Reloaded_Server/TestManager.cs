using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary;
using SharedLibrary.Models;
using Testing_Reloaded_Server.Models;
using Testing_Reloaded_Server.Networking;
using static SharedLibrary.Statics.Statics;

namespace Testing_Reloaded_Server {
    public class TestManager {
        private ServerTest currentTest;
        private ClientsManager clientsManager;

        public List<Client> ConnectedClients => clientsManager.Clients;
        public Test CurrentTest => currentTest;

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

            if (!Directory.Exists(test.HandoverDirectory)) {
                Directory.CreateDirectory(test.HandoverDirectory);
            }

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

                clientsManager.SendBytes(c, DocumentationZip).Wait();

                return JsonConvert.SerializeObject(new {Status = "OK"});
            }

            if (message["Action"].ToString() == "StateUpdate") {
                c.TestState = JsonConvert.DeserializeObject<UserTestState>(message["State"].ToString());
            }

            if (message["Action"].ToString() == "TestHandover") {
                return GetUserTest(c);
            }

            ClientStatusUpdated?.Invoke(c);

            return null;
        }

        private string GetUserTest(Client c) {
            var stream = c.DataConnection.GetStream();
            var sReader = new StreamReader(stream, SharedLibrary.Statics.Constants.USED_ENCODING);
            var dataInfo = JObject.Parse(sReader.ReadLine());

            int size = (int) dataInfo["Size"];


            stream.ReadTimeout = 5000;

            MemoryStream memoryStream = null;

            try {
                memoryStream = SharedLibrary.Networking.NetworkUtils
                    .ReadNetworkBytes(stream, size, c.DataConnection.ReceiveBufferSize)
                    .Result;

                var fastZip = new FastZip();

                fastZip.ExtractZip(memoryStream, Path.Combine(currentTest.HandoverDirectory, c.ToString()),
                    FastZip.Overwrite.Always, null, null, null, true, true);

                return JsonConvert.SerializeObject(new {Status = "OK"});
            } catch (Exception e) {
                return JsonConvert.SerializeObject(new {Status = "Error", ErrorCode = "HNDFAIL", Message = e.Message});
            }
        }

        public async Task StartTest() {
            currentTest.State = Test.TestState.Started;
            await clientsManager.SendMessageToClients(JsonConvert.SerializeObject(new {Action = "TestStarted"}));
        }

        public async Task SetTestState(Test.TestState state) {
            currentTest.State = state;
            await clientsManager.SendControlMessageToClients(
                JsonConvert.SerializeObject(new {Action = "UpdateTest", Test = currentTest}), c => true);
        }

        public async Task SyncClient(Test.TestState state, Predicate<Client> clients) {
            foreach (var client in ConnectedClients.Where(clients.Invoke)) {
                await clientsManager.SendControlMessageToClient(
                    GetJson(new {Action = "Sync", State = client.TestState}), client);
            }
        }

        public async Task AddTime(TimeSpan span, Predicate<Client> predicate) {
            await clientsManager.SendControlMessageToClients(GetJson(new {Action = "AddTime", TimeSpan = span}),
                predicate);
        }

        public async Task ToggleStateForClients(Predicate<Client> predicate) {
            foreach (var client in ConnectedClients.Where(predicate.Invoke)) {
                if (client.TestState.State != UserTestState.UserState.OnHold &&
                    client.TestState.State != UserTestState.UserState.Testing) continue;

                if (client.TestState.State == UserTestState.UserState.Testing) {
                    await clientsManager.SendControlMessageToClient(GetJson(new {Action = "Pause"}), client);
                    client.TestState.State = UserTestState.UserState.OnHold;
                } else if (client.TestState.State == UserTestState.UserState.OnHold) {
                    await clientsManager.SendControlMessageToClient(GetJson(new {Action = "Resume"}), client);
                    client.TestState.State = UserTestState.UserState.Testing;
                }

                ClientStatusUpdated?.Invoke(client);
            }
        }
    }
}