using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public class LandingUIController : MonoBehaviour
    {
        [SerializeField] private Animator gameTitleAnimator;

        [SerializeField] private GameObject gameTitlePanel;
        [SerializeField] private GameObject networkErrorPanel;

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

            
            NetworkManager netManager = FindObjectOfType<NetworkManager>();
            if(netManager != null)
            {
                //Set network error callback
                netManager.networkErrorCallback = OnNetworkError;

                //Try connecting to server
                netManager.ConnectToServer();
            }
            else
            {
                OnNetworkError();
            }
        }

        public void OnNetworkError()
        {
            //Show network error panel
            networkErrorPanel.SetActive(true);
            gameTitlePanel.SetActive(false);

            //Fade to normal
            GameManager.Instance.FadeToNormal();
        }
    }
}
