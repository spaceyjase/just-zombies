using System;
using Assets.Project.Scripts.Managers;
using Assets.Project.Scripts.Player;
using Assets.Project.Scripts.Zombie;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

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
                    if (healthData.health > 0f) return;

                    bool isPlayer;
                    try
                    {
                        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                        var _ = entityManager.GetComponentData<PlayerData>(entity);
                        isPlayer = true;
                    }
                    catch (ArgumentException)
                    {
                        // it's not the player!
                        isPlayer = false;
                    }

                    if (isPlayer)
                    {
                        GameManager.GameOver();
                    }

                    EntityManager.DestroyEntity(entity);
                }).Run();

            return inputDeps;
        }
    }
}
