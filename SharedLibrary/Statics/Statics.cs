using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary.Models;

namespace SharedLibrary.Statics {
    public class Statics {

        private static Random rand;

        public static TimeSpan ApplicationTime => TimeSpan.FromTicks((DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).Ticks);

        public static int GenerateRandomPacketId() {
            if (rand == null) rand = new Random(41);

            return rand.Next(1, Int32.MaxValue);
        }

        public static string GetJson(object obj) {
            return JsonConvert.SerializeObject(obj);
        }

        public static UserTestState.UserState MapDefaultTestState(Test.TestState s) {
            switch (s) {
                case Test.TestState.NotStarted:
                    return UserTestState.UserState.Waiting;
                    
                case Test.TestState.Started:
                    return UserTestState.UserState.Testing;
                    
                case Test.TestState.OnHold:
                    return UserTestState.UserState.OnHold;
                    case Test.TestState.Finished:
                    return UserTestState.UserState.Finished;

                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }
        }
    }
}