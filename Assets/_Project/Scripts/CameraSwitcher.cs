using UnityEngine;
using UnityEngine.InputSystem;

namespace LaserPuzzle
{
    /// <summary>
    /// 数字キーで一人称視点と俯瞰視点を切り替えます。
    /// </summary>
    public sealed class CameraSwitcher : MonoBehaviour
    {
        [Header("Cameras")]
        [SerializeField] private GameObject firstPersonCamera;
        [SerializeField] private GameObject topDownCamera;

        [Header("Player")]
        [SerializeField] private FirstPersonController firstPersonController;

        [Header("Input")]
        [SerializeField] private InputActionReference firstPersonAction;
        [SerializeField] private InputActionReference overviewAction;

        private void OnEnable()
        {
            firstPersonAction?.action.Enable();
            overviewAction?.action.Enable();
        }

        private void Start()
        {
            // ゲーム開始時は一人称視点にします。
            SetFirstPersonMode(true);
        }

        private void Update()
        {
            if (firstPersonAction != null &&
                firstPersonAction.action.WasPressedThisFrame())
            {
                SetFirstPersonMode(true);
            }

            if (overviewAction != null &&
                overviewAction.action.WasPressedThisFrame())
            {
                SetFirstPersonMode(false);
            }
        }

        private void OnDisable()
        {
            firstPersonAction?.action.Disable();
            overviewAction?.action.Disable();
        }

        private void SetFirstPersonMode(bool isFirstPerson)
        {
            if (firstPersonCamera != null)
            {
                firstPersonCamera.SetActive(isFirstPerson);
            }

            if (topDownCamera != null)
            {
                topDownCamera.SetActive(!isFirstPerson);
            }

            if (firstPersonController != null)
            {
                firstPersonController.enabled = isFirstPerson;
            }
        }
    }
}
