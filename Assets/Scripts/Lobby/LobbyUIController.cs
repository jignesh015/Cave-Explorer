using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

namespace CaveExplorer
{
    public class LobbyUIController : MonoBehaviour
    {
        [Header("CANVAS REFERENCES")]
        [SerializeField] private GameObject lobbyCanvas;
        [SerializeField] private GameObject player1Explorer;
        [SerializeField] private GameObject player1Guide;
        [SerializeField] private GameObject player2Explorer;
        [SerializeField] private GameObject player2Guide;

        [Header("SFX")]
        [SerializeField] private AudioClip buttonPressedSFX;

        private PhotonView photonView;
        private GameManager gameManager;
        private AudioSource lobbyUIAudio;

        // Start is called before the first frame update
        void Start()
        {
            photonView = PhotonView.Get(this);
            gameManager = GameManager.Instance;
            lobbyUIAudio = GetComponent<AudioSource>();

            //Add event listeners
            gameManager.OnConnectedToServer.AddListener(ConnectedToServer);
        }

        private void OnDisable()
        {
            gameManager.OnConnectedToServer.RemoveListener(ConnectedToServer);
        }

        public void ConnectedToServer()
        {
            Debug.Log("Lobby Canvas Connected to Server");
            photonView.RPC(nameof(SetPlayerRoles), RpcTarget.AllBufferedViaServer, gameManager.player1IsGuide);
        }

        public void OnConfirmPressed()
        {
            Debug.Log("CONFIRM PRESSEED");
            photonView.RPC(nameof(StartGame), RpcTarget.AllBufferedViaServer, gameManager.player1IsGuide);
        }

        public void OnSwapPressed()
        {
            gameManager.player1IsGuide = !gameManager.player1IsGuide;
            photonView.RPC(nameof(SetPlayerRoles), RpcTarget.AllBufferedViaServer, gameManager.player1IsGuide);
        }

        public void PlaySFX(AudioClip _clip)
        {
            lobbyUIAudio.Stop();
            lobbyUIAudio.clip= _clip;
            lobbyUIAudio.Play();
        }

        [PunRPC]
        private void SetPlayerRoles(bool _player1IsGuide)
        {
            gameManager.player1IsGuide = _player1IsGuide;

            //Set Role UI for Player 1
            player1Guide.SetActive(_player1IsGuide);
            player1Explorer.SetActive(!_player1IsGuide);

            //Set Role UI for Player 2
            player2Guide.SetActive(!_player1IsGuide);
            player2Explorer.SetActive(_player1IsGuide);

            //Play Button Pressed SFX
            PlaySFX(buttonPressedSFX);
        }

        [PunRPC]
        private void StartGame(bool _player1IsGuide)
        {
            gameManager.player1IsGuide = _player1IsGuide;

            //Hide Lobby Canvas
            lobbyCanvas.transform.localScale = Vector3.zero;

            //Play Button Pressed SFX
            PlaySFX(buttonPressedSFX);

            //Invoke Lobby Confirmed delegate
            gameManager.OnLobbyConfirm?.Invoke();
        }
    }
}
