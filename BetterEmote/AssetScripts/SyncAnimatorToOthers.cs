using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace BetterEmote.AssetScripts
{
    public class SyncAnimatorToOthers : NetworkBehaviour
    {
        private PlayerControllerB _player;

        private void Start()
        {
            _player = GetComponent<PlayerControllerB>();
        }

        public void UpdateEmoteIDForOthers(int newID)
        {
            if (_player.IsOwner && _player.isPlayerControlled)
            {
                UpdateCurrentEmoteIDServerRpc(newID);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateCurrentEmoteIDServerRpc(int newID)
        {
            UpdateCurrentEmoteIDClientRpc(newID);
        }

        [ClientRpc]
        private void UpdateCurrentEmoteIDClientRpc(int newID)
        {
            if (!_player.IsOwner)
            {
                _player.playerBodyAnimator.SetInteger("emoteNumber", newID);
            }
        }
    }
}
