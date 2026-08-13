using UnityEngine;
using UnityEngine.Rendering;

namespace LaserPuzzle
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class StraightLaser : MonoBehaviour
    {
        [Header("Laser Ray")]
        [Tooltip("レーザーの発射地点です。このTransformの青いZ軸方向へ進みます。未設定の場合は、このGameObjectのTransformを使用します。")]
        [SerializeField] private Transform emissionPoint;

        [Tooltip("レーザーが何にも当たらなかった場合に到達する仮の最大距離です。")]
        // 0.01f未満の値は無効とし、0.01fに補正。
        [SerializeField, Min(0.01f)] private float maxDistance = 20f;

        [Tooltip("レーザーを停止させるColliderのレイヤーを指定します。")]
        [SerializeField] private LayerMask collisionMask = Physics.DefaultRaycastLayers;

        [Tooltip("Trigger Colliderをレーザーの命中対象に含めるかを指定します。")]
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Appearance")]
        [Tooltip("レーザーの描画に使用するマテリアルです。プロジェクトの赤いレーザー用マテリアルを設定します。")]
        [SerializeField] private Material laserMaterial;

        [SerializeField, Min(0.001f)] private float width = 0.05f;
        [SerializeField] private Color color = Color.red;

        private LineRenderer lineRenderer;

        public Collider HitCollider { get; private set; }
        public Vector3 EndPosition { get; private set; }

        private void Reset()
        {
            CacheLineRenderer();
            ConfigureLineRenderer();
            UpdateLaser();
        }

        private void OnEnable()
        {
            CacheLineRenderer();
            ConfigureLineRenderer();
            UpdateLaser();
        }

        private void OnValidate()
        {
            maxDistance = Mathf.Max(0.01f, maxDistance);
            width = Mathf.Max(0.001f, width);

            CacheLineRenderer();
            ConfigureLineRenderer();
            UpdateLaser();
        }

        private void Update()
        {
            UpdateLaser();
        }

        private void CacheLineRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
        }

        private void ConfigureLineRenderer()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            if (laserMaterial != null && lineRenderer.sharedMaterial != laserMaterial)
            {
                lineRenderer.sharedMaterial = laserMaterial;
            }
        }

        private void UpdateLaser()
        {
            CacheLineRenderer();

            if (lineRenderer == null)
            {
                return;
            }

            if (emissionPoint == null)
            {
                lineRenderer.enabled = false;
                return;
            }

            lineRenderer.enabled = true;

            Transform source = emissionPoint;
            Vector3 origin = source.position;
            Vector3 direction = source.forward;

            HitCollider = null;
            EndPosition = origin + direction * maxDistance;

            if (Physics.Raycast(
                    origin,
                    direction,
                    out RaycastHit hit,
                    maxDistance,
                    collisionMask,
                    triggerInteraction))
            {
                EndPosition = hit.point;
                HitCollider = hit.collider;
            }

            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, EndPosition);
        }
    }
}
