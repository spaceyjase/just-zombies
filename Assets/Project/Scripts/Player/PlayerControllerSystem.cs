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

            GameData.PlayerPosition.x = worker.X;
            GameData.PlayerPosition.y = worker.Y;

            var fire = Input.GetButton("Fire1") && Time.ElapsedTime > nextFireTime;        // TODO: right-stick axis
            if (!fire) return inputDeps;

            var direction = new float3(inputX, inputY, 0f);     // TODO: axis

            Entities.WithoutBurst().WithStructuralChanges()
                .ForEach((ref Translation position, ref Rotation rotation, ref PlayerData data) =>
                {
                    var instance = EntityManager.Instantiate(data.Bullet);
                    EntityManager.SetComponentData(instance, new Translation
                    {
                        Value = new float3(worker.X, worker.Y, 0f)
                    });
                    //EntityManager.SetComponentData(instance, new Rotation
                    //{
                    //    //Value = math.mul(math.normalize(rotation.Value),
                    //    //    quaternion.AxisAngle(math.up(), deltaTime * direction.z))
                    //    Value = quaternion.LookRotation(math.normalize(direction), new float3(0f, 0f, 1f))
                    //});

                    EntityManager.SetComponentData(instance, new LifetimeData { Lifetime = GameManager.BulletLifetimeInSeconds });
                    EntityManager.SetComponentData(instance, new BulletData
                    {
                        Speed = GameManager.BulletSpeed,
                    });
                }).Run();

            nextFireTime = Time.ElapsedTime + GameManager.FireRate;

            return inputDeps;
        }
    }
}

