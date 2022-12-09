using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CaveExplorer
{
    public class SymbolWheelPuzzle : Puzzle
    {
        [Header("SYMBOL LIST")]
        [SerializeField] private List<PuzzleSymbol> symbols;

        [Header("PUZZLE PIECE")]
        [SerializeField] private List<PuzzlePiece> puzzlePieces;

        [Header("STEERING WHEELS")]
        [SerializeField] private Transform currentWheel;
        [SerializeField] private float wheelAngle;
        [SerializeField] private Quaternion startWheelRot;
        [SerializeField] private Quaternion currentWheelRot;
        [SerializeField] private int wheelRotationCount;

        private bool completedHalfRotation;
        private bool completedFullRotation;

        private PhotonView photonView;
        // Start is called before the first frame update
        void Start()
        {
            photonView = PhotonView.Get(this);

            GameManager.Instance.OnJoinedRoom.AddListener(IntializePuzzle);
        }

        private void OnDisable()
        {
            GameManager.Instance.OnJoinedRoom.RemoveListener(IntializePuzzle);
        }

        public void IntializePuzzle()
        {
            if (!GameManager.Instance.isPlayer1)
                return;

            //Reset bools
            completedHalfRotation = false;
            completedFullRotation = false;

            //Generate random list of symbol index
            List<int> _symbolIndexList = new List<int>();
            for(int i = 0; i < symbols.Count; i++)
            {
                int _rand = Random.Range(0, symbols.Count);
                while(_symbolIndexList.Contains(_rand))
                {
                    _rand = Random.Range(0, symbols.Count);
                }
                _symbolIndexList.Add(_rand);
            }

            //Assign 4 symbols to each piece
            for(int i = 0; i < puzzlePieces.Count; i++)
            {
                //Convert index list to string to send it over RPC
                string _symbolIndexString = "";
                for(int j = 4 * i; j < (4*i) + 4; j++)
                {
                    _symbolIndexString += _symbolIndexList[j] + ",";
                }
                _symbolIndexString = _symbolIndexString.Remove(_symbolIndexString.Length - 1, 1);
                Debug.LogFormat("<color=red>_symbolIndexString {0}</color>", _symbolIndexString);

                //Set index for random initial rotation
                int _player1RandIndex = Random.Range(0, 4);
                int _player2RandIndex = Random.Range(0, 4);
                while (_player1RandIndex == _player2RandIndex)
                {
                    _player2RandIndex = Random.Range(0, 4);
                }

                photonView.RPC(nameof(AssignPuzzlePieceSymbols),
                    RpcTarget.AllBufferedViaServer, 
                    new object[] { i, _symbolIndexString, _player1RandIndex, _player2RandIndex });
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(currentWheel!= null && !completedFullRotation)
            {
                currentWheelRot = currentWheel.rotation;
                wheelAngle = Quaternion.Angle(startWheelRot, currentWheelRot);

                if(wheelAngle > 160f)
                {
                    completedHalfRotation = true;
                }
                if(completedHalfRotation && wheelAngle < 20)
                {
                    completedFullRotation = true;
                }
                
            }
            if (completedFullRotation)
            {
                //Rotation complete
                OnWheelRotationComplete();
            }
        }

        /// <summary>
        /// Returns a string with comma separated values of all puzzle piece symbols
        /// </summary>
        /// <returns></returns>
        private string GetSymbolIndexString()
        {
            string _currentSymbolIndexString = string.Empty;
            foreach (PuzzlePiece _piece in puzzlePieces)
            {
                _currentSymbolIndexString += _piece.GetCurrentSymbolIndex() + ",";
            }
            _currentSymbolIndexString = _currentSymbolIndexString.Remove(_currentSymbolIndexString.Length - 1, 1);
            Debug.LogFormat("<color=cyan>_currentSymbolIndexString : {0}</color>", _currentSymbolIndexString);

            return _currentSymbolIndexString;
        }

        /// <summary>
        /// Is called when the wheel makes a complete rotation
        /// </summary>
        private void OnWheelRotationComplete()
        {
            //Increment rotation count
            wheelRotationCount++;

            //Reset bool
            completedHalfRotation = false;
            completedFullRotation = false;

            //Rotate puzzle piece
            if(currentWheel!= null)
            {
                PuzzlePiece _puzzlePiece = currentWheel.GetComponent<SteeringWheel>().correspondingPuzzlePiece;
                _puzzlePiece.RotatePiece();
            }

            //Check if puzzle solved
            string _symbolString = GetSymbolIndexString();
            photonView.RPC(nameof(CheckIfPuzzleSolved), RpcTarget.AllBufferedViaServer,
                new object[] { GameManager.Instance.isPlayer1, _symbolString });
        }

        /// <summary>
        /// Is called when Select Enter event is triggered on the steering wheel
        /// </summary>
        /// <param name="args"></param>
        public void OnWheelGrab(SelectEnterEventArgs args)
        {
            Debug.LogFormat("<color=yellow>interactableObject object rot {0} | {1}</color>", 
                args.interactableObject.transform.name, args.interactableObject.transform.rotation.ToString("F2"));

            currentWheel = args.interactableObject.transform;
            startWheelRot = currentWheel.rotation;

            completedHalfRotation= false;
            completedFullRotation= false;
        }

        /// <summary>
        /// Is called when Select Exit event is triggered on the steering wheel
        /// </summary>
        /// <param name="args"></param>
        public void OnWheelRelease(SelectExitEventArgs args)
        {
            StartCoroutine(MakeWheelStick());
        }

        private IEnumerator MakeWheelStick()
        {
            currentWheel.rotation = Quaternion.identity;
            currentWheel.GetComponent<Rigidbody>().velocity = Vector3.zero;

            yield return new WaitForEndOfFrame();

            //currentWheel.GetComponent<Rigidbody>().isKinematic = false;
            currentWheel = null;

            //Reset bool
            completedHalfRotation = false;
            completedFullRotation = false;

        }

        /// <summary>
        /// A PUN RPC for assigning symbols to each puzzle piece
        /// </summary>
        [PunRPC]
        public void AssignPuzzlePieceSymbols(int _pieceIndex, string _symbolIndexString, int _player1RotIndex, int _player2RotIndex)
        {
            //Convert from string to index list
            List<string> _symbolIndex = _symbolIndexString.Split(',').ToList();
            List<PuzzleSymbol> _symbolsToAssign = new List<PuzzleSymbol>();
            foreach (string _index in _symbolIndex)
            {
                _symbolsToAssign.Add(symbols[int.Parse(_index)]);
            }

            //Assign given symbol list to puzzle piece
            puzzlePieces[_pieceIndex].SetSymbolMaterials(_symbolsToAssign);

            //Assign initial rotation
            puzzlePieces[_pieceIndex].SetInitialRotation(GameManager.Instance.isPlayer1 ? _player1RotIndex : _player2RotIndex);
        }

        /// <summary>
        ///  A PUN RPC for comparing the symbol index for all the players
        ///  and checking if the puzzle is solved
        /// </summary>
        [PunRPC]
        public void CheckIfPuzzleSolved(bool _isPlayer1, string _symbolIndexString)
        {
            if(GameManager.Instance.isPlayer1 != _isPlayer1)
            {
                if (_symbolIndexString.Equals(GetSymbolIndexString()))
                {
                    //Puzzle is solved
                    Debug.LogFormat("<color=green>PUZZLE IS SOLVED</color>");
                }
            }

        }
    }
}
