using JetBrains.Annotations;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor.IMGUI.Controls;

namespace Assets.Project.Scripts.Bullet
{
    [UsedImplicitly]
    public class BulletMoveSystem : JobComponentSystem  // TODO: generic move along given direction system
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var deltaTime = Time.DeltaTime;

            var jobHandle = Entities
                .WithName(nameof(BulletMoveSystem))
                .ForEach((ref PhysicsVelocity velocity, ref Rotation rotation, ref BulletData data) =>
                {
                    velocity.Linear += data.Speed * deltaTime * math.mul(rotation.Value, new float3(1f, 0f, 0f));
                }).Schedule(inputDeps);

            jobHandle.Complete();

            return inputDeps;
        }
    }
}
