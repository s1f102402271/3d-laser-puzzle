using UnityEngine;
using UnityEngine.InputSystem;

namespace LaserPuzzle
{
    /// <summary>
    /// 真上固定の俯瞰カメラを一定の表示範囲で平行移動します。
    /// 回転操作は持たず、向きと高さは常に固定します。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class TopDownCameraController : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField, Min(0f)] private float panSpeed = 8f;
        [SerializeField] private Vector2 minimumPosition = new(-8f, -8f);
        [SerializeField] private Vector2 maximumPosition = new(8f, 8f);

        [Header("View")]
        [SerializeField, Min(0.01f)] private float fixedOrthographicSize = 10f;

        private Camera topDownCamera;
        private float fixedHeight;

        private void Awake()
        {
            topDownCamera = GetComponent<Camera>();
            fixedHeight = transform.position.y;

            // 遠近感で高さを読み取る視点ではなく、平面を確認する視点にします。
            topDownCamera.orthographic = true;
            KeepTopDownPose();
        }

        private void Update()
        {
            Pan();
        }

        private void LateUpdate()
        {
            // 別の処理からTransformが変更されても、斜め視点にはしません。
            KeepTopDownPose();
        }

        private void Pan()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            Vector2 input = Vector2.zero;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                input.y += 1f;
            }

            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 position = transform.position;
            position.x = Mathf.Clamp(
                position.x + input.x * panSpeed * Time.deltaTime,
                minimumPosition.x,
                maximumPosition.x);
            position.z = Mathf.Clamp(
                position.z + input.y * panSpeed * Time.deltaTime,
                minimumPosition.y,
                maximumPosition.y);
            position.y = fixedHeight;
            transform.position = position;
        }

        private void KeepTopDownPose()
        {
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, minimumPosition.x, maximumPosition.x);
            position.y = fixedHeight;
            position.z = Mathf.Clamp(position.z, minimumPosition.y, maximumPosition.y);
            transform.SetPositionAndRotation(position, Quaternion.Euler(90f, 0f, 0f));
            topDownCamera.orthographicSize = fixedOrthographicSize;
        }

        private void OnValidate()
        {
            maximumPosition.x = Mathf.Max(maximumPosition.x, minimumPosition.x);
            maximumPosition.y = Mathf.Max(maximumPosition.y, minimumPosition.y);
        }
    }
}
