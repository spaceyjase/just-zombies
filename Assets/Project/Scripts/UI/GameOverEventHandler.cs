using System.Collections;
using System.Collections.Generic;
using Assets.Project.Scripts.Managers;
using UnityEngine;

public class GameOverEventHandler : MonoBehaviour
{
  public void OnGameOverAnimationCompleted()
  {
    GameManager.GameOverCompleted();
  }
}
