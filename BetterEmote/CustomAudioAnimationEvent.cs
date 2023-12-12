using GameNetcodeStuff;
using UnityEngine;

namespace BetterEmote
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
                SoundsSource.PlayOneShot(claps[Random.Range(0, claps.Length)]);
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
    }
}
