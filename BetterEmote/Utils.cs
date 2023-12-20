using GameNetcodeStuff;

namespace BetterEmote
{
    internal class Utils
    {
        public static PlayerControllerB localPlayerController
        {
            get
            {
                StartOfRound instance = StartOfRound.Instance;
                return (instance != null) ? instance.localPlayerController : null;
            }
        }
        public static bool localPlayerUsingController
        {
            get
            {
                StartOfRound instance = StartOfRound.Instance;
                return (instance != null) ? instance.localPlayerUsingController : false;
            }
        }
    }
}
