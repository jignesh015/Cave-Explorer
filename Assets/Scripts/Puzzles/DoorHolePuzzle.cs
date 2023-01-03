using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public class DoorHolePuzzle : Puzzle
    {
        [Header("HOLE POSITIONS")]
        [SerializeField] private List<Transform> holeSnapPositionTransforms;
        [SerializeField] private List<int> holeFilledIndex;

        [Header("HOLE PIECES")]
        [SerializeField] private List<HolePiece> holePieces;

        [Header("DEBUG ONLY")]
        [SerializeField] private HolePiece activeHolePiece;
        [SerializeField] private int interactingHolePositionIndex = -1;

        // Start is called before the first frame update
        void Start()
        {
            holeFilledIndex = new List<int> { -1, -1, -1, -1 };
        }

        // Update is called once per frame
        void Update()
        {
            //if(activeHolePiece != null && interactingHolePositionIndex != -1 
            //    && !isHoleFilled[interactingHolePositionIndex])
            //{
            //    SnapHolePieceInToHole(activeHolePiece.pieceIndex, interactingHolePositionIndex);
            //}
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

                //Reset active hole piece
                SetActiveHolePiece();
            }
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
        }
    }
}
