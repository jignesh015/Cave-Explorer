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
        ToggleGameMenu,
        AllTheBest,
        GrabToInteract
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
        [SerializeField] private AudioClip toggleGameMenu;
        [SerializeField] private AudioClip allTheBest;

        [Header("TEXT OVERLAYS")]
        [SerializeField] private GameObject pressRightTriggerToSelectIndicator;
        [SerializeField] private GameObject pressRightTriggerToTeleportIndicator;
        [SerializeField] private GameObject pressRightGrabToInteractIndicator;
        [SerializeField] private GameObject pressLeftTriggerToCommunicateIndicator;
        [SerializeField] private GameObject oxygenLevelIndicator;
        [SerializeField] private GameObject radioBatteryIndicator;
        [SerializeField] private GameObject yButtonIndicator;

        [HideInInspector] public OnboardingScenario currentScenario;

        private AudioSource audioSource;
        private GameManager gameManager;
        private PlayerController playerController;
        private LandingUIController landingUIController;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            gameManager = GameManager.Instance;
            playerController = GameManager.Instance.playerController;

            //Add listeners
            gameManager.OnPlayerTeleport.AddListener(OnPlayerTeleport);
            gameManager.OnVoiceChatEnabled.AddListener(OnVoiceChatEnabled);
            gameManager.OnGameMenuOpened.AddListener(OnGameMenuOpened);
            gameManager.OnGrabButtonPressed.AddListener(OnGrabPressed);

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
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        #region ON TELEPORT
        private void OnPlayerTeleport()
        {
            //If player has teleported, play the next scenario
            if (currentScenario == OnboardingScenario.RightTriggerToTeleport)
                StartCoroutine(OnPlayerTeleportAsync());
        }

        private IEnumerator OnPlayerTeleportAsync()
        {
            yield return new WaitForSeconds(1f);

            if (currentScenario == OnboardingScenario.RightTriggerToTeleport)
                PlayOnboardingScenrio(OnboardingScenario.LeftTriggerToCommunicate);
        }
        #endregion

        #region ON VOICE CHAT 
        private void OnVoiceChatEnabled()
        {
            //If player enabled voice chat, play the next scenario
            if (currentScenario == OnboardingScenario.LeftTriggerToCommunicate)
                StartCoroutine(OnVoiceChatEnabledAsync());
        }

        private IEnumerator OnVoiceChatEnabledAsync()
        {
            yield return new WaitForSeconds(0.1f);

            if (currentScenario == OnboardingScenario.LeftTriggerToCommunicate)
                PlayOnboardingScenrio(OnboardingScenario.BeMindfulOfOxygenLevel);

            yield return new WaitForSeconds(beMindfulOfOxygenLevel.length + 2.5f);

            PlayOnboardingScenrio(OnboardingScenario.CooperateWithEachOther);

            yield return new WaitForSeconds(cooperateWithEachOther.length + 1f);

            PlayOnboardingScenrio(OnboardingScenario.ToggleGameMenu);
        }
        #endregion

        #region ON GAME MENU OPEN
        private void OnGameMenuOpened()
        {
            //If player opened game menu, play the next scenario
            if (currentScenario == OnboardingScenario.ToggleGameMenu)
                StartCoroutine(OnGameMenuOpenedAsync());
        }

        private IEnumerator OnGameMenuOpenedAsync()
        {
            yield return new WaitForSeconds(3f);

            if (currentScenario == OnboardingScenario.ToggleGameMenu)
                PlayOnboardingScenrio(OnboardingScenario.AllTheBest);
        }
        #endregion

        #region ON GRAB PRESS
        private void OnGrabPressed()
        {
            if (currentScenario == OnboardingScenario.GrabToInteract)
                PlayOnboardingScenrio(OnboardingScenario.None);
        }
        #endregion

        public void PlayOnboardingScenrio(OnboardingScenario _scenario)
        {
            if (currentScenario == _scenario) return;
            currentScenario = _scenario;
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
                    if(FindObjectOfType<LandingUIController>() != null)
                        FindObjectOfType<LandingUIController>().PlayUIAnimation("ShowStart");
                    pressRightTriggerToSelectIndicator.SetActive(true);
                    PlayOnboardingAudio(enterLobby);
                    playerController.ToggleControllers(false, true);
                    break;
                case OnboardingScenario.WelcomeToLobby:
                    PlayOnboardingAudio(welcomeToLobby);
                    break;
                case OnboardingScenario.RightTriggerToTeleport:
                    PlayOnboardingAudio(rightTriggerToTeleport);
                    playerController.ToggleWalkieTalkie(false);
                    playerController.TogglePlayerHandCanvas(false);
                    playerController.ToggleControllers(false, true);
                    pressRightTriggerToTeleportIndicator.SetActive(true);
                    break;
                    case OnboardingScenario.LeftTriggerToCommunicate:
                    PlayOnboardingAudio(leftTriggerToCommunicate);
                    playerController.ToggleControllers(true, false);
                    pressLeftTriggerToCommunicateIndicator.SetActive(true);
                    break;
                case OnboardingScenario.BeMindfulOfOxygenLevel:
                    PlayOnboardingAudio(beMindfulOfOxygenLevel);
                    oxygenLevelIndicator.SetActive(true);
                    radioBatteryIndicator.SetActive(true);
                    playerController.TogglePlayerHandCanvas(true);
                    playerController.ToggleWalkieTalkie(true);
                    break;
                case OnboardingScenario.CooperateWithEachOther:
                    PlayOnboardingAudio(cooperateWithEachOther); 
                    break;
                case OnboardingScenario.ToggleGameMenu:
                    PlayOnboardingAudio(toggleGameMenu);
                    yButtonIndicator.SetActive(true);
                    playerController.ToggleControllers(true, false);
                    playerController.TogglePlayerHandCanvas(false);
                    playerController.ToggleWalkieTalkie(false);
                    break;
                case OnboardingScenario.AllTheBest:
                    PlayOnboardingAudio(allTheBest);
                    playerController.TogglePlayerHandCanvas(true);
                    playerController.ToggleWalkieTalkie(true);
                    gameManager.hasCompletedOnboarding = true;
                    break;
                case OnboardingScenario.GrabToInteract:
                    pressRightGrabToInteractIndicator.SetActive(true);
                    playerController.ToggleControllers(false, true);
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
            yButtonIndicator.SetActive(false);
        }
    }
}
