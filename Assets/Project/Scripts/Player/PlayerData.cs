using Unity.Entities;
using UnityEngine;

namespace Assets.Project.Scripts.Player
{
    [GenerateAuthoringComponent]
    public struct PlayerData : IComponentData
    {
        public float Speed;
    }
}
