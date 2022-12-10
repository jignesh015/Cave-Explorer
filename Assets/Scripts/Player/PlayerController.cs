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
        [Header("PLAYER PROPS")]
        [SerializeField] private GameObject headMountedLight;
        [SerializeField] private GameObject walkieTalkie;

        [Header("PLAYER HANDS")]
        [SerializeField] private HandPresence leftHandPresence;
        [SerializeField] private HandPresence rightHandPresence;

        [Header("RETICLE")]
        [SerializeField] private Transform reticle;
        [SerializeField] private List<Vector3> reticleScale;

        private Recorder recorder;
        [HideInInspector] public float speakerAmp;

        // Start is called before the first frame update
        void Start()
        {
            recorder = FindObjectOfType<Recorder>();
            recorder.TransmitEnabled = false;

            SetReticleScale(1);
            ToggleHeadMountedLight(false);
            ToggleWalkieTalkie(false);
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

        /// <summary>
        /// Enables transmission for voice chat
        /// </summary>
        public void EnableVoiceChat()
        {
            if (recorder != null && !recorder.TransmitEnabled)
            {
                recorder.TransmitEnabled = true;
            }
        }

        /// <summary>
        /// Disables transmission for voice chat
        /// </summary>
        public void DisableVoiceChat()
        {
            if (recorder != null && recorder.TransmitEnabled)
            {
                recorder.TransmitEnabled = false;
            }
        }

        /// <summary>
        /// Sets player variables when game starts
        /// </summary>
        public void SetPlayerVariablesOnGameStart()
        {
            SetReticleScale(0);
            ToggleHeadMountedLight(true);
            ToggleWalkieTalkie(true);

            //Suspend animations for left hand
            //TODO: ADD NEW ANIMATIONS FOR WALKIE TALKIE
            leftHandPresence.suspendHandAnimation = true;
        }

        /// <summary>
        /// Toggles player headlight on/off
        /// </summary>
        /// <param name="state"></param>
        public void ToggleHeadMountedLight(bool state)
        {
            headMountedLight.SetActive(state);
        }

        /// <summary>
        /// Toggles walkie talkie on/off
        /// </summary>
        /// <param name="state"></param>
        public void ToggleWalkieTalkie(bool state)
        {
            walkieTalkie.SetActive(state);
        }

        /// <summary>
        /// Sets player position
        /// </summary>
        /// <param name="_position"></param>
        public void SetPlayerPos(Vector3 _position)
        {
            transform.position = _position;
        }

        /// <summary>
        /// Sets reticle scale
        /// | 0 = Teleport | 1 = UI
        /// </summary>
        /// <param name="_index"></param>
        public void SetReticleScale(int _index)
        {
            reticle.localScale = reticleScale[_index];
        }
    }
}

