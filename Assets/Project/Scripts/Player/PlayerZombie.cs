using Assets.Project.Scripts.Managers;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Project.Scripts.Player
{
  [UsedImplicitly]
  public class PlayerZombie : MonoBehaviour
  {
    [UsedImplicitly]
    private void Update()
    {
      transform.position = GameManager.PlayerPosition + Vector3.back;
    }
  }
}
