using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Project.Scripts.Managers
{
  public class EffectManager : MonoBehaviour
  {
    [SerializeField]
    private new UnityEngine.Camera camera;
    [SerializeField]
    private float shakeDurationInSeconds = 0.5f;
    [SerializeField]
    private float shakeMagnitude = 0.5f;

    private static EffectManager instance;

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

    public static void Shake()
    {
      if (instance == null) { return; }
      instance.StartCoroutine(instance.ShakeCamera());
    }

    private IEnumerator ShakeCamera()
    {
      var startPosition = camera.transform.localPosition;
      var elapsed = 0f;
      while (elapsed < shakeDurationInSeconds)
      {
        var x = Random.Range(-1f, 1f) * shakeMagnitude;
        var y = Random.Range(-1f, 1f) * shakeMagnitude;
        camera.transform.localPosition = new Vector3(x, y, startPosition.z);
        elapsed += Time.deltaTime;
        yield return new WaitForEndOfFrame();
      }
      camera.transform.localPosition = startPosition;
    }
  }
}
