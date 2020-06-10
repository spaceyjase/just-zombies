using System;
using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Project.Scripts.Utility
{
  [UsedImplicitly]
  public class EntityTracker : MonoBehaviour
  {
    private Entity trackedEntity = Entity.Null;

    public void SetReceivedEntity(Entity entity)
    {
      trackedEntity = entity;
    }

    [UsedImplicitly]
    private void LateUpdate()
    {
      if (trackedEntity.Equals(Entity.Null)) { return; }

      try
      {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        transform.position = entityManager.GetComponentData<Translation>(trackedEntity).Value;
        transform.rotation = entityManager.GetComponentData<Rotation>(trackedEntity).Value;
      }
      catch (Exception)
      {
        trackedEntity = Entity.Null;
      }
    }
  }
}

