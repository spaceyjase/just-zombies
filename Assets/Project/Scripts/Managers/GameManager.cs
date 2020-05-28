using System.Collections;
using System.Linq;
using Assets.Project.Scripts.Player;
using Assets.Project.Scripts.Systems;
using Assets.Project.Scripts.Utility;
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
    [Header("Level geometry")]
    [SerializeField]
    private GameObject horizontalPrefab = null;
    [SerializeField]
    private GameObject verticalPrefab = null;

    [Header("Game settings")]
    [SerializeField]
    private int gameLengthInSeconds = 60;
    [SerializeField]
    private int maxLevels = 10;

    [Header("Player settings")]
    [SerializeField]
    private GameObject playerPrefab = null;
    [SerializeField]
    private EntityTracker playerTracker = null;
    [SerializeField]
    private float playerSpeed = 10f;
    [SerializeField]
    private float fireRate = 0.1f;

    [Header("Bullet settings")]
    [SerializeField]
    private GameObject bulletPrefab = null;
    [SerializeField]
    private float bulletSpeed = 100f;
    [SerializeField]
    private float bulletLifetimeInSeconds = 2f;

    [Header("Zombie settings")]
    [SerializeField]
    private int maxZombies = 1000;
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
    public static float FireRate => instance == null ? 0.1f : instance.fireRate;
    public static float BulletLifetimeInSeconds => instance == null ? 0f : instance.bulletLifetimeInSeconds;
    public static float BulletSpeed => instance == null ? 0f : instance.bulletSpeed;
    public static Vector3 PlayerPosition => instance == null ? Vector3.zero : instance.playerTracker.gameObject.transform.position;
    #endregion

    public static void GameOver()
    {
      if (instance == null) return;

      World.DefaultGameObjectInjectionWorld.GetExistingSystem<GameStateSystem>().Enabled = false;
      instance.gameOver = true;
      // TODO: do game over UI, sounds, etc
      Debug.Log("GAME OVER");
    }

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
      store = new BlobAssetStore();
      manager = World.DefaultGameObjectInjectionWorld.EntityManager;
      settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, store);
      Initialise();
    }

    [UsedImplicitly]
    private void OnDestroy()
    {
      store.Dispose();
    }

    private void Initialise()
    {
      SpawnLevelGeometry();
      SpawnPlayer();
      StartCoroutine(nameof(SpawnZombies));
    }

    private void SpawnLevelGeometry()
    {
      var height = UnityEngine.Camera.main.orthographicSize;
      var width = UnityEngine.Camera.main.orthographicSize * UnityEngine.Camera.main.aspect;

      // Position walls just outside of camera boundary
      var offset = verticalPrefab.transform.localScale.x / 2f;
      var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(verticalPrefab, settings);
      var leftWall = manager.Instantiate(prefab);
      manager.SetComponentData(leftWall, new Translation { Value = new float3(-width - offset, 0f, 0f) });
      var rightWall = manager.Instantiate(prefab);
      manager.SetComponentData(rightWall, new Translation { Value = new float3(width + offset, 0f, 0f) });

      offset = horizontalPrefab.transform.localScale.y / 2f;
      prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(horizontalPrefab, settings);
      var topWall = manager.Instantiate(prefab);
      manager.SetComponentData(topWall, new Translation { Value = new float3(0f, height + offset, 0f) });
      var bottomWall = manager.Instantiate(prefab);
      manager.SetComponentData(bottomWall, new Translation { Value = new float3(0f, -height - offset, 0f) });
    }

    private void SpawnPlayer()
    {
      var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(playerPrefab, settings);
      var player = manager.Instantiate(prefab);

      playerTracker.SetReceivedEntity(player);

      prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(bulletPrefab, settings);

      manager.SetComponentData(player, new Translation { Value = new float3(0f, 0f, 0f) });
      manager.SetComponentData(player, new PlayerData
      {
        Speed = playerSpeed,
        Bullet = prefab
      });
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
        float x = Random.Range(width, width + 300);
        float y = Random.Range(height, height + 300);
        x *= Random.value > 0.5f ? 1 : -1;
        y *= Random.value > 0.5f ? 1 : -1;
        var position = transform.TransformPoint(new float3(x, y, 0f));
        manager.SetComponentData(zombie, new Translation { Value = position });

        // Data
        manager.SetComponentData(zombie, new ZombieData
        {
          Speed = Random.Range(minZombieSpeed, maxZombieSpeed)
        });

        yield return new WaitForSeconds(currentRate);
      }
    }
  }
}
