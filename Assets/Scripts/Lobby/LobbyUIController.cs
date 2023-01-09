using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace CaveExplorer
{
    public class LobbyUIController : MonoBehaviour
    {
        [Header("CANVAS REFERENCES")]
        [SerializeField] private GameObject lobbyCanvas;
        [SerializeField] private GameObject waitingToConnectPanel;
        [SerializeField] private GameObject connectedToServerPanel;
        [SerializeField] private GameObject player1ReadyText;
        [SerializeField] private GameObject player2ReadyText;
        [SerializeField] private Button startButton;

        [Header("SFX")]
        [SerializeField] private AudioClip buttonPressedSFX;

        //private PhotonView photonView;
        private GameManager gameManager;
        private AudioSource lobbyUIAudio;

        // Start is called before the first frame update
        void Start()
        {
            //photonView = PhotonView.Get(this);
            gameManager = GameManager.Instance;
            lobbyUIAudio = GetComponent<AudioSource>();

            //Add event listeners
            gameManager.OnConnectedToServer.AddListener(ConnectedToServer);

            //Show appropriate UI
            waitingToConnectPanel.SetActive(!gameManager.isConnectedToServer);
            connectedToServerPanel.SetActive(gameManager.isConnectedToServer);
        }

        //private void OnEnable()
        //{
        //    PhotonNetwork.NetworkingClient.EventReceived += OnCustomEvent;
        //}

        private void OnDisable()
        {
            //PhotonNetwork.NetworkingClient.EventReceived -= OnCustomEvent;
            gameManager.OnConnectedToServer.RemoveListener(ConnectedToServer);
        }

        /// <summary>
        /// Is triggered when player gets connected to a server
        /// </summary>
        private void ConnectedToServer()
        {
            waitingToConnectPanel.SetActive(false);
            connectedToServerPanel.SetActive(true);
        }

        public void OnStartPressed()
        {
            Debug.Log("CONFIRM PRESSEED");
            //photonView.RPC(nameof(StartGame), RpcTarget.AllBufferedViaServer, gameManager.isPlayer1);

            //Raise Photon Event for Start button pressed
            gameManager.RaiseCustomEvent(StaticData.StartGameEventCode, new object[] { gameManager.isPlayer1 });
        }

        //public void OnSwapPressed()
        //{
        //    gameManager.player1IsGuide = !gameManager.player1IsGuide;
        //    photonView.RPC(nameof(SetPlayerRoles), RpcTarget.AllBufferedViaServer, gameManager.player1IsGuide);
        //}

        public void PlaySFX(AudioClip _clip)
        {
            lobbyUIAudio.Stop();
            lobbyUIAudio.clip= _clip;
            lobbyUIAudio.Play();
        }

        /// <summary>
        /// This function receives the custom RPC events sent by any player
        /// </summary>
        /// <param name="photonEvent"></param>
        //public void OnCustomEvent(EventData photonEvent)
        //{
        //    byte eventCode = photonEvent.Code;
        //    if (eventCode == StaticData.StartGameEventCode)
        //        StartGame((object[])photonEvent.CustomData);
        //}

        public void StartGame(bool _isPlayer1)
        {
            //Extract data from custom event object
            //bool _isPlayer1 = (bool)_content[0];
            if (_isPlayer1)
            {
                player1ReadyText.SetActive(true);
            }
            else
            {
                player2ReadyText.SetActive(true);
            }

            //Disable Start button once pressed
            if ((_isPlayer1 && gameManager.isPlayer1) || (!_isPlayer1 && !gameManager.isPlayer1))
            {
                startButton.interactable = false;
            }

            //If both player are ready, start the game
            if ((player1ReadyText.activeSelf && player2ReadyText.activeSelf) ||
                gameManager.enableSinglePlayerMode)
            {
                //Hide Lobby Canvas
                lobbyCanvas.transform.localScale = Vector3.zero;

                //Play Button Pressed SFX
                PlaySFX(buttonPressedSFX);

                //Disable voice chat
                gameManager.playerController.DisableVoiceChat();

                //Invoke Lobby Confirmed delegate
                gameManager.OnLobbyStartPressed?.Invoke();
            }
        }

        //[PunRPC]
        //private void StartGame(bool _isPlayer1)
        //{
        //    if(_isPlayer1)
        //    {
        //        player1ReadyText.SetActive(true);
        //    }
        //    else
        //    {
        //        player2ReadyText.SetActive(true);
        //    }

        //    //Disable Start button once pressed
        //    if((_isPlayer1 && gameManager.isPlayer1) || (!_isPlayer1 && !gameManager.isPlayer1))
        //    {
        //        startButton.interactable = false;
        //    }

        //    //If both player are ready, start the game
        //    if((player1ReadyText.activeSelf && player2ReadyText.activeSelf) ||
        //        gameManager.enableSinglePlayerMode)
        //    {
        //        //Hide Lobby Canvas
        //        lobbyCanvas.transform.localScale = Vector3.zero;

        //        //Play Button Pressed SFX
        //        PlaySFX(buttonPressedSFX);

        //        //Invoke Lobby Confirmed delegate
        //        gameManager.OnLobbyStartPressed?.Invoke();
        //    }
        //}
    }
}

