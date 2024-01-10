using BetterEmote.Patches;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterEmote.AssetScripts
{
    public class CustomAnimationObjects : MonoBehaviour
    {
        private PlayerControllerB _player;
        private MeshRenderer _sign;
        private GameObject _signText;
        private SkinnedMeshRenderer _legs;

        private void Start()
        {
            _player = GetComponent<PlayerControllerB>();
        }

        private void Update()
        {
            if (_sign == null || _signText == null)
            {
                FindSign();
            }
            else
            {
                _sign.transform.localPosition = _sign.transform.parent.Find("spine").localPosition;
                if (_legs == null && _player.IsOwner && _player.isPlayerControlled)
                {
                    FindLegs();
                }
                else
                {
                    DisableEverything();
                    if (_player.performingEmote)
                    {
                        int emoteNumber = _player.playerBodyAnimator.GetInteger("emoteNumber");
                        if (emoteNumber != -10)
                        {
                            if (emoteNumber == 9)
                            {
                                if (_legs != null)
                                {
                                    _legs.enabled = true;
                                }
                                if (_player.IsOwner)
                                {
                                    EmotePatch.isLocalArmsSeparatedFromCamera = true;
                                }
                                return;
                            }
                            if (emoteNumber != 10)
                            {
                                return;
                            }
                        }
                        _sign.enabled = true;
                        if (!_signText.activeSelf)
                        {
                            _signText.SetActive(true);
                        }
                        if (_player.IsOwner)
                        {
                            EmotePatch.isLocalArmsSeparatedFromCamera = true;
                        }
                    }
                }
            }
        }

        private void DisableEverything()
        {
            if (_legs != null)
            {
                _legs.enabled = false;
            }
            _sign.enabled = false;
            if (_signText.activeSelf)
            {
                _signText.SetActive(false);
            }
            if (_player.IsOwner && _player.isPlayerControlled)
            {
                EmotePatch.isLocalArmsSeparatedFromCamera = false;
            }
        }

        private void FindSign()
        {
            _sign = _player.transform.Find("ScavengerModel").Find("metarig").Find("Sign").GetComponent<MeshRenderer>();
            _signText = _sign.transform.Find("Text").gameObject;
        }

        private void FindLegs()
        {
            _legs = _player.transform.Find("ScavengerModel").Find("LEGS").GetComponent<SkinnedMeshRenderer>();
        }
    }
}
