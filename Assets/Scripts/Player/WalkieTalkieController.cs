using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CaveExplorer
{
    public class WalkieTalkieController : MonoBehaviour
    {
        [SerializeField] private AudioClip radioStaticSFX;
        [SerializeField] private AudioClip radioOffSFX;

        [SerializeField] private List<Image> batteryBars; 

        private Animator animator;
        private AudioSource audioSource;

        private int currentBatteryLevel = 3;

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <summary>
        /// Toggles the LED material on the walkie talkie
        /// </summary>
        /// <param name="state"></param>
        public void ToggleWalkieTalkieLED(bool _turnOn)
        {
            if (animator == null) animator = GetComponent<Animator>();
            animator.SetTrigger(_turnOn ? "LED_On" : "LED_Off");
            PlaySFX(_turnOn ? radioStaticSFX : radioOffSFX);
        }

        /// <summary>
        /// Plays the given clip
        /// </summary>
        /// <param name="_clip"></param>
        public void PlaySFX(AudioClip _clip)
        {
            if(audioSource == null) audioSource = GetComponent<AudioSource>();
            audioSource.clip = _clip;
            audioSource.Play();
        }

        public void UpdateBatteryLevel(float _batteryPercentage)
        {
            if (_batteryPercentage <= 0)
                DisplayBatteryLevel(0);
            else if(_batteryPercentage <= 33)
                DisplayBatteryLevel(1);
            else if(_batteryPercentage <= 66)
                DisplayBatteryLevel(2);
            else if(_batteryPercentage > 66)
                DisplayBatteryLevel(3);

        }

        private void DisplayBatteryLevel(int _batteryLevel)
        {
            if(currentBatteryLevel == _batteryLevel) return;
            currentBatteryLevel = _batteryLevel;
            for(int i = 0; i < batteryBars.Count; i++)
            {
                if(i < _batteryLevel)
                {
                    batteryBars[i].enabled = true;
                }
                else
                {
                    batteryBars[i].enabled = false;
                }
            }
        }
    }
}
