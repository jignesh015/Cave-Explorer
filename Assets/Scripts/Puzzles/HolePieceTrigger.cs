using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public class HolePieceTrigger : MonoBehaviour
    {
        public int triggerIndex;
        private DoorHolePuzzle doorHolePuzzle;

        // Start is called before the first frame update
        void Start()
        {
            doorHolePuzzle = FindObjectOfType<DoorHolePuzzle>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnTriggerEnter(Collider other)
        {
            if(doorHolePuzzle == null)
            {
                doorHolePuzzle = FindObjectOfType<DoorHolePuzzle>();
            }

            Debug.LogFormat("<color=orange>Colliding with {0}</color>", other.name);

            HolePiece _holePiece = other.GetComponent<HolePiece>();
            if (_holePiece)
            {
                doorHolePuzzle.SetInteractingHolePositionIndex(triggerIndex);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (doorHolePuzzle == null)
            {
                doorHolePuzzle = FindObjectOfType<DoorHolePuzzle>();
            }

            HolePiece _holePiece = other.GetComponent<HolePiece>();
            if (_holePiece)
            {
                doorHolePuzzle.ResetInteractingHolePositionIndex(triggerIndex);
            }
        }
    }
}
