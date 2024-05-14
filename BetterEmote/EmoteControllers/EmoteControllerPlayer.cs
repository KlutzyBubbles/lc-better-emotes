using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BetterEmote
{
    public class EmoteControllerPlayer : EmoteController
    {
        public static PlayerControllerB localPlayerController { get { return StartOfRound.Instance?.localPlayerController; } }
        public static Dictionary<PlayerControllerB, EmoteControllerPlayer> allPlayerEmoteControllers = new Dictionary<PlayerControllerB, EmoteControllerPlayer>();
        public static EmoteControllerPlayer emoteControllerLocal { get { return localPlayerController != null && allPlayerEmoteControllers.ContainsKey(localPlayerController) ? allPlayerEmoteControllers[localPlayerController] : null; } }
        public static int emoteStateHash { get { return localPlayerController != null ? Animator.StringToHash(localPlayerController.playerBodyAnimator.GetLayerName(1) + ".Dance1") : -1; } }

        public PlayerControllerB playerController;
        public bool isLocalPlayer { get { return playerController == StartOfRound.Instance?.localPlayerController; } }
        public ulong clientId { get { return playerController.actualClientId; } }
        public ulong playerId { get { return playerController.playerClientId; } }
        public ulong steamId { get { return playerController.playerSteamId; } }
        public string username { get { return playerController.playerUsername; } }

        public float timeSinceStartingEmote { get { return (float)Traverse.Create(playerController).Field("timeSinceStartingEmote").GetValue(); } set { Traverse.Create(playerController).Field("timeSinceStartingEmote").SetValue(value); } }


        protected override void Awake()
        {
            base.Awake();
            if (!initialized)
                return;

            try
            {
                playerController = GetComponentInParent<PlayerControllerB>();
                if (playerController == null)
                {
                    Plugin.Logger.LogError("Failed to find PlayerControllerB component in parent of EmoteControllerPlayer.");
                    return;
                }
                allPlayerEmoteControllers.Add(playerController, this);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to initialize EmoteControllerPlayer: " + playerController.name + ". Error: " + e);
            }
        }


        protected override void Start()
        {
            base.Start();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            allPlayerEmoteControllers?.Remove(playerController);
        }


        protected override void AddGroundContactPoints()
        {
            base.AddGroundContactPoints();
        }


        protected override void Update()
        {
            if (!initialized || playerController == null || (playerController == localPlayerController /*&& (ConfigSettings.disableEmotesForSelf.Value || LCVR_Patcher.Enabled)*/))
                return;
            base.Update();
        }


        protected override void LateUpdate()
        {
            if (!initialized || playerController == null || (playerController == localPlayerController /*&& (ConfigSettings.disableEmotesForSelf.Value || LCVR_Patcher.Enabled)*/))
                return;

            bool isEmoting = isPerformingEmote;
            base.LateUpdate();

            if (isEmoting && !isPerformingEmote && playerController.performingEmote)
            {
                playerController.performingEmote = false;
                originalAnimator.SetInteger("emoteNumber", 0);
                var currentStateInfo = originalAnimator.GetCurrentAnimatorStateInfo(0);
                animator.Play(currentStateInfo.fullPathHash, 0, 0);
                if (isLocalPlayer)
                {
                    timeSinceStartingEmote = 0f;
                    playerController.StopPerformingEmoteServerRpc();
                }
            }
        }


        protected override void TranslateAnimation()
        {
            if (!initialized || playerController == null || !playerController.performingEmote)
                return;
            base.TranslateAnimation();
        }


        protected override bool CheckIfShouldStopEmoting()
        {
            if (playerController == null || !isPerformingEmote)
                return false;
            return base.CheckIfShouldStopEmoting() || !playerController.performingEmote /*|| performingEmote == null*/;
        }


        public override bool IsPerformingCustomEmote()
        {
            return base.IsPerformingCustomEmote() && playerController.performingEmote;
        }


        public void TryPerformingEmoteLocal(/* UnlockableEmote emote */AnimationClip clip)
        {
            Plugin.Debug("EmoteControllerPlayer.TryPerformingEmoteLocal()");
            if (!initialized /* || ConfigSettings.disableEmotesForSelf.Value || LCVR_Patcher.Enabled */)
                return;

            if (!isLocalPlayer)
            {
                Plugin.Logger.LogWarning("Cannot run TryPerformEmoteLocal on a character who does not belong to the local player. This is not allowed.");
                return;
            }

            Plugin.Debug("Attempting to emote for player: " + playerController.name);

            if (!CanPerformEmote())
                return;
            /*
            if (emote.randomEmotePool != null && emote.randomEmotePool.Count > 0)
                emote = emote.randomEmotePool[UnityEngine.Random.Range(0, emote.randomEmotePool.Count)];
            */
            //ForceSendAnimationUpdateLocal(emote);
            PerformEmote(clip);
            playerController.StartPerformingEmoteServerRpc();
            // SyncPerformingEmoteManager.SendPerformingEmoteUpdateToServer(emote);
            Plugin.Debug("Setting to 1");
            timeSinceStartingEmote = 0;
            playerController.performingEmote = true;
            originalAnimator.SetInteger("emoteNumber", 1);
        }


        public void TrySyncingEmoteWithEmoteController(EmoteController emoteController)
        {
            Plugin.Debug("EmoteControllerPlayer.TrySyncingEmoteWithEmoteController()");
            if (!initialized || emoteController == null /* || ConfigSettings.disableEmotesForSelf.Value || LCVR_Patcher.Enabled*/)
                return;

            if (!isLocalPlayer)
            {
                Plugin.Logger.LogWarning("Cannot run TrySyncingEmoteWithEmoteController on a character who does not belong to the local player. This is not allowed.");
                return;
            }

            Plugin.Debug("Attempting to sync emote for player: " + playerController.name + " with emote controller with id: " + emoteController.GetEmoteControllerId());

            if (!CanPerformEmote() || !emoteController.IsPerformingCustomEmote())
                return;

            //ForceSendAnimationUpdateLocal(emote, syncWithPlayer);
            SyncWithEmoteController(emoteController);
            playerController.StartPerformingEmoteServerRpc();
            // SyncPerformingEmoteManager.SendSyncEmoteUpdateToServer(emoteController);
            timeSinceStartingEmote = 0;
            playerController.performingEmote = true;
            originalAnimator.SetInteger("emoteNumber", 1);
        }


        public override bool CanPerformEmote()
        {
            Plugin.Debug("EmoteControllerPlayer.CanPerformEmote()");
            if (!isLocalPlayer)
                return true;

            Plugin.Debug("is local player");
            if (!initialized)
                Plugin.Logger.LogError("CanPerformEmote: NOT INITIALIZED");
            /*
            if (ConfigSettings.disableEmotesForSelf.Value)
                Plugin.Logger.LogError("CanPerformEmote: EMOTING FOR SELF DISABLED");

            if (LCVR_Patcher.Enabled)
                Plugin.Logger.LogError("CanPerformEmote: LCVR ENABLED");
            */
            if (!initialized /*|| ConfigSettings.disableEmotesForSelf.Value || LCVR_Patcher.Enabled*/)
                return false;
            Plugin.Debug("initialized");

            bool canPerformEmote = base.CanPerformEmote();

            //MethodInfo method = playerController.GetType().GetMethod("CheckConditionsForEmote", BindingFlags.NonPublic | BindingFlags.Instance);
            //canPerformEmote &= (bool)method.Invoke(playerController, new object[] { });

            canPerformEmote &= playerController.CheckConditionsForEmote();

            bool otherConditions = playerController.inAnimationWithEnemy == null && !(isLocalPlayer /* && CentipedePatcher.IsCentipedeLatchedOntoLocalPlayer()*/);
            Plugin.Debug($"canPerformEmote: {canPerformEmote}, otherConditions: {otherConditions}");
            return canPerformEmote && otherConditions;
        }


        public override void PerformEmote(/*UnlockableEmote emote,*/ AnimationClip overrideAnimationClip, float playAtTimeNormalized = 0)
        {
            Plugin.Debug("EmoteControllerPlayer.PerformEmote()");
            if (playerController == null)
                Plugin.Logger.LogError("PLAYERCONTROLLER NULL IN PERFORMEMOTE");
            if (playerController == null || isLocalPlayer)
                return;

            base.PerformEmote(overrideAnimationClip, playAtTimeNormalized);
            if (isPerformingEmote)
            {
                playerController.performingEmote = true;
                Plugin.Debug("Setting to 0");
                originalAnimator.SetInteger("emoteNumber", 0);
            }
        }


        public override void StopPerformingEmote()
        {
            if (playerController == null || isLocalPlayer)
                return;

            base.StopPerformingEmote();
        }


        public override ulong GetEmoteControllerId() => playerController != null ? playerController.NetworkObjectId : 0;
    }
}