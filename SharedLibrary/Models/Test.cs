using System;

namespace SharedLibrary.Models {
    public class Test {
        public enum TestState {
            NotStarted = 0,
            Started = 1,
            OnHold = 2,
            Finished = 4
        }

        public string TestName { get; set; }
        public TimeSpan Time { get; set; }
        public string ClientTestPath { get; set; }
        public bool ReclaimTestImmediately { get; set; }
        public bool DeleteFilesAfterEnd { get; set; }
        public TestState State { get; set; }
    }
}