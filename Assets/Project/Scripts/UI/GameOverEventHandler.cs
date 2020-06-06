using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Project.Scripts.UI
{
  [UsedImplicitly]
  public class GameOverEventHandler : MonoBehaviour
  {
    [UsedImplicitly]
    public void OnGameOverAnimationShown()
    {
      GameManager.GameOverUiShown();
    }

    [UsedImplicitly]
    public void OnGameOverAnimationCompleted()
    {
      GameManager.GameOverCompleted();
    }
  }
}
