using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CaveExplorer
{
    public class GameManager : MonoBehaviour
    {
        [HideInInspector] public bool isPlayer1;

        [Header("PLAYER SPAWN POINTS")]
        [SerializeField] private Transform player1SpawnPoint;
        [SerializeField] private Transform player2SpawnPoint;

        [Header("NETWORK EVENTS")]
        public UnityEvent OnConnectedToServer;
        public UnityEvent OnJoinedRoom;
        public UnityEvent OnPlayerEnteredRoom;
        public UnityEvent OnPlayerLeftRoom;

        [Header("UI EVENTS")]
        public UnityEvent OnLobbyStartPressed;

        [Header("SCRIPT REFERENCES")]
        [HideInInspector] public EnvironmentController envController;
        [HideInInspector] public PlayerController playerController;

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
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void InitializePlayer(bool _isPlayer1)
        {
            isPlayer1 = _isPlayer1;
            playerController.SetPlayerPos(isPlayer1 ? 
                player1SpawnPoint.position : player2SpawnPoint.position);
        }

        public void StartGame()
        {
            //Load cave environment for respective player
            Environment _envToLoad = isPlayer1 ? envController.player1CaveEnvList[0]
                : envController.player2CaveEnvList[0];
            envController.LoadEnvironment(_envToLoad, OnNewEnvLoaded);

            //Place player in their starting position
            playerController.SetPlayerPos(_envToLoad.playerSpawnPos);
        }

        public void OnNewEnvLoaded()
        {
            //Remove Lobby Environment
            envController.UnloadEnvironment(envController.lobbyEnv);

            //Set player variables
            playerController.SetPlayerVariables();
        }
    }
}

