using System.Collections;
using System.Linq;
using Assets.Project.Scripts.Player;
using Assets.Project.Scripts.Systems;
using Assets.Project.Scripts.Utility;
using Assets.Project.Scripts.Zombie;
using JetBrains.Annotations;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Project.Scripts.Managers
{
  public class GameManager : MonoBehaviour
  {
    [Header("Testing")]
    [SerializeField]
    private bool testing = false;

    [Header("Level geometry")]
    [SerializeField]
    private GameObject horizontalPrefab = null;
    [SerializeField]
    private GameObject verticalPrefab = null;
    [SerializeField]
    private Color[] levelBackgrounds = null;

    [Header("UI controls")]
    [SerializeField]
    private TextMeshProUGUI timerText;
    [SerializeField]
    private Color32 finalTimerColour;
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private GameObject gameOverUi;
    [SerializeField]
    private float backgroundTransitionStep = .1f;
    [SerializeField]
    private Animator scoreAnimator;

    [Header("Game settings")]
    [SerializeField]
    private int gameLengthInSeconds = 60;
    [SerializeField]
    private int maxLevels = 10;

    [Header("Player settings")]
    [SerializeField]
    private GameObject playerPrefab = null;
    [SerializeField]
    private GameObject playerDeathPrefab = null;
    [SerializeField]
    private EntityTracker playerTracker = null;
    [SerializeField]
    private float playerSpeed = 10f;
    [SerializeField]
    private float fireRate = 0.1f;
    [SerializeField]
    private GameObject heartBeat = null;
    [SerializeField]
    private GameObject fastHeartBeat = null;

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
    private GameObject bloodParticles = null;
    [SerializeField]
    private float minZombieSpeed = 1f;
    [SerializeField]
    private float maxZombieSpeed = 10f;
    [SerializeField]
    private float zombieSpawnSfxChange = 0.995f;

    private static GameManager instance;
    private bool gameOver;
    private EntityManager manager;
    private GameObjectConversionSettings settings;
    private float timer;
    private int level;
    private int score;
    private UnityEngine.Camera mainCamera;

    private BlobAssetStore store;

    #region Properties
    public static bool IsGameOver => instance == null || instance.gameOver;
    public static float FireRate => instance == null ? 0.1f : instance.fireRate;
    public static float BulletLifetimeInSeconds => instance == null ? 0f : instance.bulletLifetimeInSeconds;
    public static float BulletSpeed => instance == null ? 0f : instance.bulletSpeed;
    public static Vector3 PlayerPosition => instance == null ? Vector3.zero : instance.playerTracker.gameObject.transform.position;
    public static int Score
    {
      get => instance ? instance.score : 0;
      set
      {
        if (instance == null || instance.gameOver) { return; }
        instance.score = value;
        instance.scoreText.text = $"{instance.score}";
        instance.scoreAnimator.SetTrigger("scoreTrigger");
      }
    }
    #endregion

    public static void GameOver()
    {
      if (instance == null || instance.testing) { return; }

      World.DefaultGameObjectInjectionWorld.GetExistingSystem<GameStateSystem>().Enabled = false;
      instance.gameOver = true;

      // TODO: do game over UI, sounds, etc

      AudioManager.PlaySfx("Player Death");
      _ = Instantiate(instance.playerDeathPrefab, PlayerPosition, instance.playerDeathPrefab.transform.rotation);

      instance.heartBeat.SetActive(false);
      instance.fastHeartBeat.SetActive(false);

      instance.gameOverUi.SetActive(true);

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
#if UNITY_EDITOR
      Debug.Log("Testing: " + testing);
#else
      testing = false;
#endif
      timer = 0f;
      level = 0;
      mainCamera = UnityEngine.Camera.main;

      SpawnLevelGeometry();
      SpawnPlayer();
      StartCoroutine(nameof(SpawnZombies));
      StartCoroutine(DoTimer());
    }

    private IEnumerator DoTimer()
    {
      // Game running...
      while (!gameOver && timer < gameLengthInSeconds)
      {
        yield return new WaitForSeconds(0.1f);
        timer += 0.1f;
        UpdateTime();
      }
      // TODO: Win!
    }

    private void UpdateTime()
    {
      if (timer > gameLengthInSeconds)
      {
        timer = gameLengthInSeconds;
      }
      timerText.text = timer.ToString("00.0");
    }

    private void SpawnLevelGeometry()
    {
      var height = mainCamera.orthographicSize;
      var width = mainCamera.orthographicSize * UnityEngine.Camera.main.aspect;

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
      heartBeat.SetActive(true);

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
      var currentTime = levelTime;
      while (!gameOver)
      {
        if (Time.time > currentTime)
        {
          currentRate -= rate;
          if (currentRate < 0f) currentRate = 0f;
          currentTime += levelTime;
          LevelChanged();
        }
        // Create
        var zombie = manager.Instantiate(entities[Random.Range(0, entities.Count)]);

        // TODO: Set spawn location (defined in editor?)
        float height = mainCamera.orthographicSize + 1;
        float width = mainCamera.orthographicSize * UnityEngine.Camera.main.aspect;
        float x = Random.Range(width, width + 300);
        float y = Random.Range(height, height + 300);
        x *= Random.value > 0.5f ? 1 : -1;
        y *= Random.value > 0.5f ? 1 : -1;
        var position = transform.TransformPoint(new float3(x, y, 0f));
        manager.SetComponentData(zombie, new Translation { Value = position });

        // Audio
        if (Random.value > zombieSpawnSfxChange)
        {
          AudioManager.PlayZombieSpawnSfx(new Vector3((x < 0f ? -width : width) / 2f, (y < 0f ? -height : height) / 2f, 0f));
        }

        // Data
        manager.SetComponentData(zombie, new ZombieData
        {
          Speed = Random.Range(minZombieSpeed, maxZombieSpeed)
        });

        yield return new WaitForSeconds(currentRate);
      }
    }

    private void LevelChanged()
    {
      if (timer > gameLengthInSeconds / 2f && !fastHeartBeat.activeInHierarchy)
      {
        heartBeat.SetActive(false);
        fastHeartBeat.SetActive(true);
        StartCoroutine(ChangeTimer());
      }
      StartCoroutine(ChangeBackground(++level));
    }

    public static void ZombieSfx(Vector2 position)  // TODO: revisit using OnDestroy system
    {
      if (instance == null) { return; }

      Instantiate(instance.bloodParticles, position, instance.bloodParticles.transform.rotation);
    }

    private IEnumerator ChangeBackground(int newLevel)
    {
      var endTime = Time.time + 1f;
      var targetColour = levelBackgrounds[newLevel % levelBackgrounds.Length];
      var t = 0f;
      while (Time.time < endTime)
      {
        mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, targetColour, t);
        t += backgroundTransitionStep;
        yield return new WaitForSeconds(backgroundTransitionStep);
      }
      mainCamera.backgroundColor = targetColour;
    }

    private IEnumerator ChangeTimer()
    {
      var startTime = Time.time;
      var duration = gameLengthInSeconds / 2f;
      var endTime = Time.time + duration;

      while (!gameOver && Time.time < endTime)
      {
        var t = (Time.time - startTime) * .1f;
        timerText.faceColor = Color32.Lerp(timerText.faceColor, finalTimerColour, t / duration);
        timerText.fontSize = Mathf.Lerp(timerText.fontSize, 115f, t / duration);
        yield return new WaitForSeconds(.1f);
      }
    }
  }
}
