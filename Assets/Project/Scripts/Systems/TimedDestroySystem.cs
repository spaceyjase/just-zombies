using Assets.Project.Scripts.Data;
using JetBrains.Annotations;
using Unity.Entities;

namespace Assets.Project.Scripts.Systems
{
  [UsedImplicitly]
  public class TimedDestroySystem : SystemBase
  {
    protected override void OnUpdate()
    {
      var dt = Time.DeltaTime;

      var ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
      var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

      Entities
        .ForEach((Entity entity, int entityInQueryIndex, ref LifetimeData lifetimeData) =>
        {
          lifetimeData.Value -= dt;
          if (lifetimeData.Value > 0) return;

          ecb.DestroyEntity(entityInQueryIndex, entity);
        }).Schedule();
      ecbSystem.AddJobHandleForProducer(Dependency);
    }
  }
}
