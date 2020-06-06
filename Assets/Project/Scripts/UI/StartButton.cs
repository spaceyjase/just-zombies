using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Project.Scripts.UI
{
  [UsedImplicitly]
  public class StartButton : MonoBehaviour
  {
    [UsedImplicitly]
    public void OnStartClicked()
    {
      AudioManager.PlaySfx("Zombie");
      MainMenuManager.StartGame();
    }
  }
}
