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
using Unity.VisualScripting;

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

        [Header("WALKIE TALKIE BATTERY LEVEL")]
        [SerializeField] private float maxRadioBatteryTimer;
        [SerializeField] private float currentRadioBatteryTimer;

        [Header("GAME UI")]
        [SerializeField] private GameObject gameMenuUI;
        [SerializeField] private GameObject ranOutOfOxygenUI;
        [SerializeField] private GameObject ranOutOfBatteryUI;
        [SerializeField] private GameObject gameCompleteUI;

        [Header("SFX")]
        [SerializeField] private AudioClip footstepTeleportSFX;

        private Recorder recorder;
        [HideInInspector] public float speakerAmp;

        private bool isInGame;
        private AudioSource playerAudioSource;

        private bool lastYButtonState = false;
        private int yButtonPressCount = 1;

        // Start is called before the first frame update
        void Start()
        {
            recorder = FindObjectOfType<Recorder>();
            playerAudioSource = GetComponent<AudioSource>();
            SetPlayerVariablesAtStart();

            GameManager.Instance.OnPlayerTeleport.AddListener(OnTeleport);
        }

        // Update is called once per frame
        void Update()
        {
            if (isInGame && InputDevices.GetDeviceAtXRNode(XRNode.LeftHand) != null)
            {
                //Enable/Disable voice chat based on user input 
                bool leftTriggerValue;
                if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.triggerButton, out leftTriggerValue) && leftTriggerValue)
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

                //Toggle Game Menu based on user input
                bool isYPressed, tempState = false;
                tempState = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.secondaryButton, out isYPressed)
                    && isYPressed || tempState;

                if (tempState != lastYButtonState) // Button state changed since last frame
                {
                    Debug.LogFormat("<color=white>Y IS PRESSED</color>");
                    lastYButtonState = tempState;
                    if (yButtonPressCount % 2 == 0)
                    {
                        ToggleGameUI(gameMenuUI.activeSelf ? -1 : 0);
                    }
                    yButtonPressCount++;
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

                    //Game Over : Ran out of Oxygen
                    OnGameOver(0);
                }

            }

            //Start decreasing walkie-talkie battery level when player is talking
            if(isInGame && recorder != null && recorder.TransmitEnabled)
            {
                currentRadioBatteryTimer -= Time.deltaTime;
                walkieTalkie.UpdateBatteryLevel(currentRadioBatteryTimer * 100f / maxRadioBatteryTimer);
                if(currentRadioBatteryTimer <= 0)
                {
                    isInGame = false;
                    DisableVoiceChat();

                    //Game Over : Ran out of Battery
                    OnGameOver(1);
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
                GameManager.Instance.OnVoiceChatEnabled?.Invoke();
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
                GameManager.Instance.OnVoiceChatDisabled?.Invoke();
            }
        }

        /// <summary>
        /// Sets player variables when game starts
        /// </summary>
        public void SetPlayerVariablesForGame()
        {
            isInGame = true;

            if(currentOxygenTimer == 0) currentOxygenTimer = maxOxygenTimer;
            if (currentRadioBatteryTimer == 0) currentRadioBatteryTimer = maxRadioBatteryTimer;

            DisableVoiceChat();
            SetReticleScale(0);
            ToggleHeadMountedLight(true);
            ToggleWalkieTalkie(true);
            TogglePlayerHandCanvas(true);
            ToggleXRDirectInteractor(true);

            //Suspend animations for left hand
            //TODO: ADD NEW ANIMATIONS FOR WALKIE TALKIE
            leftHandPresence.suspendHandAnimation = true;
        }

        public void SetPlayerVariablesForLobby()
        {
            isInGame = false;
            recorder.TransmitEnabled = false;

            currentOxygenTimer = maxOxygenTimer;
            currentRadioBatteryTimer = maxRadioBatteryTimer;

            EnableVoiceChat();
            SetReticleScale(1);
            ToggleHeadMountedLight(false);
            ToggleWalkieTalkie(false);
            TogglePlayerHandCanvas(false);
            ToggleXRDirectInteractor(true);

            leftHandPresence.suspendHandAnimation = false;
        }

        public void SetPlayerVariablesAtStart()
        {
            isInGame = false;
            recorder.TransmitEnabled = false;

            SetReticleScale(1);
            ToggleHeadMountedLight(false);
            ToggleWalkieTalkie(false);
            TogglePlayerHandCanvas(false);
            ToggleXRDirectInteractor(true);
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
        /// Toggles controllers on/off
        /// </summary>
        /// <param name="_leftState"></param>
        /// <param name="_rightState"></param>
        public void ToggleControllers(bool _leftState, bool _rightState)
        {
            leftHandPresence.showController = _leftState;
            rightHandPresence.showController = _rightState;
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
        /// Sets player rotation
        /// </summary>
        /// <param name="_rotation"></param>
        public void SetPlayerRot(Quaternion _rotation)
        {
            transform.rotation = _rotation;
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

        /// <summary>
        /// Is called when the player teleports
        /// </summary>
        public void OnTeleport()
        {
            //Play footstep SFX
            PlaySFX(footstepTeleportSFX, 0.75f);
        }

        /// <summary>
        /// Plays the given audio clip
        /// </summary>
        /// <param name="_clip"></param>
        /// <param name="_volume"></param>
        public void PlaySFX(AudioClip _clip, float _volume = 1)
        {
            playerAudioSource.Stop();
            playerAudioSource.clip = _clip;
            playerAudioSource.volume = _volume;
            playerAudioSource.Play();
        }

        /// <summary>
        /// Is called when game is over due to running out of resources
        /// | 0 = Oxygen Over | 1 = Battery Over
        /// </summary>
        /// <param name="_reason"></param>
        public void OnGameOver(int _reason)
        {
            switch(_reason)
            {
                case 0:
                    //Raise photon event for oxygen over
                    GameManager.Instance.RaiseCustomEvent(StaticData.OutOfOxygenEventCode, null);
                    break;
                case 1:
                    //Raise photon event for battery over
                    GameManager.Instance.RaiseCustomEvent(StaticData.OutOfBatteryEventCode, null);
                    break;
            }
        }

        /// <summary>
        /// Toggles Game UI on/off
        /// | -1 = All off | 0 = Game Menu | 1 = Oxygen Over 
        /// | 2 = Battery Over | 3 = Game Complete
        /// </summary>
        /// <param name="_index"></param>
        public void ToggleGameUI(int _index = -1)
        {
            gameMenuUI.SetActive(false);
            ranOutOfOxygenUI.SetActive(false);
            ranOutOfBatteryUI.SetActive(false);
            gameCompleteUI.SetActive(false);

            SetReticleScale(_index == -1 ? 0 : 1);

            switch(_index)
            {
                case -1:
                    break;
                case 0:
                    gameMenuUI.SetActive(true);
                    GameManager.Instance.OnGameMenuOpened?.Invoke();
                    break;
                case 1:
                    ranOutOfOxygenUI.SetActive(true);
                    GameManager.Instance.envController.HideCurrentEnvironment();
                    break;
                case 2:
                    ranOutOfBatteryUI.SetActive(true);
                    GameManager.Instance.envController.HideCurrentEnvironment();
                    break;
                case 3:
                    gameCompleteUI.SetActive(true);
                    GameManager.Instance.envController.HideCurrentEnvironment();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Is called when user presses Exit to Lobby button in game ui
        /// </summary>
        public void OnExitToLobbyPressed()
        {
            //Close game ui
            ToggleGameUI();

            //Fade to black
            GameManager.Instance.FadeToBlack();

            //Raise the Photon Event for Exiting to lobby
            GameManager.Instance.RaiseCustomEvent(StaticData.ExitToLobbyEventCode, null);
        }
    }
}

