using JetBrains.Annotations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.Project.Scripts.Bullet
{
  [UsedImplicitly]
  public class BulletMoveSystem : SystemBase
  {
    protected override void OnUpdate()
    {
      var deltaTime = Time.DeltaTime;

      Entities
        .WithAll<Bullet>()
        .ForEach((ref PhysicsVelocity velocity, ref Rotation rotation, in BulletData data) =>
        {
          velocity.Linear = data.Speed * deltaTime * math.mul(rotation.Value, new float3(1f, 0f, 0f));
        }).Schedule();

      var ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
      var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

      Entities
        .ForEach((Entity entity, int entityInQueryIndex, ref BulletData data) =>
        {
          data.Lifetime -= deltaTime;
          if (data.Lifetime <= 0f)
          {
            ecb.DestroyEntity(entityInQueryIndex, entity);
          }
        }).Schedule();
      ecbSystem.AddJobHandleForProducer(Dependency);
    }
  }
}
