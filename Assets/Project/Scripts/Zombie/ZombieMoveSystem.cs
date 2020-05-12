﻿using Assets.Project.Scripts.Managers;
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
            playerPosition.x = GameManager.PlayerPosition.x;
            playerPosition.y = GameManager.PlayerPosition.y;
            playerPosition.z = 0f;

            var jobHandle = Entities
                .WithName(nameof(ZombieMoveSystem))
                .ForEach((ref Translation position, ref Rotation rotation, ref ZombieData data) =>
                {
                    var target = playerPosition - position.Value;
                    var distance = math.length(target);
                    if (distance > 0.5f)
                    {
                        position.Value = math.lerp(position.Value, playerPosition, math.mul(deltaTime, data.Speed));
                        //position.Value = math.@select(playerPosition,
                        //    math.lerp(position.Value, playerPosition,
                        //    math.mul(deltaTime, data.Speed), distance <= 1f);
                    }
                }).Schedule(inputDeps);

            return jobHandle;
        }

    }
}
