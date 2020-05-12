using Unity.Entities;

namespace Assets.Project.Scripts.Zombie
{
    [GenerateAuthoringComponent]
    public struct ZombieData : IComponentData
    {
        public int Speed;
        public int RotationSpeed;
        public int Health;
    }
}
