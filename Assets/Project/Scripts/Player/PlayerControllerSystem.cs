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
using UnityEngine.XR.WSA.Input;
using Math = System.Math;

namespace Assets.Project.Scripts.Player
{
    [UsedImplicitly]
    public class PlayerControllerSystem : JobComponentSystem
    {
        private float nextFireTime;

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
            inputX = 0f;
            inputY = Input.GetAxisRaw("Vertical_2");
            if (Mathf.Approximately(inputY, 0f))
            {
                inputY = 0f;
                inputX = Input.GetAxisRaw("Horizontal_2");
                if (Mathf.Approximately(inputX, 0f)) return inputDeps;
            }

            // Got this far so user is pressing a fire button

            var targetAngle = 0f;
            if (inputY > 0f) targetAngle = 90f * Mathf.Deg2Rad;          // up
            else if (inputY < 0f) targetAngle = 270f * Mathf.Deg2Rad;    // down
            else if (inputX > 0f) targetAngle = 0f * Mathf.Deg2Rad;      // left
            else if (inputX < 0f) targetAngle = 180f * Mathf.Deg2Rad;    // rigth

            nextFireTime += Time.DeltaTime;
            if (!(nextFireTime > GameManager.FireRate)) return inputDeps;

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
                        Value = quaternion.Euler(0f, 0f, targetAngle)
                    });

                    EntityManager.SetComponentData(instance, new LifetimeData { Lifetime = GameManager.BulletLifetimeInSeconds });
                    EntityManager.SetComponentData(instance, new BulletData { Speed = GameManager.BulletSpeed, });
                }).Run();

            nextFireTime = 0f;

            return inputDeps;
        }
    }
}

