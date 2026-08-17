using UnityEngine;
using UnityEngine.InputSystem;

namespace LaserPuzzle
{
    /// <summary>
    /// 一人称視点でのWASD歩行とマウス視点操作を担当します。
    /// 現在の実装範囲は水平歩行のみで、ジャンプ、しゃがみ、重力、ダッシュは含みません。
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
        [Tooltip("一人称視点での歩行速度（m/s）です。ダッシュは含みません。")]
        [SerializeField, Min(0f)] private float moveSpeed = 4f;

        [Header("Look")]
        [Tooltip("マウス入力1単位あたりの視点回転量です。操作感を調整する仮パラメータです。")]
        [SerializeField, Min(0f)] private float lookSensitivity = 0.1f;

        [Tooltip("一人称視点で上下を向ける最大角度です。")]
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

            // 入力ベクトルを最大1に制限し、斜め移動でも単軸移動と同じ最高速度にします。
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);


            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        private void Look(Vector2 input)
        {
            // 水平入力はプレイヤー本体のY軸回転へ変換し、移動方向と視線方向を一致させます。
            float yaw = input.x * lookSensitivity;

            // 左右回転はプレイヤー全体へ適用します。
            transform.Rotate(Vector3.up * yaw);

            // 垂直入力は上下視点用の角度として蓄積します。
            pitch -= input.y * lookSensitivity;

            // 上下角度を制限し、視点が真上・真下を越えて反転しないようにします。
            pitch = Mathf.Clamp(
                pitch,
                -maxLookAngle,
                maxLookAngle
            );

            // 上下回転はカメラを持つViewPivotだけへ適用し、プレイヤー本体を傾けません。
            viewPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
