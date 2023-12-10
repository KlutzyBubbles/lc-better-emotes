using GameNetcodeStuff;
using UnityEngine;

namespace BetterEmote
{
    public class CustomAudioAnimationEvent : MonoBehaviour
    {
        private void Start()
        {
            animator = GetComponent<Animator>();
            SoundsSource = player.movementAudio;
        }

        public void PlayClapSound()
        {
            if (player.performingEmote)
            {
                if (player.IsOwner && player.isPlayerControlled)
                {
                    if (animator.GetInteger("emoteNumber") != 6)
                    {
                        return;
                    }
                }
                bool noiseIsInsideClosedShip = player.isInHangarShipRoom && player.playersManager.hangarDoorsClosed;
                RoundManager.Instance.PlayAudibleNoise(player.transform.position, 22f, 0.6f, 0, noiseIsInsideClosedShip, 6);
                SoundsSource.pitch = Random.Range(0.59f, 0.79f);
                int num = Random.Range(0, 2);
                if (num == 1)
                {
                    SoundsSource.PlayOneShot(clap2);
                }
                else
                {
                    SoundsSource.PlayOneShot(clap1);
                }
            }
        }

        public void PlayFootstepSound()
        {
            if (player.performingEmote)
            {
                if (player.IsOwner && player.isPlayerControlled)
                {
                    if (animator.GetInteger("emoteNumber") != 4)
                    {
                        return;
                    }
                }
                player.PlayFootstepLocal();
                player.PlayFootstepServer();
            }
        }

        private Animator animator;

        private AudioSource SoundsSource;

        public AudioClip clap1;

        public AudioClip clap2;

        public AudioClip clap3;

        public PlayerControllerB player;
    }
}
