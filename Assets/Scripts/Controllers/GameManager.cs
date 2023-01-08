using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CaveExplorer
{
    public class GameManager : MonoBehaviour
    {
        [Header("PUBLIC BOOL")]
        [HideInInspector] public bool isPlayer1;
        [HideInInspector] public bool isConnectedToServer;
        [HideInInspector] public bool hasCompletedOnboarding;

        [Header("PLAYER SPAWN POINTS")]
        [SerializeField] private Transform player1SpawnPoint;
        [SerializeField] private Transform player2SpawnPoint;

        [Header("POST PROCESSING REFERENCES")]
        [SerializeField] private Volume ppv;
        [SerializeField] private float postExposureDefault;
        [SerializeField] private float postExposureFadeToBlack;

        [Header("SFX")]
        [SerializeField] private AudioMixerGroup audioDistortionMixer;
        [SerializeField] private AudioClip enterCaveSFX;

        [Header("SCRIPT REFERENCES")]
        [HideInInspector] public EnvironmentController envController;
        [HideInInspector] public PlayerController playerController;
        [HideInInspector] public OnboardingController onboardingController;
        [HideInInspector] public BackgroundSoundController bgmController;

        [Header("NETWORK EVENTS")]
        public UnityEvent OnConnectedToServer;
        public UnityEvent OnJoinedRoom;
        public UnityEvent OnPlayerEnteredRoom;
        public UnityEvent OnPlayerLeftRoom;

        [Header("UI EVENTS")]
        public UnityEvent OnLobbyStartPressed;
        public UnityEvent OnGameMenuOpened;

        [Header("GAME EVENTS")]
        public UnityEvent OnPlayerTeleport;
        public UnityEvent OnVoiceChatEnabled;
        public UnityEvent OnVoiceChatDisabled;
        public UnityEvent OnGrabButtonPressed;

        [Header("DEBUG ONLY")]
        public bool enableSinglePlayerMode;

        private ColorAdjustments colorAdjustments;
        private bool remotePlayerPressedStart;
        private bool remotePlayerStatus;

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            //Get script references
            envController = FindObjectOfType<EnvironmentController>();
            playerController = FindObjectOfType<PlayerController>();
            onboardingController= FindObjectOfType<OnboardingController>();
            bgmController = FindObjectOfType<BackgroundSoundController>();

            //Add delegate
            OnJoinedRoom.AddListener(EnterLobby);
            OnLobbyStartPressed.AddListener(StartGame);
            OnConnectedToServer.AddListener(ConnectedToServer);

            //Populate PPV reference
            ppv.profile.TryGet(out colorAdjustments);
        }

        private void OnEnable()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnCustomEvent;
        }

        private void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnCustomEvent;
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Is triggered when connected to the photon server
        /// </summary>
        private void ConnectedToServer()
        {
            isConnectedToServer = true;
        }

        /// <summary>
        /// Initializes the local player
        /// </summary>
        /// <param name="_isPlayer1"></param>
        public void InitializePlayer(bool _isPlayer1)
        {
            isPlayer1 = _isPlayer1;
            playerController.SetPlayerPos(isPlayer1 ?
                player1SpawnPoint.position : player2SpawnPoint.position);
        }

        /// <summary>
        /// Loads the lobby environment
        /// </summary>
        public void EnterLobby()
        {
            //Load Cave Environment
            envController.LoadEnvironment(envController.lobbyEnv, OnLobbyEnvLoaded, true);
        }

        /// <summary>
        /// Is called when the lobby environment is loaded 
        /// </summary>
        private void OnLobbyEnvLoaded()
        {
            //Set player Position
            playerController.SetPlayerPos(isPlayer1 ?
                player1SpawnPoint.position : player2SpawnPoint.position);

            //Set player Rotation
            playerController.SetPlayerRot(envController.lobbyEnv.playerSpawnRot);

            //Set player variables
            playerController.SetPlayerVariablesForLobby();

            //Reset cave environment level
            envController.caveLevel = -1;

            //Fade to normal
            FadeToNormal();

            //If player hasn't completed onboarding, play the scenario
            if(!hasCompletedOnboarding)
                onboardingController.PlayOnboardingScenrio(OnboardingScenario.WelcomeToLobby);

            //Call the remote start event if available
            if (remotePlayerPressedStart)
            {
                remotePlayerPressedStart = false;
                FindObjectOfType<LobbyUIController>().StartGame(remotePlayerStatus);
            }
        }

        /// <summary>
        /// Starts the game by placing the player in the respective environment
        /// </summary>
        public void StartGame()
        {
            GoToNextCave();
        }

        /// <summary>
        /// Increments the cave level and loads the new cave environment
        /// </summary>
        public void GoToNextCave()
        {
            StartCoroutine(GoToNextCaveAsync());
        }

        private IEnumerator GoToNextCaveAsync()
        {
            //Fade to black
            FadeToBlack();

            yield return new WaitForSeconds(1.2f);

            //Increment cave level
            envController.caveLevel++;

            //Load the fisrt cave environment
            LoadCaveEnvironment(envController.caveLevel);

            //Hide network players
            ToggleNetworkPlayers(false);
        }

        /// <summary>
        /// Loads the cave environment as per the given cave level
        /// And places the player at the start position
        /// </summary>
        /// <param name="_caveIndex"></param>
        public void LoadCaveEnvironment(int _caveIndex)
        {
            //Load cave environment for respective player
            Environment _envToLoad = isPlayer1 ? envController.player1CaveEnvList[_caveIndex]
                : envController.player2CaveEnvList[_caveIndex];
            envController.LoadEnvironment(_envToLoad, OnCaveEnvLoaded, true);

            //Place player in their starting position
            playerController.SetPlayerPos(_envToLoad.playerSpawnPos);

            //Set the player rotation
            playerController.SetPlayerRot(_envToLoad.playerSpawnRot);

            //Play SFX for entering the cave
            playerController.PlaySFX(enterCaveSFX);
        }

        /// <summary>
        /// Is called when a new environment is done loading
        /// </summary>
        public void OnCaveEnvLoaded()
        {
            //Set player variables
            playerController.SetPlayerVariablesForGame();

            //Fade to normal
            FadeToNormal();

            //Fade-in BGM
            bgmController.LerpBGMVolume(0.05f, 1);

            //If player hasn't completed onboarding, play the scenario
            if (!hasCompletedOnboarding)
                onboardingController.PlayOnboardingScenrio(OnboardingScenario.RightTriggerToTeleport);
        }

        /// <summary>
        /// Toggles all the network players on/off
        /// </summary>
        /// <param name="_state"></param>
        public void ToggleNetworkPlayers(bool _state)
        {
            List<NetworkPlayer> _networkPlayers = FindObjectsOfType<NetworkPlayer>().ToList();
            foreach(NetworkPlayer _np in _networkPlayers)
            {
                _np.transform.localScale = _state ? Vector3.one : Vector3.zero;

                //Set distortion audio mixer
                _np.SetAudioMixer(audioDistortionMixer);
            }
        }

        /// <summary>
        /// Fade the scene to black by updating the post exposure value
        /// </summary>
        public void FadeToBlack()
        {
            StartCoroutine(FadeSceneAsync(postExposureDefault, postExposureFadeToBlack));
        }

        /// <summary>
        /// Fade the scene to normal by updating the post exposure value
        /// </summary>
        public void FadeToNormal()
        {
            StopCoroutine(nameof(FadeSceneAsync));
            StartCoroutine(FadeSceneAsync(postExposureFadeToBlack, postExposureDefault));
        }

        private IEnumerator FadeSceneAsync(float _startValue, float _endValue)
        {
            //Fade out scene
            float _elapsedTime = 0;
            float _fadeOutTime = 15f;
            float _postExposureValue = _startValue;
            while (_elapsedTime < _fadeOutTime)
            {
                _postExposureValue = Mathf.Lerp(_postExposureValue, _endValue, (_elapsedTime / _fadeOutTime));
                colorAdjustments.postExposure.Override(_postExposureValue);
                _elapsedTime += Time.deltaTime;
                yield return null;
            }
            colorAdjustments.postExposure.Override(_endValue);
        }

        /// <summary>
        /// Raises a custom Photon event using the given parameters
        /// </summary>
        public void RaiseCustomEvent(byte _eventCode, object[] _content)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache };
            PhotonNetwork.RaiseEvent(_eventCode, _content, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// This function receives the custom RPC events sent by any player
        /// </summary>
        /// <param name="photonEvent"></param>
        public void OnCustomEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            if (eventCode == StaticData.StartGameEventCode)
                StartGameEvent((object[])photonEvent.CustomData);
            else if (eventCode == StaticData.OutOfOxygenEventCode)
                playerController.ToggleGameUI(1);
            else if (eventCode == StaticData.OutOfBatteryEventCode)
                playerController.ToggleGameUI(2);
            else if (eventCode == StaticData.GameCompleteEventCode)
                playerController.ToggleGameUI(3);
            else if (eventCode == StaticData.ExitToLobbyEventCode)
                Invoke(nameof(EnterLobby), 1f);
        }

        /// <summary>
        /// Is called when start game event is received
        /// </summary>
        /// <param name="_content"></param>
        private void StartGameEvent(object[] _content)
        {
            //Extract data from custom event object
            bool _isPlayer1 = (bool)_content[0];
            if (FindObjectOfType<LobbyUIController>() != null)
            {
                //Lobby is already loaded, call the start method
                FindObjectOfType<LobbyUIController>().StartGame(_isPlayer1);
            }
            else
            {
                //Cache the event for later
                if (isPlayer1 != _isPlayer1)
                {
                    remotePlayerPressedStart = true;
                    remotePlayerStatus = _isPlayer1;
                }
            }
        }
    }
}

