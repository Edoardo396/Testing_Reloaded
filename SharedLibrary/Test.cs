using System;

namespace SharedLibrary {
    public class Test {
        public string TestName { get; set; }
        public TimeSpan Time { get; set; }
        public string DataDownloadPath { get; set; }
        public bool ReclaimTestImmediately { get; set; }
        public bool DeleteFilesAfterEnd { get; set; }

    }
}