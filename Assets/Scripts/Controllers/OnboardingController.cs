using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CaveExplorer
{
    public enum OnboardingScenario
    {
        None,
        WelcomeExplorer,
        EnterLobby,
        WelcomeToLobby,
        RightTriggerToTeleport,
        LeftTriggerToCommunicate,
        BeMindfulOfOxygenLevel,
        CooperateWithEachOther,
        AllTheBest
    }

    public class OnboardingController : MonoBehaviour
    {
        [Header("VARIABLES")]
        [SerializeField] private float onboardingStartDelay;

        [Header("AUDIO CLIPS")]
        [SerializeField] private AudioClip welcomeExplorer;
        [SerializeField] private AudioClip enterLobby;
        [SerializeField] private AudioClip welcomeToLobby;
        [SerializeField] private AudioClip rightTriggerToTeleport;
        [SerializeField] private AudioClip leftTriggerToCommunicate;
        [SerializeField] private AudioClip beMindfulOfOxygenLevel;
        [SerializeField] private AudioClip cooperateWithEachOther;
        [SerializeField] private AudioClip allTheBest;

        [Header("TEXT OVERLAYS")]
        [SerializeField] private GameObject pressRightTriggerToSelectIndicator;
        [SerializeField] private GameObject pressRightTriggerToTeleportIndicator;
        [SerializeField] private GameObject pressRightGrabToInteractIndicator;
        [SerializeField] private GameObject pressLeftTriggerToCommunicateIndicator;
        [SerializeField] private GameObject oxygenLevelIndicator;
        [SerializeField] private GameObject radioBatteryIndicator;

        private AudioSource audioSource;
        private GameManager gameManager;
        private PlayerController playerController;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            gameManager = GameManager.Instance;
            playerController = GameManager.Instance.playerController;

            StartOnboarding();
        }

        public void StartOnboarding()
        {
            StartCoroutine(StartOnboardingAsync());
        }

        private IEnumerator StartOnboardingAsync()
        {
            yield return new WaitForSeconds(onboardingStartDelay);

            //Get player controller
            if(!playerController) playerController = GameManager.Instance.playerController;

            //Start Welcome scenario
            PlayOnboardingScenrio(OnboardingScenario.WelcomeExplorer);

            yield return new WaitForSeconds(welcomeExplorer.length + 0.5f);

            //Start Enter the lobby scenario
            PlayOnboardingScenrio(OnboardingScenario.EnterLobby);
            
            //Show Enter Lobby UI
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void PlayOnboardingScenrio(OnboardingScenario _scenario)
        {
            HideAllTextOverlays();
            StopOnboardingAudio();
            playerController.ToggleControllers(false, false);

            switch (_scenario)
            {
                case OnboardingScenario.None:
                    break;
                case OnboardingScenario.WelcomeExplorer:
                    PlayOnboardingAudio(welcomeExplorer);
                    break;
                case OnboardingScenario.EnterLobby:
                    pressRightTriggerToSelectIndicator.SetActive(true);
                    PlayOnboardingAudio(enterLobby);
                    //Show controllers
                    playerController.ToggleControllers(true, true);
                    break;
                case OnboardingScenario.WelcomeToLobby:
                    PlayOnboardingAudio(welcomeToLobby);
                    break;
                case OnboardingScenario.RightTriggerToTeleport:
                    PlayOnboardingAudio(rightTriggerToTeleport);
                    break;
                    case OnboardingScenario.LeftTriggerToCommunicate:
                    PlayOnboardingAudio(leftTriggerToCommunicate); 
                    break;
                case OnboardingScenario.BeMindfulOfOxygenLevel:
                    PlayOnboardingAudio(beMindfulOfOxygenLevel);
                    break;
                case OnboardingScenario.CooperateWithEachOther:
                    PlayOnboardingAudio(cooperateWithEachOther); 
                    break;
                case OnboardingScenario.AllTheBest:
                    PlayOnboardingAudio(allTheBest);
                    break;
                default:
                    break;
            }
        }

        public void PlayOnboardingAudio(AudioClip _clip, float _volume = 1f)
        {
            audioSource.Stop();
            audioSource.clip = _clip;
            audioSource.volume = _volume;
            audioSource.Play();
        }

        public void StopOnboardingAudio()
        {
            audioSource.Stop();
        }

        public void HideAllTextOverlays()
        {
            pressRightTriggerToSelectIndicator.SetActive(false);
            pressRightTriggerToTeleportIndicator.SetActive(false);
            pressRightGrabToInteractIndicator.SetActive(false);
            pressLeftTriggerToCommunicateIndicator.SetActive(false);
            oxygenLevelIndicator.SetActive(false);
            radioBatteryIndicator.SetActive(false);
        }
    }
}
