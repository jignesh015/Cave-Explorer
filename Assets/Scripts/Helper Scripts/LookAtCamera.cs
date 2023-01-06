using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public class LookAtCamera : MonoBehaviour
    {
        Transform camTransform;

        // Start is called before the first frame update
        void Start()
        {
            camTransform = Camera.main.transform;
        }

        // Update is called once per frame
        void Update()
        {
            transform.forward = camTransform.forward;
        }
    }
}
