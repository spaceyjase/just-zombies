using Assets.Project.Scripts.Data;
using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
        .ForEach((ref Translation position, ref Rotation rotation, in ZombieData data) =>
        {
          var target = playerPosition - position.Value;
          position.Value += data.Speed * target * deltaTime;
          rotation.Value = quaternion.identity;
        }).Schedule();
    }
  }
}
