using Unity.Entities;

namespace Assets.Project.Scripts.Data
{
    [GenerateAuthoringComponent]
    public struct LifetimeData : IComponentData
    {
        public float Lifetime;
    }
}
