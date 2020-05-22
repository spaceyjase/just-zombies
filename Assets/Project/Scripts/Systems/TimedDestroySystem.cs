using Assets.Project.Scripts.Data;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Project.Scripts.Systems
{
    [UsedImplicitly]
    public class TimedDestroySystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var dt = Time.DeltaTime;
            Entities.WithoutBurst().WithStructuralChanges()
                .ForEach((Entity entity, ref LifetimeData lifetimeData) =>
                {
                    lifetimeData.Lifetime -= dt;
                    if (lifetimeData.Lifetime <= 0f)
                    {
                        EntityManager.DestroyEntity(entity);
                    }
                }).Run();

            return inputDeps;
        }
    }
}
