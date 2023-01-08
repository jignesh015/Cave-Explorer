using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

namespace CaveExplorer
{
    public class DoorHolePuzzle : Puzzle
    {
        [Header("SYMBOL LIST")]
        [SerializeField] private List<PuzzleSymbol> symbols;

        [Header("HOLE PIECES")]
        [SerializeField] private List<HolePiece> holePieces;

        [Header("HOLE POSITIONS")]
        [SerializeField] private List<Transform> holeSnapPositionTransforms;
        [SerializeField] private List<int> holeFilledIndex;

        [Header("REMOTE PLAYER SYMBOLS")]
        [SerializeField] private List<MeshRenderer> remotePlayerSymbolMesh;

        [Header("SFX")]
        [SerializeField] private AudioClip holeHighlightSFX;
        [SerializeField] private AudioClip snapToHoleSFX;
        [SerializeField] private AudioClip puzzleSolvedSFX;
        [SerializeField] private AudioClip doorSlideSFX;

        [Header("PUZZLE DOOR MOVEMENT")]
        [SerializeField] private float moveOffset;
        [SerializeField] private float moveDuration;

        [Header("DYNAMIC VARIABLES")]
        [SerializeField] private HolePiece activeHolePiece;
        [SerializeField] private int interactingHolePositionIndex = -1;

        [Header("DEBUG ONLY")]
        [SerializeField] private bool puzzleSolvedDebug;

        private string localPlayerSymbolIndexString;
        private string remotePlayerSymbolIndexString;

        private bool isPuzzleSolved;
        private AudioSource audioSource;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            holeFilledIndex = new List<int> { -1, -1, -1, -1 };

            //Invoke(nameof(InitializePuzzle), 10f);
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

        // Update is called once per frame
        void Update()
        {
            if (puzzleSolvedDebug)
            {
                isPuzzleSolved = false;
                PuzzleSolved();
            }
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

            //Generate list of 4 random symbols for player 1 and player 2
            List<int> _symbolIndexListPlayer1 = new List<int>();
            List<int> _symbolIndexListPlayer2 = new List<int>();
            for(int i = 0; i < 4; i++)
            {
                int _rand1 = Random.Range(0, symbols.Count);
                int _rand2 = Random.Range(0, symbols.Count);
                while (_symbolIndexListPlayer1.Contains(_rand1) 
                    || _symbolIndexListPlayer2.Contains(_rand1))
                {
                    _rand1 = Random.Range(0, symbols.Count);
                }
                while(_symbolIndexListPlayer1.Contains(_rand2)
                    || _symbolIndexListPlayer2.Contains(_rand2))
                {
                    _rand2 = Random.Range(0, symbols.Count);
                }
                _symbolIndexListPlayer1.Add(_rand1);
                _symbolIndexListPlayer2.Add(_rand2);
            }

            //Convert these list to string to send it over RPC
            string _symbolIndexStringPlayer1 = string.Join(",", _symbolIndexListPlayer1);
            string _symbolIndexStringPlayer2 = string.Join(",", _symbolIndexListPlayer2);
            Debug.LogFormat("<color=olive>_symbolIndexString P1 {0} | P2 {1}</color>",
                _symbolIndexStringPlayer1, _symbolIndexStringPlayer2);

            //Raise Photon Event for assigning hole piece symbols
            RaiseCustomEvent(StaticData.AssignHolePieceSymbolEventCode,
                new object[] { _symbolIndexStringPlayer1, _symbolIndexStringPlayer2 });
        }

        /// <summary>
        /// Snaps the given hole piece into the given hole position
        /// </summary>
        /// <param name="_holePieceIndex"></param>
        /// <param name="_holeTriggerIndex"></param>
        public void SnapHolePieceInToHole(int _holePieceIndex, int _holeTriggerIndex)
        {
            if (holeFilledIndex[_holeTriggerIndex] == -1)
            {
                Transform _pieceToSnap = holePieces[_holePieceIndex].transform;
                _pieceToSnap.position = holeSnapPositionTransforms[_holeTriggerIndex].position;

                //Set hole to filled
                holeFilledIndex[_holeTriggerIndex] = _holePieceIndex;

                //Stop hole highlight
                ToggleHoleHighlight();

                //Play snapping SFX
                PlaySFX(snapToHoleSFX);

                //Reset active hole piece
                SetActiveHolePiece();

                //Check for puzzle complete by sending
                //the current hole filled index string as photon event
                string _holeFilledIndexString = GetHoleFilledSymbolIndexString();
                RaiseCustomEvent(StaticData.CheckIfDHPuzzleSolvedEventCode,
                    new object[] { GameManager.Instance.isPlayer1, _holeFilledIndexString });
            }
        }

        /// <summary>
        /// Returns index string of the symbols currently in the holes
        /// </summary>
        /// <returns></returns>
        private string GetHoleFilledSymbolIndexString()
        {
            string _currentSymbolIndexString = string.Empty;
            for(int i = 0; i < holeFilledIndex.Count; i++)
            {
                if (holeFilledIndex[i] != -1)
                {
                    _currentSymbolIndexString += holePieces[holeFilledIndex[i]].assignedSymbol.symbolIndex + ",";
                }
                else
                {
                    _currentSymbolIndexString += "-1,";
                }
            }
            _currentSymbolIndexString = _currentSymbolIndexString.Remove(_currentSymbolIndexString.Length - 1, 1);
            return _currentSymbolIndexString;
        }

        /// <summary>
        /// Sets the active hole piece
        /// </summary>
        /// <param name="_index"></param>
        public void SetActiveHolePiece(int _index = -1)
        {
            //If hole piece is colliding with the trigger on being dropped
            //Snap the hole piece into the corresponding hole
            if(_index == -1 && activeHolePiece != null && interactingHolePositionIndex != -1
                && holeFilledIndex[interactingHolePositionIndex] == -1)
            {
                SnapHolePieceInToHole(activeHolePiece.pieceIndex, interactingHolePositionIndex);
            }
            else
            {
                //Else, set the active hole piece
                activeHolePiece = _index == -1 ? null : holePieces[_index];
            }

        }

        /// <summary>
        /// Sets the index of the currently interacting hole
        /// </summary>
        /// <param name="_index"></param>
        public void SetInteractingHolePositionIndex(int _index)
        {
            interactingHolePositionIndex = _index;

            //Highlight the hole if it is empty
            if (holeFilledIndex[_index] == -1)
            {
                PlaySFX(holeHighlightSFX);
                ToggleHoleHighlight(_index);
            }
        }

        /// <summary>
        /// Resets the index of the currently interacting hole
        /// </summary>
        /// <param name="_index"></param>
        public void ResetInteractingHolePositionIndex(int _index)
        {
            //While reseting, if an hole piece is active and the corresponding hole is filled,
            //Then empty the hole
            if(activeHolePiece != null && holeFilledIndex[_index] == activeHolePiece.pieceIndex)
            {
                holeFilledIndex[_index] = -1;
            }

            interactingHolePositionIndex = -1;
            ToggleHoleHighlight();
        }

        /// <summary>
        /// Toggles hole highlight on/off
        /// | -1 = Off |
        /// </summary>
        /// <param name="state"></param>
        private void ToggleHoleHighlight(int _holeIndex = -1)
        {
            foreach(Transform _holeTransform in holeSnapPositionTransforms)
            {
                _holeTransform.GetComponentInChildren<MeshRenderer>().enabled = false;
            }
            if(_holeIndex != -1)
                holeSnapPositionTransforms[_holeIndex].GetComponentInChildren<MeshRenderer>().enabled = true;
        }

        private void AssignRemotePlayerSymbols(int _index, PuzzleSymbol _symbol)
        {
            Material[] _matArray = remotePlayerSymbolMesh[_index].materials;
            for (int i = 0; i < _matArray.Length; i++)
            {
                _matArray[i] = _symbol.symbolMaterial;
            }
            remotePlayerSymbolMesh[_index].materials = _matArray;
        }

        /// <summary>
        /// This function receives the custom RPC events sent by any player
        /// </summary>
        /// <param name="photonEvent"></param>
        public void OnCustomEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            if (eventCode == StaticData.AssignHolePieceSymbolEventCode)
                AssignHolePieceSymbols((object[])photonEvent.CustomData);
            else if (eventCode == StaticData.CheckIfDHPuzzleSolvedEventCode)
                CheckIfPuzzleSolved((object[])photonEvent.CustomData);
            else if (eventCode == StaticData.DHPuzzleSolvedEventCode)
                PuzzleSolved();
        }

        /// <summary>
        /// Assigns symbols to each hole piece
        /// </summary>
        /// <param name="_content"></param>
        private void AssignHolePieceSymbols(object[] _content)
        {
            //Extract data from custom event object
            string _symbolIndexStringPlayer1 = (string)_content[0];
            string _symbolIndexStringPlayer2 = (string)_content[1];

            //Save symbolIndexString for local & remote player
            localPlayerSymbolIndexString = GameManager.Instance.isPlayer1 ? _symbolIndexStringPlayer1 :
                _symbolIndexStringPlayer2;
            remotePlayerSymbolIndexString = GameManager.Instance.isPlayer1 ? _symbolIndexStringPlayer2 :
                _symbolIndexStringPlayer1;

            //Convert from string to index list
            List<string> _symbolIndexListPlayer1 = _symbolIndexStringPlayer1.Split(',').ToList();
            List<string> _symbolIndexListPlayer2 = _symbolIndexStringPlayer2.Split(',').ToList();

            Debug.LogFormat("<color=magenta> OnCustomEvent _symbolIndexString P1 {0} | P2 {1}</color>",
                _symbolIndexStringPlayer1, _symbolIndexStringPlayer2);

            List<PuzzleSymbol> _symbolsToAssignPlayer1 = new List<PuzzleSymbol>();
            List<PuzzleSymbol> _symbolsToAssignPlayer2 = new List<PuzzleSymbol>();
            for(int i = 0; i < _symbolIndexListPlayer1.Count; i++)
            {
                _symbolsToAssignPlayer1.Add(symbols[int.Parse(_symbolIndexListPlayer1[i])]);
                _symbolsToAssignPlayer2.Add(symbols[int.Parse(_symbolIndexListPlayer2[i])]);

                //Assign symbol to hole piece for respective player
                if(GameManager.Instance.isPlayer1)
                {
                    //Assign for player 1
                    holePieces[i].AssignSymbolMaterials(symbols[int.Parse(_symbolIndexListPlayer1[i])]);

                    //Assign remote player symbols
                    AssignRemotePlayerSymbols(i, symbols[int.Parse(_symbolIndexListPlayer2[i])]);
                }
                else
                {
                    //Assign for player 2
                    holePieces[i].AssignSymbolMaterials(symbols[int.Parse(_symbolIndexListPlayer2[i])]);

                    //Assign remote player symbols
                    AssignRemotePlayerSymbols(i, symbols[int.Parse(_symbolIndexListPlayer1[i])]);
                }
            }
        }

        /// <summary>
        /// Checks if all the players have placed the puzzle pieces 
        /// in the corresponding holes
        /// </summary>
        /// <param name="_content"></param>
        private void CheckIfPuzzleSolved(object[] _content)
        {
            //Extract data from custom event object
            bool _isPlayer1 = (bool)_content[0];
            string _holeFilledIndexString = (string)(_content[1]);

            Debug.LogFormat("<color=cyan> ---- CheckIfPuzzleSolved ---- </color>");

            bool _localPlayerPuzzleSolved = false;
            bool _remotePlayerPuzzleSolved = false;

            //First, check if local player's puzzle is solved
            string _localPlayerHoleFilledIndexString = GetHoleFilledSymbolIndexString();
            _localPlayerPuzzleSolved = _localPlayerHoleFilledIndexString.Equals(localPlayerSymbolIndexString);
            Debug.LogFormat("<color=cyan> _localPlayerHoleFilledIndexString {0} | localPlayerSymbolIndexString {1}</color>",
                _localPlayerHoleFilledIndexString, localPlayerSymbolIndexString);

            //Now, check if remote player's puzzle is solved
            if (GameManager.Instance.isPlayer1 != _isPlayer1)
            {
                _remotePlayerPuzzleSolved = _holeFilledIndexString.Equals(remotePlayerSymbolIndexString);
                Debug.LogFormat("<color=cyan> _remotePlayerHoleFilledIndexString {0} | remotePlayerSymbolIndexString {1}</color>",
                    _holeFilledIndexString, remotePlayerSymbolIndexString);
            }

            //If both the puzzles are solved, proceed
            if(_localPlayerPuzzleSolved && _remotePlayerPuzzleSolved)
            {
                //Puzzle is solved
                Debug.LogFormat("<color=green>DOOR HOLE PUZZLE IS SOLVED</color>");

                //Raise photon event to notify each player
                RaiseCustomEvent(StaticData.DHPuzzleSolvedEventCode, null);
            }
            else
            {
                Debug.LogFormat("<color=yellow>DOOR HOLE PUZZLE NOT YET SOLVED</color>");
            }
        }

        /// <summary>
        /// Is called when the puzzle is solved
        /// </summary>
        private void PuzzleSolved()
        {
            if (isPuzzleSolved) return;

            isPuzzleSolved = true;
            puzzleSolvedDebug = false;
            PlaySFX(puzzleSolvedSFX);

            //Lerp Door to side
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

            yield return new WaitForSeconds(1.5f);

            PlaySFX(doorSlideSFX);

            float startTime = Time.time;
            Vector3 _startPos = transform.localPosition;
            Vector3 _endPos = new Vector3(_startPos.x + moveOffset, _startPos.y, _startPos.z);

            while (Time.time < startTime + moveDuration)
            {
                transform.localPosition = Vector3.Lerp(_startPos, _endPos, (Time.time - startTime) / moveDuration);
                yield return null;
            }
            transform.localPosition = _endPos;
            StopSFX();

            yield return new WaitForSeconds(0.25f);

            //Fade to black
            //GameManager.Instance.FadeToBlack();

            //Raise event for Game Complete
            GameManager.Instance.RaiseCustomEvent(StaticData.GameCompleteEventCode, null);
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
    }
}
