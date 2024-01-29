using BetterEmote.Utils;
using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;

namespace BetterEmote.AssetScripts
{
    public class SyncVRState : NetworkBehaviour
    {
        private PlayerControllerB _player;
        //public bool isVR;
        public List<ulong> vrPlayers = new List<ulong>();

        private void Start()
        {
            _player = GetComponent<PlayerControllerB>();
            //isVR = false;
        }

        public void UpdateVRStateForOthers(bool isVR)
        {
            Plugin.Debug($"UpdateVRStatusForOthers({isVR})");
            if (_player.IsOwner && _player.isPlayerControlled)
            {
                UpdateVRStateServerRpc(isVR);
            }
        }

        public void RequestVRStateFromOthers()
        {
            Plugin.Debug($"RequestVRStateFromOthers()");
            if (_player.IsOwner && _player.isPlayerControlled)
            {
                RequestedVRStateServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestedVRStateServerRpc()
        {
            Plugin.Debug($"RequestedVRState()");
            if (!_player.IsOwner)
            {
                RequestedVRStateClientRpc();
            }
        }

        [ClientRpc]
        private void RequestedVRStateClientRpc()
        {
            Plugin.Debug($"RequestedVRStateClientRpc()");
            UpdateVRStateServerRpc(Settings.disableSelfEmote);
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateVRStateServerRpc(bool isVR)
        {
            Plugin.Debug($"UpdateVRStateServerRpc({isVR})");
            UpdateVRStateClientRpc(isVR);
        }

        [ClientRpc]
        private void UpdateVRStateClientRpc(bool isVRChange)
        {
            Plugin.Debug($"UpdateVRStateClientRpc({isVRChange})");
            if (!_player.IsOwner)
            {
                if (isVRChange)
                {
                    if (!vrPlayers.Contains(_player.playerClientId))
                    {
                        vrPlayers.Add(_player.playerClientId);
                    }
                } else
                {
                    if (vrPlayers.Contains(_player.playerClientId))
                    {
                        vrPlayers.Remove(_player.playerClientId);
                    }
                }
                //isVR = isVRChange;
            }
        }
    }
}
