using JetBrains.Annotations;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Project.Scripts.Systems
{
    [UsedImplicitly]
    public class HealthDestroySystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Entities.WithoutBurst().WithStructuralChanges()
                .ForEach((Entity entity, ref HealthData healthData) =>
                {
                    if (healthData.health <= 0f)
                    {
                        EntityManager.DestroyEntity(entity);
                    }
                }).Run();

            return inputDeps;
        }
    }
}
