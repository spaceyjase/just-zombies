﻿using Unity.Entities;

namespace Assets.Project.Scripts.Data
{
  [GenerateAuthoringComponent]
  public struct HealthData : IComponentData
  {
    public float Value;
    public float DestroyTime;
  }
}
