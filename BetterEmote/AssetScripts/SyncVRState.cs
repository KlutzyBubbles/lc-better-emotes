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
            Plugin.Debug($"UpdateVRStatusForOthers({isVR}, {_player.name}, {_player.playerSteamId}, {_player.playerUsername}, {_player.playerClientId}, {_player.actualClientId}, {_player.IsHost}, {_player.IsOwner})");
            if (_player.IsOwner && _player.isPlayerControlled)
            {
                UpdateVRStateServerRpc(isVR, _player.playerClientId);
            }
        }

        public void RequestVRStateFromOthers()
        {
            Plugin.Debug($"RequestVRStateFromOthers({_player.name}, {_player.playerSteamId}, {_player.playerUsername}, {_player.playerClientId}, {_player.actualClientId}, {_player.IsHost}, {_player.IsOwner})");
            if (_player.IsOwner && _player.isPlayerControlled)
            {
                RequestedVRStateServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestedVRStateServerRpc()
        {
            Plugin.Debug($"RequestedVRState({_player.name}, {_player.playerSteamId}, {_player.playerUsername}, {_player.playerClientId}, {_player.actualClientId}, {_player.IsHost}, {_player.IsOwner})");
            if (!_player.IsOwner)
            {
                RequestedVRStateClientRpc();
            }
        }

        [ClientRpc]
        private void RequestedVRStateClientRpc()
        {
            Plugin.Debug($"RequestedVRStateClientRpc({_player.name}, {_player.playerSteamId}, {_player.playerUsername}, {_player.playerClientId}, {_player.actualClientId}, {_player.IsHost}, {_player.IsOwner})");
            UpdateVRStateServerRpc(Settings.disableSelfEmote, _player.playerClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateVRStateServerRpc(bool isVR, ulong clientId)
        {
            Plugin.Debug($"UpdateVRStateServerRpc({isVR}, {_player.name}, {_player.playerSteamId}, {_player.playerUsername}, {_player.playerClientId}, {_player.actualClientId}, {_player.IsHost}, {_player.IsOwner})");
            UpdateVRStateClientRpc(isVR, clientId);
        }

        [ClientRpc]
        private void UpdateVRStateClientRpc(bool isVRChange, ulong clientId)
        {
            Plugin.Debug($"UpdateVRStateClientRpc({isVRChange}, {_player.name}, {_player.playerSteamId}, {_player.playerUsername}, {_player.playerClientId}, {_player.actualClientId}, {_player.IsHost}, {_player.IsOwner}, {_player.isPlayerControlled})");
            //if (!_player.IsOwner)
            //{
                //Plugin.Debug($"Player not owner or controlled");
                if (isVRChange)
                {
                    if (!vrPlayers.Contains(clientId))
                    {
                        Plugin.Debug($"vr players not contained");
                        vrPlayers.Add(clientId);
                    }
                } else
                {
                    if (vrPlayers.Contains(_player.playerClientId))
                    {
                        Plugin.Debug($"vr players contained");
                        vrPlayers.Remove(_player.playerClientId);
                    }
                }
                //isVR = isVRChange;
            //}
        }
    }
}
