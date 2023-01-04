using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
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
        [SerializeField] private List<Rigidbody> steeringWheelRigidBody;
        [SerializeField] private Transform currentWheel;
        [SerializeField] private float wheelAngle;
        [SerializeField] private Quaternion startWheelRot;
        [SerializeField] private Quaternion currentWheelRot;
        [SerializeField] private int wheelRotationCount;

        [Header("SFX")]
        [SerializeField] private AudioClip puzzleSolvedSFX;
        [SerializeField] private AudioClip doorSlideSFX;

        [Header("PUZZLE DOOR MOVEMENT")]
        [SerializeField] private float moveOffset;
        [SerializeField] private float moveDuration;

        [Header("DEBUG ONLY")]
        [SerializeField] private bool puzzleSolvedDebug;

        private bool completedHalfRotation;
        private bool completedFullRotation;

        private bool isPuzzleSolved;

        private AudioSource audioSource;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();

            InitializePuzzle();
        }

        private void OnEnable()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnCustomEvent;
        }

        private void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnCustomEvent;
        }

        /// <summary>
        /// Initializes the puzzle and syncs it for all the players
        /// </summary>
        public void InitializePuzzle()
        {
            if (!GameManager.Instance.isPlayer1)
                return;

            //Reset bools
            isPuzzleSolved = false;
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
                Debug.LogFormat("<color=olive>_symbolIndexString {0}</color>", _symbolIndexString);

                //Set index for random initial rotation
                int _player1RandIndex = Random.Range(0, 4);
                int _player2RandIndex = Random.Range(0, 4);
                while (_player1RandIndex == _player2RandIndex)
                {
                    _player2RandIndex = Random.Range(0, 4);
                }

                //Raise Photon Event for assigning puzzle piece symbol
                RaiseCustomEvent(StaticData.AssignPuzzlePieceSymbolsEventCode,
                   new object[] { i, _symbolIndexString, _player1RandIndex, _player2RandIndex });
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(puzzleSolvedDebug)
            {
                isPuzzleSolved= false;
                PuzzleSolved();
            }

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

            //Raise photon event for Checking if puzzle solved
            string _symbolString = GetSymbolIndexString();
            RaiseCustomEvent(StaticData.CheckIfSWPuzzleSolvedEventCode,
                   new object[] { GameManager.Instance.isPlayer1, _symbolString });
        }

        /// <summary>
        /// Is called when Select Enter event is triggered on the steering wheel
        /// </summary>
        /// <param name="args"></param>
        public void OnWheelGrab(SelectEnterEventArgs args)
        {
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

        public void PlaySFX(AudioClip _clip)
        {
            audioSource.Stop();
            audioSource.clip = _clip;
            audioSource.Play();
        }

        public void StopSFX()
        {
            audioSource.Stop();
        }

        /// <summary>
        /// This function receives the custom RPC events sent by any player
        /// </summary>
        /// <param name="photonEvent"></param>
        public void OnCustomEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            if (eventCode == StaticData.AssignPuzzlePieceSymbolsEventCode)
                AssignPuzzlePieceSymbols((object[])photonEvent.CustomData);
            else if(eventCode == StaticData.CheckIfSWPuzzleSolvedEventCode)
                CheckIfPuzzleSolved((object[])photonEvent.CustomData);
            else if (eventCode == StaticData.SWPuzzleSolvedEventCode)
                PuzzleSolved();
        }

        /// <summary>
        /// Assigns symbols to each puzzle piece
        /// </summary>
        public void AssignPuzzlePieceSymbols(object[] _content)
        {
            //Extract data from custom event object
            int _pieceIndex = (int)_content[0];
            string _symbolIndexString = (string)_content[1];
            int _player1RotIndex = (int)_content[2];
            int _player2RotIndex = (int)_content[3];

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
        ///  Compares the symbol index for all the players
        ///  and checks if the puzzle is solved
        /// </summary>
        public void CheckIfPuzzleSolved(object[] _content)
        {
            //Extract data from custom event object
            bool _isPlayer1 = (bool)_content[0];
            string _symbolIndexString = (string)(_content[1]);

            if (GameManager.Instance.isPlayer1 != _isPlayer1)
            {
                if (_symbolIndexString.Equals(GetSymbolIndexString()))
                {
                    //Puzzle is solved
                    Debug.LogFormat("<color=green>PUZZLE IS SOLVED</color>");

                    //Raise photon event to notify each player
                    RaiseCustomEvent(StaticData.SWPuzzleSolvedEventCode, null);
                }
                else
                {
                    Debug.LogFormat("<color=yellow>PUZZLE NOT SOLVED</color>");
                }
            }
        }

        /// <summary>
        /// Is called when the puzzle is solved
        /// </summary>
        public void PuzzleSolved()
        {
            if (isPuzzleSolved) return;

            isPuzzleSolved = true;
            puzzleSolvedDebug = false;
            PlaySFX(puzzleSolvedSFX);

            //Lerp Puzzle piece to side
            StartCoroutine(MovePuzzleDoorToSide());
        }

        /// <summary>
        /// Lerps puzzle door to the side on puzzle completion
        /// </summary>
        /// <returns></returns>
        private IEnumerator MovePuzzleDoorToSide()
        {
            //Disable right hand direct interactor
            GameManager.Instance.playerController.ToggleXRDirectInteractor(false);

            //Make all rigid bodies kinematic
            foreach(Rigidbody _rb in steeringWheelRigidBody)
            {
                _rb.isKinematic = true;
            }

            yield return new WaitForSeconds(1.5f);

            PlaySFX(doorSlideSFX);

            float startTime = Time.time;
            Vector3 _startPos = transform.localPosition;
            Vector3 _endPos = new Vector3(_startPos.x, _startPos.y, _startPos.z - moveOffset);

            while (Time.time < startTime + moveDuration)
            {
                transform.localPosition = Vector3.Lerp(_startPos, _endPos, (Time.time - startTime) / moveDuration);
                yield return null;
            }
            transform.localPosition = _endPos;
            StopSFX();

            yield return new WaitForSeconds(0.25f);

            //Fade to black
            GameManager.Instance.FadeToBlack();
        }
    }
}
