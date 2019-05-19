using System;
using System.Drawing;

namespace SharedLibrary {
    public class UserTestState {

        public enum UserState {
            Connected,
            Waiting,
            DownloadingDocs,
            Testing,
            OnHold,
            Finished,
            Crashed
        }

       

        public TimeSpan RemainingTime { get; set; }
        public UserState State { get; set; }
    }
}