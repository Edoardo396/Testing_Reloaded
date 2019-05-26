using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharedLibrary.Statics {
    public class Statics {
        private static Random rand;

        public static int GenerateRandomPacketId() {
            if (rand == null) rand = new Random(41);

            return rand.Next(1, Int32.MaxValue);
        }

        public static string GetJson(object obj) {
            return JsonConvert.SerializeObject(obj);
        }
    }
}