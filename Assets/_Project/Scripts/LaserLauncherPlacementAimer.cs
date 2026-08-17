using UnityEngine;

namespace LaserPuzzle
{
    /// <summary>
    /// 一人称カメラの画面中央からRayを飛ばし、照準中の発射可能エリアが持つ離散配置地点を取得します。
    /// このコンポーネントは候補地点の検出とプレビューだけを担当し、入力処理と発射装置の生成は行いません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class LaserLauncherPlacementAimer : MonoBehaviour
    {
        [Header("Aim")]
        [Tooltip("配置可能エリアを探せる最大距離です。")]
        [SerializeField, Min(0.01f)] private float maxAimDistance = 10f;

        [Tooltip("Raycastの対象とするレイヤーです。発射可能エリアのレイヤーを含めてください。")]
        [SerializeField] private LayerMask placementAreaMask = Physics.DefaultRaycastLayers;

        [Header("Preview")]
        [Tooltip("選ばれたエリアの配置地点へ表示する目印です。未設定でも照準判定は動作します。")]
        [SerializeField] private GameObject placementPreview;

        private Camera aimCamera;

        public bool HasPlacementPoint { get; private set; }
        public Vector3 PlacementPosition { get; private set; }

        private void Reset()
        {
            aimCamera = GetComponent<Camera>();
        }

        private void Awake()
        {
            if (aimCamera == null)
            {
                aimCamera = GetComponent<Camera>();
            }

            SetPreviewVisible(false);
        }

        private void OnValidate()
        {
            maxAimDistance = Mathf.Max(0.01f, maxAimDistance);
        }

        private void Update()
        {
            HasPlacementPoint = TryFindPlacementPoint(out Vector3 nextPosition);

            if (!HasPlacementPoint)
            {
                SetPreviewVisible(false);
                return;
            }

            PlacementPosition = nextPosition;

            if (placementPreview != null)
            {
                placementPreview.transform.position = PlacementPosition;
            }

            SetPreviewVisible(true);
        }

        private bool TryFindPlacementPoint(out Vector3 placementPosition)
        {
            placementPosition = default;

            if (aimCamera == null)
            {
                return false;
            }

            // Viewport中央の(0.5, 0.5)からRayを作り、画面中央の照準位置を判定します。
            Ray aimRay = aimCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f));

            // 配置候補は最大照準距離内かつplacementAreaMaskに含まれる通常Colliderに限定し、Triggerは対象外とします。
            if (!Physics.Raycast(
                    aimRay,
                    out RaycastHit hit,
                    maxAimDistance,
                    placementAreaMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            // LayerMask内の別オブジェクトを配置面と誤認しないよう、命中Colliderと同じGameObjectに専用コンポーネントがあることを要求します。
            if (!hit.collider.TryGetComponent(
                    out LaserLauncherPlacementArea placementArea))
            {
                return false;
            }

            // 現在は1エリアにつき1地点の仕様なので、命中したエリアの中央配置地点を返します。
            return placementArea.TryGetPlacementPoint(out placementPosition);
        }

        private void SetPreviewVisible(bool isVisible)
        {
            if (placementPreview != null && placementPreview.activeSelf != isVisible)
            {
                placementPreview.SetActive(isVisible);
            }
        }
    }
}
