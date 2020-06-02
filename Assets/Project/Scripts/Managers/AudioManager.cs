using System;
using Assets.Project.Scripts.Audio;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Project.Scripts.Managers
{
  public class AudioManager : MonoBehaviour
  {
    [SerializeField]
    private AudioClip[] zombieSpawnSounds;
    [SerializeField]
    private Sound[] sounds;

    private static AudioManager instance;

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
      foreach (var sound in sounds)
      {
        sound.Source = gameObject.AddComponent<AudioSource>();
        sound.Source.clip = sound.Clip;
        sound.Source.volume = sound.Volume;
        sound.Source.pitch = sound.Pitch;
        sound.Source.loop = sound.Loop;
        if (sound.LoadOnStartup)
        {
          sound.Source.clip.LoadAudioData();
        }
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
      sound?.Play();
    }
    public static void PlaySfx(string name, Vector3 position)
    {
      if (instance == null) { return; }

      var sound = Array.Find(instance.sounds, s => s.name == name);
      sound?.Play(position);
    }
  }
}

