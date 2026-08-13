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
        private LaserGoal currentGoal;

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

        private void OnDisable()
        {
            SetActiveGoal(null);
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
            LaserGoal hitGoal = null;

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

                // Raycastで当たったColliderがゴールかどうかを調べます。
                // ゴールならhitGoalにLaserGoalが入り、壁などならnullのままです。
                TryFindGoal(hit.collider, out hitGoal);
            }

            // StraightLaserは編集モードでも動くため、CLEAR判定は再生中だけ更新します。
            if (Application.isPlaying)
            {
                SetActiveGoal(hitGoal);
            }

            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, EndPosition);
        }

        private bool TryFindGoal(Collider hitCollider, out LaserGoal hitGoal)
        {
            // まず「ゴールが見つかっていない」状態にします。
            // out引数なので、呼び出し元のhitGoalにもこの結果が渡ります。
            hitGoal = null;

            // Raycastで当たったColliderと同じGameObjectからLaserGoalを探します。
            // 見つかった場合：戻り値はtrue、hitGoalには見つけたLaserGoalが入ります。
            // 見つからない場合：戻り値はfalse、hitGoalはnullのままです。
            // ※ColliderとLaserGoalが別の親子オブジェクトにある場合は見つかりません。
            return hitCollider.TryGetComponent<LaserGoal>(out hitGoal);
        }

        private void SetActiveGoal(LaserGoal nextGoal)
        {
            if (currentGoal == nextGoal)
            {
                // 同じゴールに当たり続けている場合も、表示状態を命中中に保ちます。
                // Awakeの実行順やInspector操作でClearTextが非表示になっても、
                // 次のフレームで正しい状態へ戻せます。
                if (currentGoal != null)
                {
                    currentGoal.SetLaserHit(true);
                }

                return;
            }

            if (currentGoal != null)
            {
                currentGoal.SetLaserHit(false);
            }

            currentGoal = nextGoal;

            if (currentGoal != null)
            {
                currentGoal.SetLaserHit(true);
            }
        }
    }
}
