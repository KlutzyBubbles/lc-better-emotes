using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;

namespace BetterEmote.AssetScripts
{
    public class SignEmoteText : NetworkBehaviour
    {
        private PlayerControllerB _playerInstance;

        private TextMeshPro _signModelText;

        public string Text
        {
            get
            {
                return _signModelText.text;
            }
        }

        private void Start()
        {
            Plugin.Debug("Start()");
            _playerInstance = GetComponent<PlayerControllerB>();
            _signModelText = _playerInstance.transform.Find("ScavengerModel").Find("metarig").Find("Sign").Find("Text").GetComponent<TextMeshPro>();
        }

        public void UpdateSignText(string newText)
        {
            Plugin.Debug($"UpdateSignText({newText})");
            if (_playerInstance.IsOwner && _playerInstance.isPlayerControlled)
            {
                UpdateSignTextServerRpc(newText);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateSignTextServerRpc(string newText)
        {
            Plugin.Debug($"UpdateSignTextServerRpc({newText})");
            UpdateSignTextClientRpc(newText);
        }

        [ClientRpc]
        private void UpdateSignTextClientRpc(string newText)
        {
            Plugin.Debug($"UpdateSignTextClientRpc({newText})");
            _signModelText.text = newText;
        }
    }
}
