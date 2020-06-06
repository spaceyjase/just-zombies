using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Project.Scripts.UI
{
  [UsedImplicitly]
  public class StartButton : MonoBehaviour
  {
    private bool clicked = false;

    [UsedImplicitly]
    private void Update()
    {
      if (Input.GetButtonDown("Submit")) { OnStartClicked(); }
    }

    public void OnStartClicked()
    {
      if (clicked) return;

      clicked = true;
      AudioManager.PlaySfx("Zombie");
      MainMenuManager.StartGame();
    }
  }
}
