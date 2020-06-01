using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
  [SerializeField]
  private AudioClip[] zombieSpawnSounds;

  private static AudioManager instance;

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

  public static void PlayZombieSpawnSfx(Vector3 position)
  {
    if (instance == null) { return; }

    AudioSource.PlayClipAtPoint(instance.zombieSpawnSounds[Random.Range(0, instance.zombieSpawnSounds.Length)], position);
  }
}

