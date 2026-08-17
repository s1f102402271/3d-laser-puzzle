using UnityEngine;
using UnityEngine.InputSystem;

namespace LaserPuzzle
{
    /// <summary>
    /// 左クリック時に照準中の離散配置地点へレーザー発射装置を1台だけ配置します。
    /// 配置済みの場合は新しい装置を生成せず同じ装置を移動します。発射状態による再配置禁止はまだ扱いません。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LaserLauncherPlacer : MonoBehaviour
    {
        [Header("Input")]
        [Tooltip("発射装置を配置する入力です。Laser/Place（左クリック）を指定してください。")]
        [SerializeField] private InputActionReference placeAction;

        [Header("Placement")]
        [Tooltip("現在選択されているPlacementAreaの配置地点を提供するコンポーネントです。")]
        [SerializeField] private LaserLauncherPlacementAimer placementAimer;

        [Tooltip("配置するレーザー発射装置のPrefabです。")]
        [SerializeField] private GameObject launcherPrefab;

        private GameObject placedLauncher;

        public bool HasPlacedLauncher => placedLauncher != null;
        public GameObject PlacedLauncher => placedLauncher;

        private void Reset()
        {
            placementAimer = GetComponent<LaserLauncherPlacementAimer>();
        }

        private void Awake()
        {
            if (placementAimer == null)
            {
                placementAimer = GetComponent<LaserLauncherPlacementAimer>();
            }

            if (placeAction == null)
            {
                Debug.LogError("配置入力(placeAction)が設定されていません。", this);
            }

            if (placementAimer == null)
            {
                Debug.LogError("配置地点の照準(placementAimer)が設定されていません。", this);
            }

            if (launcherPrefab == null)
            {
                Debug.LogError("発射装置のPrefab(launcherPrefab)が設定されていません。", this);
            }
        }

        private void OnEnable()
        {
            placeAction?.action.Enable();
        }

        private void OnDisable()
        {
            placeAction?.action.Disable();
        }

        private void Update()
        {
            if (placeAction == null ||
                !placeAction.action.WasPressedThisFrame())
            {
                return;
            }

            PlaceLauncher();
        }

        private void PlaceLauncher()
        {
            if (placementAimer == null ||
                launcherPrefab == null ||
                !placementAimer.HasPlacementPoint)
            {
                return;
            }

            Vector3 position = placementAimer.PlacementPosition;

            if (placedLauncher == null)
            {
                placedLauncher = Instantiate(
                    launcherPrefab,
                    position,
                    launcherPrefab.transform.rotation);
                return;
            }

            placedLauncher.transform.position = position;
        }
    }
}
