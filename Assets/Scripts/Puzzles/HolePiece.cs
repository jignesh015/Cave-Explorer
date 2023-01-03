using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CaveExplorer
{
    public class HolePiece : MonoBehaviour
    {
        public int pieceIndex;


        [Header("OUTLINE VARIABLES")]
        private Outline outline;
        [SerializeField] private Color outlineHoverColor;
        [SerializeField] private Color outlineSelectColor;
        [SerializeField] private float outlineHoverWidth;
        [SerializeField] private float outlineSelectWidth;

        private DoorHolePuzzle doorHolePuzzle;

        // Start is called before the first frame update
        void Start()
        {
            outline = GetComponent<Outline>();
            doorHolePuzzle = FindObjectOfType<DoorHolePuzzle>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <summary>
        /// Is called when the hole piece is grabbed
        /// </summary>
        public void OnPieceGrab()
        {
            //Adjust outline
            outline.OutlineColor = outlineSelectColor;
            outline.OutlineWidth = outlineSelectWidth;

            //Set as active hole piece
            doorHolePuzzle.SetActiveHolePiece(pieceIndex);
        }

        /// <summary>
        /// Is called when the hole piece is dropped
        /// </summary>
        public void OnPieceDrop()
        {
            //Adjust outline
            outline.OutlineColor = outlineHoverColor;
            outline.OutlineWidth = outlineHoverWidth;

            //Reset active hole piece
            doorHolePuzzle.SetActiveHolePiece();
        }
    }
}
