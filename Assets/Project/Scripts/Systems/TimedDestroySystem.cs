using Assets.Project.Scripts.Data;
using Assets.Project.Scripts.Managers;
using Assets.Project.Scripts.Player;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Project.Scripts.Systems
{
  [UsedImplicitly]
  public class TimedDestroySystem : SystemBase
  {
    protected override void OnUpdate()
    {
      var dt = Time.DeltaTime;

      Entities
        .WithStructuralChanges()
        .ForEach((Entity entity, int entityInQueryIndex, ref LifetimeData lifetimeData) =>
        {
          lifetimeData.Value -= dt;
          if (lifetimeData.Value > 0) return;

          if (EntityManager.HasComponent<PlayerData>(entity))
          {
            GameManager.GameOver();
          }
          EntityManager.DestroyEntity(entity);
        }).Run();
    }
  }
}
