using System;
using System.Drawing;
using SharedLibrary.Models;

namespace SharedLibrary.Statics {
    public static class Extensions {
        public static Color UserStateToColor(this UserTestState.UserState s)
        {
            switch (s)
            {
                case UserTestState.UserState.Connected:
                    return Color.LightGreen;
                    break;
                case UserTestState.UserState.Waiting:
                    return Color.Yellow;
                    break;
                case UserTestState.UserState.DownloadingDocs:
                    return Color.LightGray;
                    break;
                case UserTestState.UserState.Testing:
                    return Color.LightGreen;
                    break;
                case UserTestState.UserState.OnHold:
                    return Color.Olive;
                    break;
                case UserTestState.UserState.Finished:
                    return Color.DeepSkyBlue;
                    break;
                case UserTestState.UserState.Crashed:
                    return Color.Red;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }
        }

        public static Color TestStateToColor(this Test.TestState s)
        {
            switch (s) {
                case Test.TestState.NotStarted:
                    return Color.Red;
                    break;
                case Test.TestState.Started:
                    return Color.Green;
                    break;
                case Test.TestState.OnHold:
                    return Color.Blue;
                    break;
                case Test.TestState.Finished:
                    return Color.DeepSkyBlue;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }
        }
    }
}