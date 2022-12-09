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
        [SerializeField] private List<SymbolPuzzle> symbols;

        [Header("STEERING WHEELS")]
        [SerializeField] private Transform currentWheel;
        [SerializeField] private float wheelAngle;
        [SerializeField] private Quaternion startWheelRot;
        [SerializeField] private Quaternion currentWheelRot;
        [SerializeField] private int wheelRotationCount;

        private bool completedHalfRotation;
        private bool completedFullRotation;
        // Start is called before the first frame update
        void Start()
        {
            
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
    }
}
