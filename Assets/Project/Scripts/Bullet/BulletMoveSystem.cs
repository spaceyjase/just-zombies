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
    public class BulletMoveSystem : SystemBase  // TODO: generic move along given direction system
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            Entities
              .WithAll<Bullet>()
              .ForEach((ref PhysicsVelocity velocity, ref Rotation rotation, in BulletData data) =>
              {
                  velocity.Linear = data.Speed * deltaTime * math.mul(rotation.Value, new float3(1f, 0f, 0f));
              }).Schedule();
        }
    }
}
