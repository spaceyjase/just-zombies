using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Project.Scripts.Camera
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/PixelBoy")]
    [UsedImplicitly]
    public class PixelBoy : MonoBehaviour
    {
        [SerializeField]
        private int width = 800;

        private int height;

        [UsedImplicitly]
        private void Update()
        {
            float ratio = UnityEngine.Camera.main.pixelHeight / (float)UnityEngine.Camera.main.pixelWidth;
            height = Mathf.RoundToInt(width * ratio);
        }

        [UsedImplicitly]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            source.filterMode = FilterMode.Point;
            var buffer = RenderTexture.GetTemporary(width, height, -1);
            buffer.filterMode = FilterMode.Point;
            Graphics.Blit(source, buffer);
            Graphics.Blit(buffer, destination);
            RenderTexture.ReleaseTemporary(buffer);
        }
    }
}