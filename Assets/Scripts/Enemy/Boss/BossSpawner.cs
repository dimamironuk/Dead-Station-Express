using UnityEngine;
using Zenject;

public class BossSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private int _spawnOnWaveIndex = 4;
    [SerializeField] private Vector3 _spawnOffsetBehind = new Vector3(0f, 0f, -30f);

    private DiContainer _container;
    private TrainEngine _trainEngine;
    private EnemySpawner _enemySpawner;
    private bool _hasSpawned = false;

    [Inject]
    public void Construct(DiContainer container, TrainEngine trainEngine, [InjectOptional] EnemySpawner enemySpawner)
    {
        _container = container;
        _trainEngine = trainEngine;
        _enemySpawner = enemySpawner;
    }

    public void SpawnBoss()
    {
        if (_hasSpawned || _bossPrefab == null) return;
        _hasSpawned = true;

        Vector3 spawnPosition = transform.position;
        if (_trainEngine != null)
        {
            spawnPosition = _trainEngine.transform.position + _spawnOffsetBehind;
        }
        GameObject bossObj = _container.InstantiatePrefab(_bossPrefab, spawnPosition, Quaternion.identity, null);
        bossObj.layer = LayerMask.NameToLayer("Enemy");
    }
}