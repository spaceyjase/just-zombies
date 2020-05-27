using Unity.Entities;

namespace Assets.Project.Scripts.Data
{
  [GenerateAuthoringComponent]
  public struct DamageData : IComponentData
  {
    public float Value;
  }
}
