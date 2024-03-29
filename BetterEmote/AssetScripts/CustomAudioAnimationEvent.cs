﻿using BetterEmote.Utils;
using GameNetcodeStuff;
using UnityEngine;

namespace BetterEmote.AssetScripts
{
    public class CustomAudioAnimationEvent : MonoBehaviour
    {
        private Animator animator;

        private AudioSource SoundsSource;

        public static AudioClip[] claps = new AudioClip[2];

        public PlayerControllerB player;

        private void Start()
        {
            animator = GetComponent<Animator>();
            SoundsSource = player.movementAudio;
        }

        public void PlayClapSound()
        {
            if (player != null && player.performingEmote)
            {
                if (player.IsOwner && player.isPlayerControlled)
                {
                    if (getCurrentEmoteID() != (int)Emote.Clap)
                    {
                        return;
                    }
                }
                bool noiseIsInsideClosedShip = player.isInHangarShipRoom && (player.playersManager?.hangarDoorsClosed ?? false);
                RoundManager.Instance.PlayAudibleNoise(player.transform.position, 22f, 0.6f, 0, noiseIsInsideClosedShip, 6);
                SoundsSource.pitch = Random.Range(0.59f, 0.79f);
                if (claps != null && claps.Length > 0)
                {
                    SoundsSource.PlayOneShot(claps[Random.Range(0, claps.Length)]);
                }
            }
        }

        public void PlayFootstepSound()
        {
            if (player != null && player.performingEmote)
            {
                if (player.IsOwner && player.isPlayerControlled)
                {
                    if (getCurrentEmoteID() != (int)Emote.Griddy)
                    {
                        return;
                    }
                }
                player.PlayFootstepLocal();
                player.PlayFootstepServer();
            }
        }

        private int getCurrentEmoteID()
        {
            return EmoteDefs.normalizeEmoteNumber(animator?.GetInteger("emoteNumber") ?? 0);
        }
    }
}
