using BetterEmote.Utils;
using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;

namespace BetterEmote.Netcode
{
    public class SyncVRState : NetworkBehaviour
    {
        private PlayerControllerB playerInstance;

        public static Dictionary<ulong, bool> vrPlayers = new Dictionary<ulong, bool>();

        private void Start()
        {
            playerInstance = GetComponent<PlayerControllerB>();
        }

        public void UpdateVRStateForOthers(bool isVR)
        {
            Plugin.Debug($"UpdateVRStatusForOthers({isVR}, {playerInstance.playerClientId}, {playerInstance.IsOwner}, {playerInstance.isPlayerControlled})");
            if (playerInstance != null && playerInstance.IsOwner && playerInstance.isPlayerControlled)
            {
                UpdateVRStateServerRpc(isVR, playerInstance.playerClientId);
            }
        }

        public void RequestVRStateFromOthers()
        {
            Plugin.Debug($"RequestVRStateFromOthers({playerInstance.IsOwner}, {playerInstance.isPlayerControlled})");
            if (playerInstance != null && playerInstance.IsOwner && playerInstance.isPlayerControlled)
            {
                RequestedVRStateServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestedVRStateServerRpc()
        {
            Plugin.Debug($"RequestedVRState()");
            RequestedVRStateClientRpc();
        }

        [ClientRpc]
        private void RequestedVRStateClientRpc()
        {
            Plugin.Debug($"RequestedVRStateClientRpc({Settings.DisableModelOverride}, {GameValues.localPlayerController.playerClientId})");
            if (playerInstance != null && !playerInstance.IsOwner)
            {
                UpdateVRStateServerRpc(Settings.DisableModelOverride, GameValues.localPlayerController.playerClientId); // Big man Smoku Broku <3
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateVRStateServerRpc(bool isVR, ulong clientId)
        {
            Plugin.Debug($"UpdateVRStateServerRpc({isVR}, {clientId})");
            UpdateVRStateClientRpc(isVR, clientId);
        }

        [ClientRpc]
        private void UpdateVRStateClientRpc(bool isVRChange, ulong clientId)
        {
            Plugin.Debug($"UpdateVRStateClientRpc({isVRChange}, {clientId})");
            vrPlayers[clientId] = isVRChange;
        }
    }
}
