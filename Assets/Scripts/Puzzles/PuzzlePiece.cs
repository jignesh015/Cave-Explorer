using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

namespace CaveExplorer
{
    public class PuzzlePiece : MonoBehaviour
    {
        [Header("PIECE MOVEMENT REFERENCES")]
        [SerializeField] private float movementOffset;
        [SerializeField] private float pushMovementTime;
        [SerializeField] private float resetMovementTime;
        [SerializeField] private Vector3 startPosition;
        [SerializeField] private Vector3 endPosition;

        [Header("SFX")]
        [SerializeField] private AudioClip pushSFX;
        [SerializeField] private AudioClip resetSFX;

        private AudioSource audioSource;
        private Transform pieceToMove;

        private bool isMoving;
        private bool isPushed;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            pieceToMove = transform.GetChild(0);

            startPosition = pieceToMove.localPosition;
            endPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z + movementOffset);
        }

        // Update is called once per frame
        void Update()
        {
            //endPosition = transform.localPosition;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.LogFormat("<color=red>COLLIDED WITH {0} </color>", collision.gameObject.name);
            if(!isMoving && !isPushed)
            {
                PushPiece();
            }
        }

        private void PushPiece()
        {
            StartCoroutine(MovePiece(startPosition, endPosition, pushMovementTime, true));
            PlaySFX(pushSFX);

            Invoke(nameof(ResetPiece), 5f);
        }

        private void ResetPiece()
        {
            StartCoroutine(MovePiece(endPosition, startPosition, resetMovementTime, false));
            PlaySFX(resetSFX);
        }

        private IEnumerator MovePiece(Vector3 _startPos, Vector3 _endPos, float _lerpTime, bool _isPushed)
        {
            isMoving = true;
            float startTime = Time.time;

            while (Time.time < startTime + _lerpTime)
            {
                pieceToMove.localPosition = Vector3.Lerp(_startPos, _endPos, (Time.time - startTime) / _lerpTime);
                yield return null;
            }
            pieceToMove.localPosition = _endPos;
            isMoving = false;
            isPushed = _isPushed;
            StopSFX();
        }

        public void PlaySFX(AudioClip _clip)
        {
            audioSource.Stop();
            audioSource.clip = _clip;
            audioSource.Play();
        }

        public void StopSFX()
        {
            audioSource.Stop();
        }
    }
}
