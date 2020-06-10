using System;
using System.Collections;
using System.Linq;
using System.Numerics;
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
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
    private GameObject gameUi;
    [SerializeField]
    private GameObject gameOverUi;
    [SerializeField]
    private GameObject winUi;
    [SerializeField]
    private TextMeshProUGUI winDestroyedText;
    [SerializeField]
    private TextMeshProUGUI survivedText;
    [SerializeField]
    private TextMeshProUGUI destroyedText;
    [SerializeField]
    private float backgroundTransitionTimeInSeconds = 1f;
    [SerializeField]
    private Animator scoreAnimator;
    [SerializeField]
    private float deathCameraSpeed = 0.25f;
    [SerializeField]
    private float deathCameraTarget = 10f;

    [Header("Game settings")]
    [SerializeField]
    private int gameLengthInSeconds = 60;
    [SerializeField]
    private int maxLevels = 10;
    [SerializeField]
    private float musicFadeTimeInSeconds = 3f;

    [Header("Player settings")]
    [SerializeField]
    private GameObject playerPrefab = null;
    [SerializeField]
    private GameObject playerZombie = null;
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
    private float zombieSpawnSfxChance = 0.995f;

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
      if (instance == null || instance.testing || IsGameOver) { return; }

      World.DefaultGameObjectInjectionWorld.GetExistingSystem<GameStateSystem>().Enabled = false;
      instance.gameOver = true;

      AudioManager.PlaySfx("Player Death");
      _ = Instantiate(instance.playerDeathPrefab, PlayerPosition, instance.playerDeathPrefab.transform.rotation);

      DisableHeartBeat();

      instance.gameUi.SetActive(false);
      instance.gameOverUi.SetActive(true);
      instance.survivedText.text = $"Survived {instance.timer:00.0} secs";
      instance.destroyedText.text = $"{Score} destroyed";

      instance.StartCoroutine(instance.PlayerFocusCamera());

      Debug.Log("GAME OVER");
    }

    private static void DisableHeartBeat()
    {
      instance.heartBeat.SetActive(false);
      instance.fastHeartBeat.SetActive(false);
    }

    private static void Win()
    {
      if (instance == null) { return; }

      World.DefaultGameObjectInjectionWorld.GetExistingSystem<GameStateSystem>().Enabled = false;
      instance.gameOver = true;

      instance.gameUi.SetActive(false);
      instance.gameOverUi.SetActive(false);
      instance.winUi.SetActive(true);
      instance.winDestroyedText.text = $"{Score} destroyed";

      instance.StartCoroutine(instance.PlayerFocusCamera());

      Instantiate(instance.playerZombie, PlayerPosition + Vector3.back, instance.playerZombie.transform.rotation);

      DisableHeartBeat();

      Debug.Log("You Survived!");
    }

    private IEnumerator PlayerFocusCamera()
    {
      while (mainCamera.orthographicSize > deathCameraTarget)
      {
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, PlayerPosition, Time.deltaTime * deathCameraSpeed);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, 0f, Time.deltaTime * deathCameraSpeed);
        yield return new WaitForEndOfFrame();
      }
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
      StopAllCoroutines();

      if (!gameOver)
      {
        Debug.Log("Disposed");
        store.Dispose();  // Stops editor from crashing
      }
    }

    private void Initialise()
    {
#if UNITY_EDITOR
      Debug.Log("Testing: " + testing);
#else
      testing = false;
#endif

      gameOver = false;

      StopAllCoroutines();

      winUi.SetActive(false);
      gameOverUi.SetActive(false);
      gameUi.SetActive(true);

      timer = 0f;
      level = 0;
      Score = 0;

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

      if (timer >= gameLengthInSeconds)
      {
        Win();
      }
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
      World.DefaultGameObjectInjectionWorld.GetExistingSystem<GameStateSystem>().Enabled = true;
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
        if (Time.timeSinceLevelLoad > currentTime)
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
        if (Random.value > zombieSpawnSfxChance)
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
      var startTime = Time.timeSinceLevelLoad;
      var endTime = Time.timeSinceLevelLoad + backgroundTransitionTimeInSeconds;
      var targetColour = levelBackgrounds[newLevel % levelBackgrounds.Length];
      while (!gameOver && Time.timeSinceLevelLoad < endTime)
      {
        var t = (Time.timeSinceLevelLoad - startTime) * Time.deltaTime;
        mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, targetColour, t / backgroundTransitionTimeInSeconds);
        yield return new WaitForEndOfFrame();
      }
    }

    private IEnumerator ChangeTimer()
    {
      var startTime = Time.timeSinceLevelLoad;
      var duration = gameLengthInSeconds / 2f;
      var endTime = Time.timeSinceLevelLoad + duration;

      while (!gameOver && Time.timeSinceLevelLoad < endTime)
      {
        var t = (Time.timeSinceLevelLoad - startTime) * Time.deltaTime;
        timerText.faceColor = Color32.Lerp(timerText.faceColor, finalTimerColour, t / duration);
        timerText.fontSize = Mathf.Lerp(timerText.fontSize, 115f, t / duration);
        yield return new WaitForEndOfFrame();
      }
    }

    public static void GameOverCompleted()
    {
      if (instance == null && !IsGameOver) { return; }

      if (instance != null)
      {
        var array = instance.manager.GetAllEntities();
        foreach (var entity in array)
        {
          instance.manager.DestroyEntity(entity);
        }
      }

      SceneManager.LoadScene(0);
    }

    public static void GameOverUiShown()
    {
      if (instance == null) { return; }
      AudioManager.FadeMusic(instance.musicFadeTimeInSeconds);
    }
  }
}
