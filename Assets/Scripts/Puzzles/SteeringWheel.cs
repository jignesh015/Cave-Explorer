using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace CaveExplorer
{
    public class SteeringWheel : MonoBehaviour
    {
        public PuzzlePiece correspondingPuzzlePiece;


        [Header("OUTLINE VARIABLES")]
        [SerializeField] private Color outlineHoverColor;
        [SerializeField] private Color outlineSelectColor;
        [SerializeField] private float outlineHoverWidth;
        [SerializeField] private float outlineSelectWidth;
        private Outline colliderOutline;

        private void Start()
        {
            colliderOutline = GetComponentInChildren<Outline>();
            if(colliderOutline != null)
            {
                colliderOutline.enabled = false;
            }
        }

        /// <summary>
        /// Is called when the wheel is grabbed
        /// </summary>
        public void OnWheelGrab()
        {
            //Adjust outline
            colliderOutline.OutlineColor = outlineSelectColor;
            colliderOutline.OutlineWidth = outlineSelectWidth;
        }

        /// <summary>
        /// Is called when the wheel is dropped
        /// </summary>
        public void OnWheelRelease()
        {
            //Adjust outline
            colliderOutline.OutlineColor = outlineHoverColor;
            colliderOutline.OutlineWidth = outlineHoverWidth;
        }
    }
}
