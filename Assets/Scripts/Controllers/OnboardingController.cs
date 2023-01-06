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
        [SerializeField] private GameObject pressRightTriggerIndicator;
        [SerializeField] private GameObject pressLeftTriggerIndicator;
        [SerializeField] private GameObject oxygenAndRadioBatteryIndicator;

        private AudioSource audioSource;
        private GameManager gameManager;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            gameManager = GameManager.Instance;

            StartOnboarding();
        }

        public void StartOnboarding()
        {
            StartCoroutine(StartOnboardingAsync());
        }

        private IEnumerator StartOnboardingAsync()
        {
            yield return new WaitForSeconds(onboardingStartDelay);

            //Start Welcome scenario
            PlayOnboardingScenrio(OnboardingScenario.WelcomeExplorer);

            yield return new WaitForSeconds(welcomeExplorer.length + 0.5f);

            //Start Enter the lobby scenario
            PlayOnboardingScenrio(OnboardingScenario.EnterLobby);
            
            //Show UI for Entering the Lobby
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void PlayOnboardingScenrio(OnboardingScenario _scenario)
        {
            //HideAllTextOverlays();
            StopOnboardingAudio();

            switch (_scenario)
            {
                case OnboardingScenario.None:
                    break;
                case OnboardingScenario.WelcomeExplorer:
                    PlayOnboardingAudio(welcomeExplorer);
                    break;
                case OnboardingScenario.EnterLobby:
                    PlayOnboardingAudio(enterLobby);
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
            pressRightTriggerIndicator.SetActive(false);
            pressLeftTriggerIndicator.SetActive(false);
            oxygenAndRadioBatteryIndicator.SetActive(false);
        }
    }
}
