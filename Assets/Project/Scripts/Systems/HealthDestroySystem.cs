using Assets.Project.Scripts.Components;
using Assets.Project.Scripts.Data;
using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Project.Scripts.Systems
{
  [UsedImplicitly]
  public class HealthDestroySystem : SystemBase
  {
    protected override void OnUpdate()
    {
      // Modify health based on damage data
      Entities
        .ForEach((DynamicBuffer<CollisionBuffer> collisionBuffer, ref HealthData health) =>
        {
          for (var i = 0; i < collisionBuffer.Length; ++i)
          {
            if (!HasComponent<DamageData>(collisionBuffer[i].Entity)) continue;
            health.Value -= GetComponent<DamageData>(collisionBuffer[i].Entity).Value;
          }
        }).Schedule();

      // For entities that can be destroyed, add a lifetime component
      var ecb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
      Entities
        .WithoutBurst()
        .WithNone<LifetimeData>()
        .ForEach((Entity entity, in HealthData health) =>
        {
          if (health.Value > 0f) { return; }

          if (HasComponent<Zombie.Zombie>(entity))
          {
            GameManager.Score++;
            var translation = GetComponent<Translation>(entity);
            GameManager.ZombieSfx(new Vector2(translation.Value.x, translation.Value.y));
          }
          ecb.AddComponent(entity, new LifetimeData
          {
            Value = health.DestroyTime
          });
        }).Run();
    }
  }
}
