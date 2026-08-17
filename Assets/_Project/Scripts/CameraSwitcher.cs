using UnityEngine;
using UnityEngine.InputSystem;

namespace LaserPuzzle
{
    /// <summary>
    /// 数字キー1で一人称視点、数字キー2で真上固定の俯瞰視点へ切り替えます。
    /// 俯瞰視点中は一人称用コントローラーを無効化し、プレイヤーの移動と視点操作を停止します。
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
            // ステージ開始時の標準視点は一人称とし、移動とマウス視点操作を有効にします。
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
