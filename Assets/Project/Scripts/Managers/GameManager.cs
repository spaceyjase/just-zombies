using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Project.Scripts.Data;
using Assets.Project.Scripts.Player;
using Assets.Project.Scripts.Zombie;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
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
        private int maxLevels = 10;
        [SerializeField]
        private GameObject playerPrefab = null;
        [SerializeField]
        private float playerSpeed = 10f;
        [SerializeField]
        private GameObject[] zombiePrefabs = null;
        [SerializeField]
        private float minZombieSpeed = 1f;
        [SerializeField]
        private float maxZombieSpeed = 10f;

        private static GameManager instance;
        private bool gameOver = false;
        private EntityManager manager;
        private GameObjectConversionSettings settings;

        private BlobAssetStore store;

        #region Properties
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
            store = new BlobAssetStore();
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, store);
            Initialise();
        }

        private void OnDestroy()
        {
            store.Dispose();
        }

        private void Initialise()
        {
            SpawnPlayer();
            StartCoroutine(nameof(SpawnZombies));
        }

        private void SpawnPlayer()
        {
            var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(playerPrefab, settings);
            var player = manager.Instantiate(prefab);

            manager.SetComponentData(player, new Translation { Value = new float3(0f, 0f, 0f) });
            manager.SetComponentData(player, new PlayerData { Speed = playerSpeed });
        }

        private IEnumerator SpawnZombies()
        {
            var rate = (float)gameLengthInSeconds / maxZombies;
            if (!(rate > 0)) yield return null;

            var levelTime = (float)gameLengthInSeconds / maxLevels;
            var currentRate = rate * levelTime;   // starting rate is easy peasy

            var entities = zombiePrefabs.Select(zombiePrefab => GameObjectConversionUtility.ConvertGameObjectHierarchy(zombiePrefab, settings)).ToList();
            var currentTime = 10f;
            while (!gameOver)
            {
                if (Time.time > currentTime)
                {
                    currentRate -= rate;
                    if (currentRate < 0f) currentRate = 0f;
                    currentTime += levelTime;
                    // TODO: notify spawn rate change, increased level (messenger)
                }
                // Create
                var zombie = manager.Instantiate(entities[Random.Range(0, entities.Count)]);

                // TODO: Set spawn location (defined in editor?)
                float height = UnityEngine.Camera.main.orthographicSize + 1;
                float width = UnityEngine.Camera.main.orthographicSize * UnityEngine.Camera.main.aspect;
                float x = UnityEngine.Random.Range(width, width + 300);
                float y = UnityEngine.Random.Range(height, height + 300);
                x *= Random.value > 0.5f ? 1 : -1;
                y *= Random.value > 0.5f ? 1 : -1;
                var position = transform.TransformPoint(new float3(x, y, 0f));
                manager.SetComponentData(zombie, new Translation { Value = position });

                // Data
                manager.SetComponentData(zombie, new ZombieData
                {
                    Speed = Random.Range(minZombieSpeed, maxZombieSpeed),
                    Health = 1
                });

                yield return new WaitForSeconds(currentRate);
            }
        }
    }
}
