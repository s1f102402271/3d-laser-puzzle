using UnityEngine;

namespace LaserPuzzle
{
    /// <summary>
    /// Colliderをレーザーの遮断面として扱うためのマーカーです。
    /// StraightLaserはゴール以外の通常Colliderを反射面として扱うため、反射させず命中地点で停止させる面に追加します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class LaserBlockingWall : MonoBehaviour
    {
    }
}
