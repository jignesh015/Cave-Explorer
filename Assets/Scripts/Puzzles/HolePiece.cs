using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CaveExplorer
{
    public class HolePiece : MonoBehaviour
    {
        public int pieceIndex;
        public PuzzleSymbol assignedSymbol;

        [Header("OUTLINE VARIABLES")]
        private Outline outline;
        [SerializeField] private Color outlineHoverColor;
        [SerializeField] private Color outlineSelectColor;
        [SerializeField] private float outlineHoverWidth;
        [SerializeField] private float outlineSelectWidth;

        private MeshRenderer holePieceRenderer;
        private DoorHolePuzzle doorHolePuzzle;

        // Start is called before the first frame update
        void Start()
        {
            outline = GetComponent<Outline>();
            holePieceRenderer = GetComponentInChildren<MeshRenderer>();
            doorHolePuzzle = FindObjectOfType<DoorHolePuzzle>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void AssignSymbolMaterials(PuzzleSymbol _symbol)
        {
            assignedSymbol = _symbol;
            Material[] _matArray = holePieceRenderer.materials;
            for (int i = 1; i < 5; i++)
            {
                _matArray[i] = _symbol.symbolMaterial;
            }
            holePieceRenderer.materials = _matArray;
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
