using Unity.Entities;

namespace Assets.Project.Scripts.Zombie
{
    [GenerateAuthoringComponent]
    public struct ZombieData : IComponentData
    {
        public float Speed;
        //public int RotationSpeed;
        public int Health;
    }
}
