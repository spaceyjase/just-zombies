using Assets.Project.Scripts.Data;
using Assets.Project.Scripts.Managers;
using Assets.Project.Scripts.Player;
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
            lifetimeData.Value -= dt;
            if (!(lifetimeData.Value <= 0f)) return;

            if (EntityManager.HasComponent<PlayerData>(entity))
            {
              GameManager.GameOver();
            }
            EntityManager.DestroyEntity(entity);
          }).Run();

      return inputDeps;
    }
  }
}
