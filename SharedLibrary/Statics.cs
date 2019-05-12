using System;

namespace SharedLibrary {
    public class Statics {

        private static Random rand;

        public static int GenerateRandomPacketId() {
            if(rand == null) rand = new Random(41);

            return rand.Next(1, Int32.MaxValue);
        }
    }
}