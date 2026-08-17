using UnityEngine;
using UnityEngine.Rendering;

namespace LaserPuzzle
{
    /// <summary>
    /// 発射点から常時照射されるレーザーの経路計算、反射、描画、通常モードのゴール到達通知を担当します。
    /// ゴール以外の通常Colliderは反射面、LaserBlockingWallを持つColliderは遮断面として扱います。
    /// 経路全体は最大距離と最大反射回数で制限します。出力値と距離減衰はまだ扱わず、反射による追加減衰も行いません。
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class StraightLaser : MonoBehaviour
    {
        [Header("Laser Ray")]
        [Tooltip("レーザーの発射地点です。このTransformの青いZ軸方向へ進みます。未設定の場合は、このGameObjectのTransformを使用します。")]
        [SerializeField] private Transform emissionPoint;

        [Tooltip("レーザーが何にも当たらなかった場合に到達する仮の最大距離です。")]
        // 最大距離は正の値を必須とし、Inspector入力が0.01m未満なら安全な最小値へ補正します。
        [SerializeField, Min(0.01f)] private float maxDistance = 20f;

        [Tooltip("レーザーが命中判定を行うColliderのレイヤーを指定します。反射壁、遮断壁、ゴールを含めてください。")]
        [SerializeField] private LayerMask collisionMask = Physics.DefaultRaycastLayers;

        [Tooltip("Trigger Colliderをレーザーの命中対象に含めるかを指定します。")]
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Reflection (Provisional)")]
        [Tooltip("1回の照射で許可する最大反射回数です。仮実装値であり、確定仕様ではありません。")]
        [SerializeField, Min(0)] private int maxReflections = 3;

        [Tooltip("反射直後に同じ面へ再命中することを避けるための微小距離です。仮実装値です。")]
        [SerializeField, Min(0.0001f)] private float reflectionOffset = 0.001f;

        [Header("Appearance")]
        [Tooltip("レーザーの描画に使用するマテリアルです。プロジェクトの赤いレーザー用マテリアルを設定します。")]
        [SerializeField] private Material laserMaterial;

        [Tooltip("LineRendererで表示するレーザーの太さです。見た目を調整する仮パラメータです。")]
        [SerializeField, Min(0.001f)] private float width = 0.05f;

        [Tooltip("LineRendererへ適用するレーザーの色です。見た目を調整する仮パラメータです。")]
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
            maxReflections = Mathf.Max(0, maxReflections);
            reflectionOffset = Mathf.Max(0.0001f, reflectionOffset);
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
            Vector3 direction = source.forward.normalized;
            float remainingDistance = maxDistance;
            int pointCount = 1;

            HitCollider = null;
            EndPosition = origin + direction * maxDistance;
            LaserGoal hitGoal = null;

            // LineRendererには始点、各反射点、最終到達点が必要なため、許可される最大点数を先に確保します。
            lineRenderer.positionCount = maxReflections + 2;
            lineRenderer.SetPosition(0, origin);

            for (int reflectionCount = 0; reflectionCount <= maxReflections; reflectionCount++)
            {
                if (!Physics.Raycast(
                        origin,
                        direction,
                        out RaycastHit hit,
                        remainingDistance,
                        collisionMask,
                        triggerInteraction))
                {
                    EndPosition = origin + direction * remainingDistance;
                    lineRenderer.SetPosition(pointCount, EndPosition);
                    pointCount++;
                    break;
                }

                EndPosition = hit.point;
                HitCollider = hit.collider;
                lineRenderer.SetPosition(pointCount, hit.point);
                pointCount++;

                // ゴールへ到達した場合はそこで経路計算を終了し、通常モードのクリア表示対象として保持します。
                if (TryFindGoal(hit.collider, out hitGoal))
                {
                    break;
                }

                // 遮断面では反射せず停止します。通常面でも最大反射回数に達した時点を経路の終点とします。
                if (hit.collider.TryGetComponent<LaserBlockingWall>(out _) ||
                    reflectionCount == maxReflections)
                {
                    break;
                }

                remainingDistance -= hit.distance;
                if (remainingDistance <= 0f)
                {
                    break;
                }

                direction = CalculateReflectedDirection(direction, hit.normal);
                if (direction.sqrMagnitude < Mathf.Epsilon)
                {
                    // 有効な反射方向を計算できない異常値の場合は、無効なRayを継続せず衝突地点で停止します。
                    break;
                }

                direction.Normalize();
                origin = hit.point + direction * reflectionOffset;
            }

            lineRenderer.positionCount = pointCount;

            // ExecuteAlwaysによるScene編集時のプレビューではゲーム状態を変更せず、ゴール表示はPlayモード中だけ更新します。
            if (Application.isPlaying)
            {
                SetActiveGoal(hitGoal);
            }

        }

        private Vector3 CalculateReflectedDirection(Vector3 incomingDirection, Vector3 surfaceNormal)
        {
            // 面法線を基準とした鏡面反射を使用し、入射角と反射角が等しくなる方向を返します。
            // Vector3.Reflectは incoming - 2 * dot(incoming, normal) * normal の式に相当します。
            return Vector3.Reflect(incomingDirection, surfaceNormal);
        }

        private bool TryFindGoal(Collider hitCollider, out LaserGoal hitGoal)
        {
            // ゴール以外のColliderでは必ずnullを返せるよう、検索前に出力値を未到達状態へ初期化します。
            hitGoal = null;

            // ゴール判定は命中Colliderと同じGameObjectのLaserGoalだけを対象とし、親子階層は検索しません。
            return hitCollider.TryGetComponent<LaserGoal>(out hitGoal);
        }

        private void SetActiveGoal(LaserGoal nextGoal)
        {
            if (currentGoal == nextGoal)
            {
                // 同じゴールへの常時照射中は、別処理で表示が変わっても毎フレーム命中表示へ戻します。
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
