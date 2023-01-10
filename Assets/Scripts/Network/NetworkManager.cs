using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.Events;

namespace CaveExplorer
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public UnityAction networkErrorCallback;

        [SerializeField] private float networkTimeoutLimit;
        private float connectionTime;

        // Start is called before the first frame update
        void Start()
        {
            //ConnectToServer();
        }

        private void Update()
        {
            if(connectionTime != 0)
            {
                Debug.LogFormat("<color=yellow>Connecting... {0}</color>", (Time.time - connectionTime).ToString("F2"));
                if((Time.time - connectionTime) > networkTimeoutLimit)
                {
                    connectionTime = 0;
                    networkErrorCallback?.Invoke();
                }
            }
        }

        public void ConnectToServer()
        {
            PhotonNetwork.ConnectUsingSettings();
            connectionTime = Time.time;
            Debug.Log("<color=yellow>Try connecting to a server...</color>");
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("<color=green>Connected to a server</color>");
            base.OnConnectedToMaster();
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 10;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;

            //Join an existing room or create one if room doesn't exist
            PhotonNetwork.JoinOrCreateRoom("CaveExplorer_1", roomOptions, TypedLobby.Default);

            //Invoke delegate
            GameManager.Instance.OnConnectedToServer?.Invoke();
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            //Reset connection time
            connectionTime = 0;

            int playerCount = PlayerCount();
            PhotonNetwork.LocalPlayer.NickName = "Player " + playerCount;
            Debug.LogFormat("<color=cyan>Joined a room with {0} players</color>", playerCount);

            //Initialize local player
            GameManager.Instance.InitializePlayer(playerCount == 1);

            //Invoke delegate
            GameManager.Instance.OnJoinedRoom?.Invoke();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            int playerCount = PlayerCount();
            Debug.LogFormat("<color=cyan>Player {0} entered the room with {1} players</color>",
                newPlayer.NickName, playerCount);

            //Invoke delegate
            GameManager.Instance.OnPlayerEnteredRoom?.Invoke();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            int playerCount = PlayerCount();
            PhotonNetwork.LocalPlayer.NickName = "Player " + playerCount;

            //Re-Initialize local player
            GameManager.Instance.InitializePlayer(playerCount == 1);

            //Invoke delegate
            GameManager.Instance.OnPlayerLeftRoom?.Invoke();
        }

        /// <summary>
        /// Returns the number of players in the current room
        /// </summary>
        /// <returns></returns>
        public int PlayerCount()
        {
            return PhotonNetwork.PlayerList.Length;
        }

        /// <summary>
        /// Returns the local player's name
        /// </summary>
        /// <returns></returns>
        public string PlayerName()
        {
            return PhotonNetwork.LocalPlayer.NickName;
        }
    }
}

