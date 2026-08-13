using UnityEngine;

namespace LaserPuzzle
{
    /// <summary>
    /// レーザーを反射せず、その場で遮断する壁を示すマーカーです。
    /// 通常のColliderは反射するため、例外となる壁だけに追加します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class LaserBlockingWall : MonoBehaviour
    {
    }
}
