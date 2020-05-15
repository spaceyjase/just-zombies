using Assets.Project.Scripts.Data;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Project.Scripts.Player
{
    public class PlayerControllerSystem : JobComponentSystem
    {
        struct Worker
        {
            public float InputX;
            public float InputY;
            public bool ResetX;
            public bool ResetY;
            public float DeltaTime;

            public float X;
            public float Y;

            public void DoWork(ref Translation translation,
                ref PhysicsVelocity physics,
                ref Rotation rotation,
                in PlayerData data)
            {
                physics.Linear += DeltaTime * data.Speed * new float3(InputX, InputY, 0f);

                //if (ResetX) physics.Linear.x = 0f;
                //if (ResetY) physics.Linear.y = 0f;

                rotation.Value = quaternion.identity;

                X = translation.Value.x;
                Y = translation.Value.y;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var inputY = Input.GetAxis("Vertical");
            var inputX = Input.GetAxis("Horizontal");

            var worker = new Worker()
            {
                InputX = inputX,
                InputY = inputY,
                ResetX = Mathf.Approximately(inputX, 0f),
                ResetY = Mathf.Approximately(inputY, 0f),
                DeltaTime = Time.DeltaTime
            };

            Entities
                .WithName(nameof(PlayerControllerSystem))
                .ForEach((ref Translation translation, ref PhysicsVelocity physics, ref Rotation rotation, in PlayerData data) 
                    => worker.DoWork(ref translation, ref physics, ref rotation, in data))
                .Run();

            GameData.PlayerPosition.x = worker.X;
            GameData.PlayerPosition.y = worker.Y;

            return inputDeps;
        }
    }
}

