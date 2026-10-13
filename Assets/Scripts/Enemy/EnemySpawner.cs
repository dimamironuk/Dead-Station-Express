using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour
{
    private class WaveData
    {
        public int enemyCount;
        public float spawnInterval;
        public float restAfterWave;
    }

    [Header("Pool Settings")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _poolSize = 30;

    [Header("Spawn Position Settings")]
    [SerializeField] private float _spawnDistanceAhead = 40f;
    [SerializeField] private float _minSideOffset = -6f;
    [SerializeField] private float _maxSideOffset = 6f;

    [Header("Auto-Wave Generation Settings")]
    [SerializeField] private int _baseEnemyCount = 4;
    [SerializeField] private int _enemyIncreasePerWave = 2;
    [SerializeField] private float _baseSpawnInterval = 2.0f;
    [SerializeField] private float _minSpawnInterval = 0.4f;
    [SerializeField] private float _baseRestTime = 10.0f;
    [SerializeField] private float _minRestTime = 4.0f;

    [Header("Boss Settings")]
    [SerializeField] private BossSpawner _bossSpawner;
    [SerializeField] private int _bossSpawnWaveIndex = 4;

    private List<Enemy> _enemyPool = new List<Enemy>();
    private JourneyManager _journeyManager;
    private DiContainer _container;
    private Transform _trainEngineTransform;

    [SerializeField] private int _currentWaveIndex = 0;
    private bool _isSpawningActive = false;

    [Inject]
    public void Construct(JourneyManager journeyManager, DiContainer container)
    {
        _journeyManager = journeyManager;
        _container = container;
    }

    private void Awake()
    {
        InitializePool();
    }

    private void OnEnable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached += StopSpawning;
            _journeyManager.OnStationDeparted += StartSpawning;
        }
    }

    private void OnDisable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached -= StopSpawning;
            _journeyManager.OnStationDeparted -= StartSpawning;
        }
    }

    private void Start()
    {
        var engine = FindObjectOfType<TrainEngine>();
        if (engine != null)
        {
            _trainEngineTransform = engine.transform;
        }

        if (_journeyManager != null && _journeyManager.IsTraveling)
        {
            StartSpawning();
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject obj;
            if (_container != null)
            {
                obj = _container.InstantiatePrefab(_enemyPrefab, transform);
            }
            else
            {
                obj = Instantiate(_enemyPrefab, transform);
            }

            obj.SetActive(false);
            if (obj.TryGetComponent<Enemy>(out var enemy))
            {
                _enemyPool.Add(enemy);
            }
        }
    }

    private void StartSpawning()
    {
        if (!_isSpawningActive)
        {
            _isSpawningActive = true;
            StartCoroutine(WaveRoutine());
        }
    }

    private void StopSpawning()
    {
        _isSpawningActive = false;
        StopAllCoroutines();
    }

    private IEnumerator WaveRoutine()
    {
        while (_isSpawningActive)
        {
            if (_currentWaveIndex == _bossSpawnWaveIndex && _bossSpawner != null)
            {
                _bossSpawner.SpawnBoss();
            }

            WaveData currentWave = GenerateWaveData(_currentWaveIndex);

            for (int i = 0; i < currentWave.enemyCount; i++)
            {
                if (!_isSpawningActive) yield break;

                SpawnEnemyFromPool();
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }

            yield return new WaitForSeconds(currentWave.restAfterWave);

            _currentWaveIndex++;
        }
    }

    private WaveData GenerateWaveData(int index)
    {
        int enemyCount = _baseEnemyCount + (index * _enemyIncreasePerWave);
        float spawnInterval = Mathf.Max(_minSpawnInterval, _baseSpawnInterval - (index * 0.1f));
        float restTime = Mathf.Max(_minRestTime, _baseRestTime - (index * 0.3f));

        return new WaveData
        {
            enemyCount = enemyCount,
            spawnInterval = spawnInterval,
            restAfterWave = restTime
        };
    }

    private void SpawnEnemyFromPool()
    {
        Enemy enemy = GetInactiveEnemy();
        if (enemy == null) return;

        Vector3 spawnPos = CalculateSpawnPosition();
        enemy.transform.position = spawnPos;
        enemy.transform.rotation = Quaternion.Euler(0, 180, 0);
        enemy.gameObject.SetActive(true);
    }

    private Enemy GetInactiveEnemy()
    {
        foreach (var enemy in _enemyPool)
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                return enemy;
            }
        }
        return null;
    }

    private Vector3 CalculateSpawnPosition()
    {
        Vector3 basePosition = transform.position;

        if (_trainEngineTransform != null)
        {
            basePosition = _trainEngineTransform.position;
        }

        float randomX = Random.Range(_minSideOffset, _maxSideOffset);
        return basePosition + new Vector3(randomX, 0f, _spawnDistanceAhead);
    }
}