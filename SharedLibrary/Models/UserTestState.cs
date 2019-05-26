using System;

namespace SharedLibrary.Models {
    public class UserTestState {
        public enum UserState {
            Connected = 0,
            Waiting = 1,
            DownloadingDocs = 2,
            Testing = 3,
            OnHold = 4,
            Finished = 5,
            Crashed = 6
        }


        public TimeSpan RemainingTime { get; set; }
        public UserState State { get; set; }
    }
}