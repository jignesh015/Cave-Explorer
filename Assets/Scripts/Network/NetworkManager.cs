using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("<color=yellow>Try connecting to a server...</color>");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("<color=green>Connected to a server</color>");
        base.OnConnectedToMaster();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        roomOptions.IsVisible= true;
        roomOptions.IsOpen= true;

        //Join an existing room or create one if room doesn't exist
        PhotonNetwork.JoinOrCreateRoom("CaveExplorer_1", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("<color=cyan>Joined a room</color>");
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("<color=cyan>Player {0} entered the room</color>", newPlayer.NickName);
        base.OnPlayerEnteredRoom(newPlayer);
    }
}
