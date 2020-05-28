using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Project.Scripts.Data;
using Assets.Project.Scripts.Managers;
using Assets.Project.Scripts.Player;
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
