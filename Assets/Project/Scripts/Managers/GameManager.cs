using System.Collections;
using System.Collections.Generic;
using Assets.Project.Scripts.Zombie;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Project.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private int maxZombies = 1000;
        [SerializeField]
        private int gameLengthInSeconds = 60;
        [SerializeField]
        private GameObject player = null;
        [SerializeField]
        private GameObject[] zombiePrefabs = null;
        [SerializeField]
        private float minZombieSpeed = 1f;
        [SerializeField]
        private float maxZombieSpeed = 10f;

        private static GameManager instance;

        private bool gameOver = false;

        #region Properties
        public static Vector2 PlayerPosition =>
            instance != null ? (Vector2) instance.player.transform.position : Vector2.zero;

        public static bool IsGameOver => instance == null || instance.gameOver;
        #endregion

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

        private void Start()
        {
            Initialise();
        }

        private void Initialise()
        {
            StartCoroutine(nameof(SpawnZombies));
        }

        private IEnumerator SpawnZombies()
        {
            float rate = (float)gameLengthInSeconds / maxZombies;
            if (!(rate > 0)) yield return null;

            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            var entities = new List<Entity>();
            foreach (var zombiePrefab in zombiePrefabs)
            {
                entities.Add(GameObjectConversionUtility.ConvertGameObjectHierarchy(zombiePrefab, settings));
            }
            while (!gameOver)
            {
                // Create
                var zombie = manager.Instantiate(entities[Random.Range(0, entities.Count)]);

                // TODO: Set spawn location (defined in editor)
                float x = UnityEngine.Random.Range(-300, 300);
                float y = UnityEngine.Random.Range(-300, 300);
                var position = transform.TransformPoint(new float3(x, y, 0f));
                manager.SetComponentData(zombie, new Translation { Value = position });

                // Data
                manager.SetComponentData(zombie, new ZombieData
                {
                    Speed = UnityEngine.Random.Range(minZombieSpeed, maxZombieSpeed),
                    Health = 1
                });

                yield return new WaitForSeconds(rate);
            }
        }

    }
}
