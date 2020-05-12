using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Project.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject player = null;

        private static GameManager instance;

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

        public static Vector2 PlayerPosition
        {
            get
            {
                if (instance == null) return Vector2.zero;

                return instance.player.transform.position;
            }
        }
    }
}
