using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using Unity.Entities;

namespace Assets.Project.Scripts.Systems
{
  [UsedImplicitly]
  [AlwaysUpdateSystem]
  public class GameStateSystem : SystemBase
  {
    protected override void OnUpdate()
    {
      if (GameManager.IsGameOver) return;

      var playerQuery = GetEntityQuery(ComponentType.ReadOnly<Player.Player>());
      if (playerQuery.CalculateEntityCount() <= 0)
      {
        GameManager.GameOver();
      }
    }
  }
}
