using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Project.Scripts.Managers
{
  public class MainMenuManager : MonoBehaviour
  {
    [SerializeField] private Animator animator = null;

    private bool started = false;
    private static MainMenuManager instance;

    [UsedImplicitly]
    private void Awake()
    {
      if (instance != null && instance != this)
      {
        Destroy(gameObject);
      }
      else
      {
        instance = this;
      }
    }

    [UsedImplicitly]
    private void Start()
    {
      Invoke(nameof(PlayZombieSfx), Random.Range(5, 20));
    }

    private void PlayZombieSfx()
    {
      AudioManager.PlayZombieSpawnSfx(Vector3.zero);
      Invoke(nameof(PlayZombieSfx), Random.Range(5, 20));
    }

    public static void StartGame()
    {
      if (instance == null) return;

      instance.started = true;
      instance.animator.SetTrigger("fadeIn");
      AudioManager.FadeMusic(0.5f);
    }

    public static void LoadGame()
    {
      SceneManager.LoadScene(1);
    }
  }
}
