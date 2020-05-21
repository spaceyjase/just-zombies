using Assets.Project.Scripts.Zombie;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Assets.Project.Scripts.Systems
{
    [UsedImplicitly]
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public class ZombieCollisionSystem : JobComponentSystem
    {
        private BuildPhysicsWorld physicsWorld;
        private StepPhysicsWorld stepPhysicsWorld;

        private struct CollisionEventJob : ICollisionEventsJob
        {
            [ReadOnly] public ComponentDataFromEntity<ZombieData> ZombieData;
            public ComponentDataFromEntity<HealthData> HealthData;

            public void Execute(CollisionEvent collisionEvent)
            {
                var entityA = collisionEvent.Entities.EntityA;
                var entityB = collisionEvent.Entities.EntityB;

                var isTargetA = HealthData.Exists(entityA);
                var isTargetB = HealthData.Exists(entityB);

                var isZombieA = ZombieData.Exists(entityA);
                var isZombieB = ZombieData.Exists(entityB);

                if (isZombieA && isZombieB) return; // Zombies don't kill each other

                if (isZombieA && isTargetB)
                {
                    DoHit(entityB, ref HealthData);
                }

                if (isZombieB && isTargetA)
                {
                    DoHit(entityA, ref HealthData);
                }
            }

            private static void DoHit(Entity entity, ref ComponentDataFromEntity<HealthData> healthData)
            {
                var component = healthData[entity];
                component.health = 0;
                healthData[entity] = component;
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
            stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var jobHandle = new CollisionEventJob
            {
                ZombieData = GetComponentDataFromEntity<ZombieData>(),
                HealthData = GetComponentDataFromEntity<HealthData>()
            }.Schedule(stepPhysicsWorld.Simulation, ref physicsWorld.PhysicsWorld, inputDeps);

            jobHandle.Complete();

            return jobHandle;
        }
    }
}
