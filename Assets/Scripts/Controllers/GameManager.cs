using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        [Header("PLAYER SPAWN POINTS")]
        [SerializeField] private Transform player1SpawnPoint;
        [SerializeField] private Transform player2SpawnPoint;

        [Header("POST PROCESSING REFERENCES")]
        [SerializeField] private Volume ppv;
        [SerializeField] private float postExposureDefault;
        [SerializeField] private float postExposureFadeToBlack;

        [Header("SFX")]
        [SerializeField] private AudioClip enterCaveSFX;

        [Header("SCRIPT REFERENCES")]
        [HideInInspector] public EnvironmentController envController;
        [HideInInspector] public PlayerController playerController;

        [Header("NETWORK EVENTS")]
        public UnityEvent OnConnectedToServer;
        public UnityEvent OnJoinedRoom;
        public UnityEvent OnPlayerEnteredRoom;
        public UnityEvent OnPlayerLeftRoom;

        [Header("UI EVENTS")]
        public UnityEvent OnLobbyStartPressed;

        [Header("DEBUG ONLY")]
        public bool enableSinglePlayerMode;

        private ColorAdjustments colorAdjustments;

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

            //Add delegate
            OnLobbyStartPressed.AddListener(StartGame);
            OnConnectedToServer.AddListener(ConnectedToServer);

            //Populate PPV reference
            ppv.profile.TryGet(out colorAdjustments);
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

            yield return new WaitForSeconds(1f);

            //Increment cave level
            envController.caveLevel++;

            //Load the fisrt cave environment
            LoadCaveEnvironment(envController.caveLevel);

            //Switch off network players
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
            envController.LoadEnvironment(_envToLoad, OnNewEnvLoaded, true);

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
        public void OnNewEnvLoaded()
        {
            //Set player variables
            playerController.SetPlayerVariablesForGame();

            //Fade to normal
            FadeToNormal();
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
    }
}

