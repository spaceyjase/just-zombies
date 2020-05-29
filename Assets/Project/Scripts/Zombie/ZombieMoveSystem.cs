using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.Project.Scripts.Zombie
{
  [UsedImplicitly]
  public class ZombieMoveSystem : SystemBase
  {
    protected override void OnUpdate()
    {
      var deltaTime = Time.DeltaTime;

      float3 playerPosition;
      playerPosition.x = GameManager.PlayerPosition.x;
      playerPosition.y = GameManager.PlayerPosition.y;
      playerPosition.z = 0f;

      Entities
        .WithAll<Zombie>()
        .ForEach((ref PhysicsVelocity velocity, ref Rotation rotation, in Translation translation, in ZombieData data) =>
        {
          var target = playerPosition - translation.Value;
          velocity.Linear = data.Speed * target * deltaTime;
          rotation.Value = quaternion.identity;
        }).Schedule();
    }
  }
}
