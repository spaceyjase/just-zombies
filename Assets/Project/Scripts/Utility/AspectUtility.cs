using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Project.Scripts.Utility
{
  [UsedImplicitly]
  public class AspectUtility : MonoBehaviour
  {
    [SerializeField]
    private new UnityEngine.Camera camera;
    [SerializeField]
    private float wantedAspectRatio = 1.333333f;
    [SerializeField]
    private bool forceLandscapeMode = true;

    private UnityEngine.Camera backgroundCam;

    [UsedImplicitly]
    private void Awake()
    {
      if (!camera)
      {
        camera = UnityEngine.Camera.main;
        Debug.Log("Setting the main camera " + camera.name);
      }
      else
      {
        Debug.Log("Setting the main camera " + camera.name);
      }

      if (!camera)
      {
        Debug.LogError("No camera available");
        return;
      }

      SetCamera();
    }

    private void SetCamera()
    {
      float currentAspectRatio;
      if (Screen.orientation == ScreenOrientation.LandscapeRight ||
          Screen.orientation == ScreenOrientation.LandscapeLeft)
      {
        Debug.Log("Landscape detected...");
        currentAspectRatio = (float)Screen.width / Screen.height;
      }
      else
      {
        Debug.Log("Portrait detected...?");
        if (Screen.height > Screen.width && forceLandscapeMode)
        {
          currentAspectRatio = (float)Screen.height / Screen.width;
        }
        else
        {
          currentAspectRatio = (float)Screen.width / Screen.height;
        }
      }
      // If the current aspect ratio is already approximately equal to the desired aspect ratio,
      // use a full-screen Rect (in case it was set to something else previously)

      Debug.Log("currentAspectRatio = " + currentAspectRatio + ", wantedAspectRatio = " + wantedAspectRatio);

      if (Mathf.Approximately((currentAspectRatio * 100) / 100.0f, wantedAspectRatio * 100 / 100.0f))
      {
        camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        if (backgroundCam)
        {
          Destroy(backgroundCam.gameObject);
        }
        return;
      }

      // Pillarbox
      if (currentAspectRatio > wantedAspectRatio)
      {
        var inset = 1.0f - wantedAspectRatio / currentAspectRatio;
        camera.rect = new Rect(inset / 2, 0.0f, 1.0f - inset, 1.0f);
      }
      // Letterbox
      else
      {
        var inset = 1.0f - currentAspectRatio / wantedAspectRatio;
        camera.rect = new Rect(0.0f, inset / 2, 1.0f, 1.0f - inset);
      }

      if (backgroundCam) return;

      // Make a new camera behind the normal camera which displays black; otherwise the unused space is undefined
      backgroundCam = new GameObject("BackgroundCam", typeof(UnityEngine.Camera)).GetComponent<UnityEngine.Camera>();
      backgroundCam.depth = int.MinValue;
      backgroundCam.clearFlags = CameraClearFlags.SolidColor;
      backgroundCam.backgroundColor = Color.black;
      backgroundCam.cullingMask = 0;
    }
  }
}
