using UnityEngine;

namespace LaserPuzzle
{
    /// <summary>
    /// 通常モードのゴール到達状態を表示します。
    /// レーザーがColliderへ到達している間だけCLEARメッセージを表示し、現在は出力値の判定やステージ遷移を行いません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class LaserGoal : MonoBehaviour
    {
        [Tooltip("レーザーがゴールに当たっている間、表示するCLEARメッセージです。")]
        [SerializeField] private GameObject clearMessage;

        private void Awake()
        {
            SetLaserHit(false);
        }

        public void SetLaserHit(bool isHit)
        {
            if (clearMessage == null)
            {
                return;
            }

            clearMessage.SetActive(isHit);
        }
    }
}
