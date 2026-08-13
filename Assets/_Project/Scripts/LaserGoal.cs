using UnityEngine;

namespace LaserPuzzle
{
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
