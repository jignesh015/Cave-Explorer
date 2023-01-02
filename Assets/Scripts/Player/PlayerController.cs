using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System;

namespace CaveExplorer
{
    public class PlayerController : MonoBehaviour
    {
        [Header("PLAYER PROPS")]
        [SerializeField] private GameObject headMountedLight;
        [SerializeField] private WalkieTalkieController walkieTalkie;

        [Header("PLAYER HANDS")]
        [SerializeField] private HandPresence leftHandPresence;
        [SerializeField] private HandPresence rightHandPresence;
        [SerializeField] private XRDirectInteractor rightDirectInteractor;

        [Header("RETICLE")]
        [SerializeField] private Transform reticle;
        [SerializeField] private List<Vector3> reticleScale;

        [Header("PLAYER OXYGEN LEVEL")]
        //Oxygen timer is the amount of time the player has left till their oxygen lasts
        [SerializeField] private float maxOxygenTimer;
        [SerializeField] private float currentOxygenTimer;
        [SerializeField] private GameObject playerHandCanvas;
        [SerializeField] private TextMeshProUGUI oxygenTimerDisplay;

        private Recorder recorder;
        [HideInInspector] public float speakerAmp;

        private bool isInGame;

        // Start is called before the first frame update
        void Start()
        {
            recorder = FindObjectOfType<Recorder>();
            SetPlayerVariablesForLobby();
        }

        // Update is called once per frame
        void Update()
        {
            //Enable/Disable voice chat based on user input 
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

            //Start decreasing oxygen level when player are in game
            if(isInGame)
            {
                currentOxygenTimer -= Time.deltaTime;
                DisplayCurrentOxygenTimer(currentOxygenTimer);
                if (currentOxygenTimer <= 0)
                {
                    isInGame= false;
                    DisplayCurrentOxygenTimer(0);

                    //TODO: GAME OVER LOGIC
                }

            }
        }

        /// <summary>
        /// Displays the current oxygen timer to the user
        /// </summary>
        private void DisplayCurrentOxygenTimer(float _timer)
        {
            TimeSpan time = TimeSpan.FromSeconds(_timer);
            string str = time.ToString(@"mm\:ss");
            oxygenTimerDisplay.text= str;
        }

        /// <summary>
        /// Enables transmission for voice chat
        /// </summary>
        public void EnableVoiceChat()
        {
            if (recorder != null && !recorder.TransmitEnabled)
            {
                recorder.TransmitEnabled = true;
                walkieTalkie.ToggleWalkieTalkieLED(true);
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
                walkieTalkie.ToggleWalkieTalkieLED(false);
            }
        }

        /// <summary>
        /// Sets player variables when game starts
        /// </summary>
        public void SetPlayerVariablesForGame()
        {
            isInGame = true;
            currentOxygenTimer = maxOxygenTimer;

            SetReticleScale(0);
            ToggleHeadMountedLight(true);
            ToggleWalkieTalkie(true);
            TogglePlayerHandCanvas(true);

            //Suspend animations for left hand
            //TODO: ADD NEW ANIMATIONS FOR WALKIE TALKIE
            leftHandPresence.suspendHandAnimation = true;
        }

        public void SetPlayerVariablesForLobby()
        {
            isInGame = false;
            recorder.TransmitEnabled = false;

            SetReticleScale(1);
            ToggleHeadMountedLight(false);
            ToggleWalkieTalkie(false);
            TogglePlayerHandCanvas(false);

            leftHandPresence.suspendHandAnimation = false;
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
            walkieTalkie.gameObject.SetActive(state);
        }

        /// <summary>
        /// Toggles the hand canvas on/off
        /// </summary>
        /// <param name="state"></param>
        public void TogglePlayerHandCanvas(bool state)
        {
            playerHandCanvas.SetActive(state);
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

        /// <summary>
        /// Toggles direct interactor on/off
        /// </summary>
        /// <param name="state"></param>
        public void ToggleXRDirectInteractor(bool state)
        {
            rightDirectInteractor.enabled = state;
        }
    }
}

