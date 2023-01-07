using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CaveExplorer
{
    public class BackgroundSoundController : MonoBehaviour
    {
        [Header("BGM MUSIC")]
        [SerializeField] private AudioClip ominousEntry;


        private AudioSource audioSource;
        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            PlayBGM(0);
        }

        /// <summary>
        /// Plays the given BGM
        /// </summary>
        /// <param name="_index"></param>
        /// <param name="_volume"></param>
        public void PlayBGM(int _index, float _volume = 0.1f)
        {
            switch(_index)
            {
                case 0:
                    PlayAudio(ominousEntry, _volume); 
                    break;
            }
        }

        private void PlayAudio(AudioClip _clip, float _volume)
        {
            audioSource.clip = _clip;
            audioSource.volume = _volume;
            audioSource.loop = true;
            audioSource.Play();
        }


        /// <summary>
        /// Lerps the BGM volume to give a fade-in/fade-out effect
        /// </summary>
        /// <param name="_volume"></param>
        /// <param name="_lerpTime"></param>
        public void LerpBGMVolume(float _volume, float _lerpTime = 1f)
        {
            StartCoroutine(LerpBGMVolumeAsync(_volume, _lerpTime));
        }

        private IEnumerator LerpBGMVolumeAsync(float _volume, float _lerpTime)
        {
            float startTime = Time.time;
            float _startVol = audioSource.volume;
            float _endVol = _volume;

            while (Time.time < startTime + _lerpTime)
            {
                audioSource.volume = Mathf.Lerp(_startVol, _endVol, (Time.time - startTime) / _lerpTime);
                yield return null;
            }
            audioSource.volume = _endVol;
        }
    }
}
