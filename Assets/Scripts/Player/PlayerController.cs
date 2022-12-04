using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;

namespace CaveExplorer
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameObject headMountedLight;

        private Recorder recorder;
        [HideInInspector] public float speakerAmp;

        public bool isPlayerGuide { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            recorder = FindObjectOfType<Recorder>();
            recorder.TransmitEnabled = false;

            ToggleHeadMountedLight(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand) != null)
            {
                bool triggerValue;
                if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)
                {
                    EnableVoiceChat();
                }
                else
                {
                    DisableVoiceChat();
                }

                if (recorder.IsCurrentlyTransmitting)
                {
                    speakerAmp = recorder.LevelMeter.CurrentPeakAmp * 1000f;
                    if (speakerAmp > 0.1f)
                    {
                        Debug.LogFormat("<color=green>IsCurrentlyTransmitting {0}</color>", speakerAmp.ToString("F3"));
                    }
                }
                else
                {
                    speakerAmp = 0;
                }
            }
        }

        public void EnableVoiceChat()
        {
            if (recorder != null && !recorder.TransmitEnabled)
            {
                recorder.TransmitEnabled = true;
            }
        }

        public void DisableVoiceChat()
        {
            if (recorder != null && recorder.TransmitEnabled)
            {
                recorder.TransmitEnabled = false;
            }
        }

        public void SetPlayerVariables()
        {
            if(isPlayerGuide)
            {
                //If Player is a Guide
            }
            else
            {
                //If Player is an Explorer
                ToggleHeadMountedLight(true);
            }
        }

        public void ToggleHeadMountedLight(bool state)
        {
            headMountedLight.SetActive(state);
        }

        public void SetPlayerPos(Vector3 _position)
        {
            transform.position = _position;
        }
    }
}

