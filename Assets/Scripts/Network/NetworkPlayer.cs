using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.XR.CoreUtils;
using UnityEngine.XR;
using UnityEngine.Video;
using TMPro;
using UnityEngine.Audio;

namespace CaveExplorer
{
    public class NetworkPlayer : MonoBehaviour
    {
        [Header("NETWORK PLAYER REFERENCES")]
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        [Header("NETWORK PLAYER CANVAS")]
        [SerializeField] private GameObject playerCanvas;
        [SerializeField] private TextMeshProUGUI playerNameText;

        [Header("ANIMATORS")]
        [SerializeField] private Animator playerCanvasAnim;
        [SerializeField] private Animator leftHandAnimator;
        [SerializeField] private Animator rightHandAnimator;

        [Header("AUDIO SOURCE")]
        [SerializeField] private AudioSource networkPlayerAudioSource;

        private PhotonView photonView;

        private Transform headOrigin;
        private Transform leftHandOrigin;
        private Transform rightHandOrigin;

        private PlayerController playerController;
        private NetworkManager networkManager;

        // Start is called before the first frame update
        void Start()
        {
            photonView = GetComponent<PhotonView>();
            XROrigin rig = FindObjectOfType<XROrigin>();
            headOrigin = rig.transform.Find("Camera Offset/Main Camera");
            leftHandOrigin = rig.transform.Find("Camera Offset/LeftHand Controller");
            rightHandOrigin = rig.transform.Find("Camera Offset/RightHand Controller");

            playerController = FindObjectOfType<PlayerController>();
            networkManager = FindObjectOfType<NetworkManager>();


            //Disable self-renderers
            if (photonView.IsMine)
            {
                foreach (var item in GetComponentsInChildren<Renderer>())
                {
                    item.enabled = false;
                }
                playerCanvas.transform.localScale= Vector3.zero;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine)
            {
                MapPosition(head, headOrigin);
                MapPosition(leftHand, leftHandOrigin);
                MapPosition(rightHand, rightHandOrigin);

                UpdateCanvasAnimation();
                UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), leftHandAnimator);
                UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), rightHandAnimator);
            }
            playerNameText.text = photonView.Controller.NickName;
        }

        void UpdateHandAnimation(InputDevice targetDevice, Animator handAnimator)
        {
            if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            {
                handAnimator.SetFloat("Trigger", triggerValue);
            }
            else
            {
                handAnimator.SetFloat("Trigger", 0);
            }

            if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
            {
                handAnimator.SetFloat("Grip", gripValue);
            }
            else
            {
                handAnimator.SetFloat("Grip", 0);
            }
        }

        public void UpdateCanvasAnimation()
        {
            playerCanvasAnim.SetFloat("Speaking", playerController.speakerAmp);
        }

        private void MapPosition(Transform target, Transform rigTransform)
        {
            target.position = rigTransform.position;
            target.rotation = rigTransform.rotation;
        }

        public void SetAudioMixer(AudioMixerGroup _audioMixer = null)
        {
            networkPlayerAudioSource.outputAudioMixerGroup = _audioMixer;
        }
    }
}

