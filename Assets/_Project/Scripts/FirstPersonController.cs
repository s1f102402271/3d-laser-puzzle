using UnityEngine;
using UnityEngine.InputSystem;

namespace LaserPuzzle
{
    /// <summary>
    /// 一人称視点での歩行とマウス視点操作を担当します。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform viewPivot;

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 4f;

        [Header("Look")]
        [SerializeField, Min(0f)] private float lookSensitivity = 0.1f;
        [SerializeField, Range(0f, 90f)] private float maxLookAngle = 80f;

        private float pitch;

        private void Reset()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Awake()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
        }

        private void OnEnable()
        {
            moveAction?.action.Enable();
            lookAction?.action.Enable();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            lookAction?.action.Disable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (characterController == null || viewPivot == null ||
                moveAction == null || lookAction == null)
            {
                return;
            }

            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

            Move(moveInput);
            Look(lookInput);
        }

        private void Move(Vector2 input)
        {
            
            Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;

            // 斜め移動だけ速くならないよう、長さを最大1に制限します。
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);


            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        private void Look(Vector2 input)
        {
            // マウスの左右移動量から、左右の回転角度を求める
            float yaw = input.x * lookSensitivity;

            // Player全体を左右に回す
            transform.Rotate(Vector3.up * yaw);

            // 上下の角度を蓄積する
            pitch -= input.y * lookSensitivity;

            // 真上・真下を越えないように制限する
            pitch = Mathf.Clamp(
                pitch,
                -maxLookAngle,
                maxLookAngle
            );

            // ViewPivotだけを上下に回す
            viewPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
