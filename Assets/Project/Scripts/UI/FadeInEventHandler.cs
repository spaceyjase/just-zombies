using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Project.Scripts.UI
{
  [UsedImplicitly]
  public class FadeInEventHandler : MonoBehaviour
  {
    public void OnFadeInEvent()
    {
      MainMenuManager.LoadGame();
    }
  }
}
