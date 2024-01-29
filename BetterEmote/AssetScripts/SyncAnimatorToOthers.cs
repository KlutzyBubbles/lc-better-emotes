using GameNetcodeStuff;
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
            Plugin.Debug($"UpdateEmoteIDForOthers({newID}, {_player.playerClientId}, {_player.IsHost}, {_player.IsClient}, {_player.actualClientId})");
            if (_player.IsOwner && _player.isPlayerControlled)
            {
                UpdateCurrentEmoteIDServerRpc(newID);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateCurrentEmoteIDServerRpc(int newID)
        {
            Plugin.Debug($"UpdateCurrentEmoteIDServerRpc({newID}, {_player.playerClientId}, {_player.IsHost}, {_player.IsClient}, {_player.actualClientId})");
            UpdateCurrentEmoteIDClientRpc(newID);
        }

        [ClientRpc]
        private void UpdateCurrentEmoteIDClientRpc(int newID)
        {
            Plugin.Debug($"UpdateCurrentEmoteIDClientRpc({newID}, {_player.playerClientId}, {_player.IsHost}, {_player.IsClient}, {_player.actualClientId})");
            if (!_player.IsOwner || !_player.isPlayerControlled)
            {
                _player.playerBodyAnimator.SetInteger("emoteNumber", newID);
            }
        }
    }
}
