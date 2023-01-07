using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public class Puzzle : MonoBehaviour
    {
        /// <summary>
        /// Is called when a puzzle starts
        /// </summary>
        public virtual void OnPuzzleStart()
        {

        }

        /// <summary>
        /// Is called when a puzzle is solved by the players
        /// </summary>
        public virtual void OnPuzzleSolved()
        {

        }

        /// <summary>
        /// Is called when the player teleports
        /// </summary>
        public virtual void OnPlayerTeleport()
        {
            GameManager.Instance.OnPlayerTeleport?.Invoke();
        }

        /// <summary>
        /// Raises a custom Photon event using the given parameters
        /// </summary>
        public virtual void RaiseCustomEvent(byte _eventCode, object[] _content)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(_eventCode, _content, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}
