using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Project.Scripts.Bullet
{
    [GenerateAuthoringComponent]
    public struct BulletData : IComponentData
    {
        public float Speed;
        public float Lifetime;
    }
}
