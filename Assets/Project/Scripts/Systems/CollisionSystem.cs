using System.Diagnostics;
using Assets.Project.Scripts.Bullet;
using Assets.Project.Scripts.Components;
using Assets.Project.Scripts.Zombie;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Assets.Project.Scripts.Systems
{
  [UsedImplicitly]
  public class CollisionSystem : SystemBase
  {
    private struct CollisionEventJob : ICollisionEventsJob
    {
      public BufferFromEntity<CollisionBuffer> Collisions;

      public void Execute(CollisionEvent collisionEvent)
      {
        var entityA = collisionEvent.Entities.EntityA;
        var entityB = collisionEvent.Entities.EntityB;

        if (Collisions.Exists(entityA))
        {
          Collisions[entityA].Add(new CollisionBuffer { Entity = entityB });
        }

        if (Collisions.Exists(entityB))
        {
          Collisions[entityB].Add(new CollisionBuffer { Entity = entityA });
        }
      }
    }

    protected override void OnUpdate()
    {
      var physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
      var simulation = World.GetOrCreateSystem<StepPhysicsWorld>().Simulation;

      Entities.ForEach((DynamicBuffer<CollisionBuffer> collisions) =>
      {
        collisions.Clear();
      }).Run();

      var jobHandle = new CollisionEventJob
      {
        Collisions = GetBufferFromEntity<CollisionBuffer>()
      }.Schedule(simulation, ref physicsWorld, Dependency);

      jobHandle.Complete();
    }
  }
}
