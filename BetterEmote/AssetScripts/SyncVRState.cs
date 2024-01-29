using BetterEmote.Patches;
using BetterEmote.Utils;
using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;

namespace BetterEmote.AssetScripts
{
    public class SyncVRState : NetworkBehaviour
    {
        private PlayerControllerB _player;

        private void Start()
        {
            _player = GetComponent<PlayerControllerB>();
        }

        public void UpdateVRStateForOthers(bool isVR)
        {
            Plugin.Debug($"UpdateVRStatusForOthers({isVR}, {_player.playerClientId}, {_player.IsOwner}, {_player.isPlayerControlled})");
            if (_player.IsOwner && _player.isPlayerControlled)
            {
                UpdateVRStateServerRpc(isVR, _player.playerClientId);
            }
        }

        public void RequestVRStateFromOthers()
        {
            Plugin.Debug($"RequestVRStateFromOthers({_player.IsOwner}, {_player.isPlayerControlled})");
            if (_player.IsOwner && _player.isPlayerControlled)
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
            Plugin.Debug($"RequestedVRStateClientRpc({Settings.disableSelfEmote}, {GameValues.localPlayerController.playerClientId})");
            if (!_player.IsOwner)
            {
                UpdateVRStateServerRpc(Settings.disableSelfEmote, GameValues.localPlayerController.playerClientId); // Big man Smoku Broku <3
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
            EmotePatch.vrPlayers[clientId] = isVRChange;
        }
    }
}
