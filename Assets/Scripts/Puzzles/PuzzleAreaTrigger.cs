using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public class PuzzleAreaTrigger : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.LogFormat("<color=orange>PuzzleAreaTrigger Colliding with {0}</color>",
                other.name);
            if(other.CompareTag("Player"))
            {
                if (GameManager.Instance.onboardingController.currentScenario == OnboardingScenario.AllTheBest)
                    GameManager.Instance.onboardingController.PlayOnboardingScenrio(OnboardingScenario.GrabToInteract);
            }
        }
    }
}
