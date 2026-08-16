using UnityEngine;

namespace LaserPuzzle
{
    /// <summary>
    /// カメラ中央の視線から発射可能エリアを探し、そのエリアの配置地点をプレビューします。
    /// この段階では照準だけを担当し、クリック入力や発射装置の生成は行いません。
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

            // Viewportでは、画面の左下が(0, 0)、右上が(1, 1)です。
            // したがって(0.5, 0.5)からRayを作ると、画面中央の視線になります。
            Ray aimRay = aimCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f));

            // 指定距離内で、placementAreaMaskに含まれるColliderだけを調べます。
            // Triggerは配置面として使わないため、ここでは無視します。
            if (!Physics.Raycast(
                    aimRay,
                    out RaycastHit hit,
                    maxAimDistance,
                    placementAreaMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            // Rayが何かに当たっても、それが発射可能エリアとは限りません。
            // Colliderと同じGameObjectから専用コンポーネントを探します。
            if (!hit.collider.TryGetComponent(
                    out LaserLauncherPlacementArea placementArea))
            {
                return false;
            }

            // 1エリアにつき配置地点は1つなので、当たったエリアの中央地点を取得します。
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
