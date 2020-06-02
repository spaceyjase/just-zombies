using Unity.Entities;

namespace Assets.Project.Scripts.Player
{
    [GenerateAuthoringComponent]
    public struct PlayerData : IComponentData
    {
        public float Speed;
        public Entity Bullet;
    }
}
