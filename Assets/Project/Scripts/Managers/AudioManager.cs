using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Project.Scripts.Audio;
using System;

public class AudioManager : MonoBehaviour
{
  [SerializeField]
  private AudioClip[] zombieSpawnSounds;
  [SerializeField]
  private Sound[] sounds;

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

    AudioSource.PlayClipAtPoint(instance.zombieSpawnSounds[UnityEngine.Random.Range(0, instance.zombieSpawnSounds.Length)], position);
  }
  
  public static void PlaySfx(string name)
  {
    if (instance == null) { return; }

    var sound = Array.Find(instance.sounds, s => s.name == name);
    if (sound != null)
    {
      sound.Play();
    }
  }
}

