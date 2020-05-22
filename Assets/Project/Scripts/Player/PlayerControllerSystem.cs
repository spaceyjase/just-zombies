using Assets.Project.Scripts.Bullet;
using Assets.Project.Scripts.Data;
using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Project.Scripts.Player
{
    [UsedImplicitly]
    public class PlayerControllerSystem : JobComponentSystem
    {
        private double nextFireTime = GameManager.FireRate;

        private struct Worker
        {
            public float InputX;
            public float InputY;
            public float DeltaTime;

            public float X;
            public float Y;

            public void DoWork(ref Translation translation,
                ref PhysicsVelocity physics,
                ref Rotation rotation,
                in PlayerData data)
            {
                physics.Linear += DeltaTime * data.Speed * new float3(InputX, InputY, 0f);
                physics.Angular = float3.zero;

                rotation.Value = quaternion.identity;

                X = translation.Value.x;
                Y = translation.Value.y;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var inputY = Input.GetAxis("Vertical");
            var inputX = Input.GetAxis("Horizontal");
            var deltaTime = Time.DeltaTime;

            var worker = new Worker()
            {
                InputX = inputX,
                InputY = inputY,
                DeltaTime = deltaTime
            };

            Entities
                .WithName(nameof(PlayerControllerSystem))
                .ForEach((ref Translation translation, ref PhysicsVelocity physics, ref Rotation rotation, in PlayerData data) 
                    => worker.DoWork(ref translation, ref physics, ref rotation, in data))
                .Run();

            GameData.PlayerPosition.x = worker.X;   // TODO: revisit this
            GameData.PlayerPosition.y = worker.Y;

            // TODO: refactoring...
            bool resetY = false;
            inputY = Input.GetAxis("Vertical_2");
            if (Mathf.Approximately(inputY, 0f))
            {
                resetY = true;
                inputY = 0f;
            }

            bool resetX = false;
            inputX = Input.GetAxis("Horizontal_2");
            if (Mathf.Approximately(inputX, 0f))
            {
                resetX = true;
                inputX = 0f;
            }

            if (resetX && resetY) return (inputDeps);
            if (!(Time.ElapsedTime > nextFireTime)) return inputDeps;

            var direction = new float3(inputY, inputX, 0f);

            Entities.WithoutBurst().WithStructuralChanges()
                .ForEach((ref Translation position, ref Rotation rotation, ref PlayerData data) =>
                {
                    var instance = EntityManager.Instantiate(data.Bullet);
                    EntityManager.SetComponentData(instance, new Translation
                    {
                        Value = new float3(position.Value.x, position.Value.y, position.Value.z + 0.25f)
                    });
                    EntityManager.SetComponentData(instance, new Rotation
                    {
                        Value = quaternion.LookRotation(math.normalize(direction), new float3(0f, 0f, 1f))
                    });

                    EntityManager.SetComponentData(instance, new LifetimeData { Lifetime = GameManager.BulletLifetimeInSeconds });
                    EntityManager.SetComponentData(instance, new BulletData { Speed = GameManager.BulletSpeed, });
                }).Run();

            nextFireTime = Time.ElapsedTime + GameManager.FireRate;

            return inputDeps;
        }
    }
}

