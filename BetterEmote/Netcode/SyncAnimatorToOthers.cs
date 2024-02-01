using GameNetcodeStuff;
using Unity.Netcode;

namespace BetterEmote.Netcode
{
    public class SyncAnimatorToOthers : NetworkBehaviour
    {
        private PlayerControllerB playerInstance;

        private void Start()
        {
            playerInstance = GetComponent<PlayerControllerB>();
        }

        public void UpdateEmoteIDForOthers(int newID)
        {
            Plugin.Debug($"UpdateEmoteIDForOthers({newID}, {playerInstance.IsOwner}, {playerInstance.isPlayerControlled})");
            if (playerInstance.IsOwner && playerInstance.isPlayerControlled)
            {
                UpdateCurrentEmoteIDServerRpc(newID);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateCurrentEmoteIDServerRpc(int newID)
        {
            Plugin.Debug($"UpdateCurrentEmoteIDServerRpc({newID})");
            UpdateCurrentEmoteIDClientRpc(newID);
        }

        [ClientRpc]
        private void UpdateCurrentEmoteIDClientRpc(int newID)
        {
            Plugin.Debug($"UpdateCurrentEmoteIDClientRpc({newID}, {playerInstance.IsOwner}, {playerInstance.isPlayerControlled})");
            if (!playerInstance.IsOwner || !playerInstance.isPlayerControlled)
            {
                playerInstance.playerBodyAnimator.SetInteger("emoteNumber", newID);
            }
        }
    }
}
