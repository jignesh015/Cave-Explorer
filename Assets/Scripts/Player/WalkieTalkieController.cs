using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public class WalkieTalkieController : MonoBehaviour
    {
        [SerializeField] private AudioClip radioStaticSFX;
        [SerializeField] private AudioClip radioOffSFX;

        private Animator animator;
        private AudioSource audioSource;

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
    }
}
