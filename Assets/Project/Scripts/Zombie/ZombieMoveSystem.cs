﻿using Assets.Project.Scripts.Data;
using Assets.Project.Scripts.Managers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Project.Scripts.Zombie
{
    public class ZombieMoveSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var deltaTime = Time.DeltaTime;

            float3 playerPosition;
            playerPosition.x = GameData.PlayerPosition.x;
            playerPosition.y = GameData.PlayerPosition.y;
            playerPosition.z = 0f;

            var jobHandle = Entities
                .WithName(nameof(ZombieMoveSystem))
                .ForEach((ref Translation position, ref Rotation rotation, in ZombieData data) =>
                {
                    var target = playerPosition - position.Value;
                    var distance = math.length(target);

                    if (!(distance > 0.2f)) return; // too close

                    position.Value += data.Speed * target * deltaTime;
                    rotation.Value = quaternion.identity;
                }).Schedule(inputDeps);

            jobHandle.Complete();

            return jobHandle;
        }

    }
}
