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
                photonView.RPC(nameof(AssignPuzzlePieceSymbols),
                    RpcTarget.AllBufferedViaServer, new object[] { i, _symbolIndexString });
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
        /// Is called when the wheel makes a complete rotation
        /// </summary>
        private void OnWheelRotationComplete()
        {
            //Increment rotation count
            wheelRotationCount++;

            //Reset bool
            completedHalfRotation = false;
            completedFullRotation = false;

            //TODO: CHANGE PUZZLE PIECE ROTATION
            if(currentWheel!= null)
            {
                PuzzlePiece _puzzlePiece = currentWheel.GetComponent<SteeringWheel>().correspondingPuzzlePiece;
                _puzzlePiece.RotatePiece();
            }

            Debug.LogFormat("<color=cyan>OnWheelRotationComplete : {0}</color>", wheelRotationCount);
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
        public void AssignPuzzlePieceSymbols(int _pieceIndex, string _symbolIndexString)
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

            //Assign random initial rotation
            puzzlePieces[_pieceIndex].SetInitialRotation();
        }
    }
}
