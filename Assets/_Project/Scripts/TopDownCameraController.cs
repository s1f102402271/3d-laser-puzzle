using UnityEngine;
using UnityEngine.InputSystem;

namespace LaserPuzzle
{
    /// <summary>
    /// ステージを平面的に把握するための真上固定・平行投影の俯瞰カメラです。
    /// X/Z平面上の指定範囲だけを平行移動し、回転、高さ変更、ズーム操作は行いません。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class TopDownCameraController : MonoBehaviour
    {
        [Header("Pan")]
        [Tooltip("俯瞰カメラを平行移動する速度（m/s）です。操作感を調整する仮パラメータです。")]
        [SerializeField, Min(0f)] private float panSpeed = 8f;

        [Tooltip("俯瞰カメラ中心が移動できるX/Z範囲の最小値です。")]
        [SerializeField] private Vector2 minimumPosition = new(-8f, -8f);

        [Tooltip("俯瞰カメラ中心が移動できるX/Z範囲の最大値です。")]
        [SerializeField] private Vector2 maximumPosition = new(8f, 8f);

        [Header("View")]
        [Tooltip("俯瞰カメラの固定Orthographic Sizeです。ズーム実装前の仮パラメータです。")]
        [SerializeField, Min(0.01f)] private float fixedOrthographicSize = 10f;

        private Camera topDownCamera;
        private float fixedHeight;

        private void Awake()
        {
            topDownCamera = GetComponent<Camera>();
            fixedHeight = transform.position.y;

            // 俯瞰視点では遠近感を使わず配置と経路を比較できるよう、平行投影を強制します。
            topDownCamera.orthographic = true;
            KeepTopDownPose();
        }

        private void Update()
        {
            Pan();
        }

        private void LateUpdate()
        {
            // 外部処理やInspector操作でTransformが変わっても、毎フレーム真上固定の仕様へ戻します。
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
