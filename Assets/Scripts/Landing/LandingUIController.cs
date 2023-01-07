using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public class LandingUIController : MonoBehaviour
    {
        [SerializeField] private Animator gameTitleAnimator;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void PlayUIAnimation(string _trigger)
        {
            gameTitleAnimator.SetTrigger(_trigger);
        }

        public void OnStartButtonPressed()
        {
            GameManager.Instance.FadeToBlack();

            //Fade-out BGM
            GameManager.Instance.bgmController.LerpBGMVolume(0.02f, 1);

            //Change onboarding scenario to none
            GameManager.Instance.onboardingController.PlayOnboardingScenrio(OnboardingScenario.None);

            //Try connecting to server
            FindObjectOfType<NetworkManager>().ConnectToServer();

        }
    }
}
