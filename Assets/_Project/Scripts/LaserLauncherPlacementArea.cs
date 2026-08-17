using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LaserPuzzle
{
    /// <summary>
    /// レーザー発射装置を配置できる1区画と、その中央にある1つの離散配置地点を定義します。
    /// ステージに複数候補を用意する場合は、このコンポーネントを持つ区画を候補数だけ配置します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class LaserLauncherPlacementArea : MonoBehaviour
    {
        [Header("Area")]
        [Tooltip("発射可能エリアの横幅(X)と奥行き(Z)です。")]
        [SerializeField] private Vector2 areaSize = new Vector2(6f, 2f);

        [Tooltip("床面判定に使うBoxColliderの厚みです。")]
        [SerializeField, Min(0.01f)] private float colliderThickness = 0.1f;

        [Header("Placement Point")]
        [Tooltip("床との表示重なりを避けるため、配置地点を少し上へずらす量です。")]
        [SerializeField, Min(0f)] private float pointHeightOffset = 0.03f;

        [Header("Scene View Preview")]
        [Tooltip("Sceneビューで表示する発射可能エリアの色です。ゲーム画面の表示色ではありません。")]
        [SerializeField] private Color areaColor = new Color(0.1f, 0.55f, 1f, 0.25f);

        [Tooltip("Sceneビューで表示する配置地点の色です。ゲーム画面の表示色ではありません。")]
        [SerializeField] private Color pointColor = new Color(0.1f, 0.8f, 1f, 1f);

        [Tooltip("Sceneビューで表示する配置地点の半径です。判定範囲には影響しません。")]
        [SerializeField, Min(0.01f)] private float pointRadius = 0.18f;

        private BoxCollider areaCollider;

        private void Reset()
        {
            CacheCollider();
            ConfigureCollider();
        }

        private void Awake()
        {
            CacheCollider();
        }

        private void OnValidate()
        {
            areaSize.x = Mathf.Max(0.01f, areaSize.x);
            areaSize.y = Mathf.Max(0.01f, areaSize.y);
            colliderThickness = Mathf.Max(0.01f, colliderThickness);
            pointHeightOffset = Mathf.Max(0f, pointHeightOffset);
            pointRadius = Mathf.Max(0.01f, pointRadius);

            CacheCollider();
            ConfigureCollider();
        }

        /// <summary>
        /// 区画中央に高さ補正を加えた配置地点をワールド座標で返します。
        /// </summary>
        public Vector3 GetPlacementPointPosition()
        {
            Vector3 localPosition = new Vector3(0f, pointHeightOffset, 0f);
            return transform.TransformPoint(localPosition);
        }

        /// <summary>
        /// この区画が持つ唯一の配置地点を返します。現在は常に1地点を持つため成功します。
        /// </summary>
        public bool TryGetPlacementPoint(out Vector3 placementPosition)
        {
            placementPosition = GetPlacementPointPosition();
            return true;
        }

        private void CacheCollider()
        {
            if (areaCollider == null)
            {
                areaCollider = GetComponent<BoxCollider>();
            }
        }

        private void ConfigureCollider()
        {
            if (areaCollider == null)
            {
                return;
            }

            // 区画のTransform位置を床面として扱い、BoxCollider全体が床面より下側へ伸びるようにします。
            areaCollider.center = new Vector3(0f, -colliderThickness * 0.5f, 0f);
            areaCollider.size = new Vector3(areaSize.x, colliderThickness, areaSize.y);
            areaCollider.isTrigger = false;
        }

        private void OnDrawGizmos()
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = areaColor;
            Gizmos.DrawCube(
                new Vector3(0f, -0.01f, 0f),
                new Vector3(areaSize.x, 0.02f, areaSize.y));

            Gizmos.color = pointColor;
            Gizmos.DrawSphere(
                new Vector3(0f, pointHeightOffset, 0f),
                pointRadius);

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;

#if UNITY_EDITOR
            DrawPointLabels();
#endif
        }

#if UNITY_EDITOR
        private void DrawPointLabels()
        {
            Vector3 labelPosition = GetPlacementPointPosition() +
                                    transform.up * (pointRadius + 0.08f);
            Handles.Label(labelPosition, "Placement Point");
        }
#endif
    }
}
