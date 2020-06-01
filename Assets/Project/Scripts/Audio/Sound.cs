using System;
using UnityEngine;

namespace Assets.Project.Scripts.Audio
{
  [Serializable]
  public class Sound
  {
    [SerializeField]
    public string name;
    [SerializeField]
    private AudioClip clip;
    [Range(0f, 1f)]
    [SerializeField]
    private float volume;
    [Range(0.1f, 3f)]
    [SerializeField]
    private float pitch = 1;
    [SerializeField]
    private bool loop;
    [SerializeField]
    private bool randomisePitch;
    [Range(0.1f, 3f)]
    [SerializeField]
    private float lowPitchRange = .95f;
    [Range(0.1f, 3f)]
    [SerializeField]
    private float highPitchRange = 1.05f;
    [SerializeField]
    private bool loadOnStartup = false;

    public AudioSource Source { get; set; }
    public AudioClip Clip { get { return clip; } }
    public float Pitch { get { return pitch; } }
    public float Volume { get { return volume; } }
    public bool Loop { get { return loop; } }
    public bool LoadOnStartup { get { return loadOnStartup; } }

    public void Play()
    {
      if (randomisePitch)
      {
        Source.pitch = UnityEngine.Random.Range(lowPitchRange, highPitchRange);
      }
      if (loop)
      {
        Source.Play();
      }
      else
      {
        Source.PlayOneShot(clip, volume);
      }
    }

    public void Stop()
    {
      Source.Stop();
    }

    public void Play(Vector3 point)
    {
      AudioSource.PlayClipAtPoint(clip, point, volume);
    }
  }
}
