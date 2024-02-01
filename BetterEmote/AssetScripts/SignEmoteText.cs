using GameNetcodeStuff;
using TMPro;
using Unity.Netcode;

namespace BetterEmote.AssetScripts
{
    public class SignEmoteText : NetworkBehaviour
    {
        private PlayerControllerB playerInstance;

        private TextMeshPro signModelText;

        private void Start()
        {
            Plugin.Debug("Start()");
            playerInstance = GetComponent<PlayerControllerB>();
            signModelText = playerInstance?.transform?.Find("ScavengerModel")?.Find("metarig")?.Find("Sign")?.Find("Text")?.GetComponent<TextMeshPro>();
        }

        public void UpdateSignText(string newText)
        {
            Plugin.Debug($"UpdateSignText({newText})");
            if (playerInstance != null && playerInstance.IsOwner && playerInstance.isPlayerControlled)
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
            if (signModelText != null)
            {
                signModelText.text = newText;
            }
        }
    }
}
