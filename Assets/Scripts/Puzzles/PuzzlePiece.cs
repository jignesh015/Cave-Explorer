using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

namespace CaveExplorer
{
    public class PuzzlePiece : MonoBehaviour
    {
        [Header("PIECE ROTATION REFERENCES")]
        [SerializeField] private float rotateTime = 0.25f;
        [SerializeField] private Transform pieceToRotate;
        [SerializeField] private List<Quaternion> rotationAngles;
        [SerializeField] private Quaternion currentRotation;

        [Header("SYMBOL REFERENCES")]
        [SerializeField] private List<PuzzleSymbol> puzzleSymbols;
        private MeshRenderer puzzlePieceRenderer;

        [Header("SFX")]
        [SerializeField] private AudioClip rotateSFX;

        private AudioSource audioSource;

        private bool isRotating;

        [Header("DEBUG ONLY")]
        [SerializeField] private int currentRotationAngleIndex;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            pieceToRotate = transform.GetChild(0);
            puzzlePieceRenderer = pieceToRotate.GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            currentRotation = pieceToRotate.localRotation;
        }

        private IEnumerator MovePiece(Vector3 _startPos, Vector3 _endPos, float _lerpTime, bool _isPushed)
        {
            isRotating = true;
            float startTime = Time.time;

            while (Time.time < startTime + _lerpTime)
            {
                pieceToRotate.localPosition = Vector3.Lerp(_startPos, _endPos, (Time.time - startTime) / _lerpTime);
                yield return null;
            }
            pieceToRotate.localPosition = _endPos;
            isRotating = false;
            StopSFX();
        }

        /// <summary>
        /// Rotates the puzzle piece by 90 degrees
        /// </summary>
        public void RotatePiece()
        {
            StartCoroutine(RotatePieceAsync());
        }

        private IEnumerator RotatePieceAsync()
        {
            StopSFX();

            //Increment rotation angle index
            currentRotationAngleIndex++;
            if (currentRotationAngleIndex > rotationAngles.Count) currentRotationAngleIndex = 0;

            //Start rotation
            isRotating = true;
            float startTime = Time.time;
            Quaternion _startRot = pieceToRotate.localRotation;
            Quaternion _endRot = rotationAngles[currentRotationAngleIndex];

            while (Time.time < startTime + rotateTime)
            {
                pieceToRotate.localRotation = Quaternion.Lerp(_startRot, _endRot, (Time.time - startTime) / rotateTime);
                yield return null;
            }
            pieceToRotate.localRotation = _endRot;
            isRotating = false;

            //Play Rotation SFX
            PlaySFX(rotateSFX);
        }

        /// <summary>
        /// Sets the given list of materials on to the puzzle piece
        /// </summary>
        /// <param name="_puzzleSymbols"></param>
        public void SetSymbolMaterials(List<PuzzleSymbol> _puzzleSymbols)
        {
            puzzleSymbols = _puzzleSymbols;
            Material[] _matArray = puzzlePieceRenderer.materials;
            for(int i = 0; i < puzzleSymbols.Count; i++)
            {
                //puzzlePieceRenderer.materials[i + 1] = puzzleSymbols[i].symbolMaterial;
                //Debug.LogFormat("<color=orange>SetSymbolMaterials {0} | {1}</color>", i + 1,
                //    puzzleSymbols[i].symbolMaterial.name);
                _matArray[i + 1] = puzzleSymbols[i].symbolMaterial;
            }
            puzzlePieceRenderer.materials = _matArray;
        }

        /// <summary>
        /// Sets the intial rotation of the puzzle piece |
        /// -1 = Random
        /// </summary>
        /// <param name="_rotationIndex"></param>
        public void SetInitialRotation(int _rotationIndex = -1)
        {
            currentRotationAngleIndex = _rotationIndex == -1 ? Random.Range(0,4) : _rotationIndex;
            pieceToRotate.localRotation = rotationAngles[currentRotationAngleIndex];
        }    

        /// <summary>
        /// Returns the index of the currently active symbol
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSymbolIndex()
        {
            return puzzleSymbols[currentRotationAngleIndex].symbolIndex;
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
