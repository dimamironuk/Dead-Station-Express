using UnityEngine;
using System.Collections.Generic;

public class GrassChunkSpawner : MonoBehaviour
{
    [Header("Prefabs Arrays")]
    [SerializeField] private GameObject[] _grassPrefabs;
    [SerializeField] private GameObject[] _bushPrefabs;

    [Header("Area Settings")]
    [SerializeField] private Vector2 _chunkSize = new Vector2(75f, 70f);
    [SerializeField] private float _trackClearanceWidth = 5f;
    [SerializeField] private int _spawnAttempts = 400;

    [Header("Raycast Optimization")]
    [SerializeField] private LayerMask _groundLayer;

    [Header("Natural Layering (Perlin Noise)")]
    [SerializeField] private float _perlinScale = 0.05f;
    [Range(0f, 1f)][SerializeField] private float _grassThreshold = 0.2f;
    [Range(0f, 1f)][SerializeField] private float _bushThreshold = 0.6f;

    [Header("Randomization")]
    [SerializeField] private Vector2 _grassScaleRange = new Vector2(0.8f, 1.4f);
    [SerializeField] private Vector2 _bushScaleRange = new Vector2(1f, 1.8f);

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Generate Vegetation")]
    public void Generate()
    {
        ClearVegetation();

        if ((_grassPrefabs == null || _grassPrefabs.Length == 0) &&
            (_bushPrefabs == null || _bushPrefabs.Length == 0))
        {
            return;
        }

        for (int i = 0; i < _spawnAttempts; i++)
        {
            float localX = Random.Range(-_chunkSize.x / 2f, _chunkSize.x / 2f);
            float localZ = Random.Range(-_chunkSize.y / 2f, _chunkSize.y / 2f);

            if (Mathf.Abs(localX) < _trackClearanceWidth / 2f) continue;

            Vector3 worldPos = transform.position + new Vector3(localX, 50f, localZ);
            float noise = Mathf.PerlinNoise(worldPos.x * _perlinScale, worldPos.z * _perlinScale);

            if (noise < _grassThreshold) continue;

            if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, 100f, _groundLayer))
            {
                Vector3 spawnPoint = hit.point;
                GameObject prefabToSpawn = null;
                Vector2 scaleRange = Vector2.one;

                if (noise >= _bushThreshold && _bushPrefabs != null && _bushPrefabs.Length > 0)
                {
                    prefabToSpawn = GetRandomPrefab(_bushPrefabs);
                    scaleRange = _bushScaleRange;
                }
                else if (_grassPrefabs != null && _grassPrefabs.Length > 0)
                {
                    prefabToSpawn = GetRandomPrefab(_grassPrefabs);
                    scaleRange = _grassScaleRange;
                }

                if (prefabToSpawn == null) continue;

                GameObject spawnedObj = Instantiate(prefabToSpawn, spawnPoint, Quaternion.identity, transform);
                float randomYRot = Random.Range(0f, 360f);
                float randomScale = Random.Range(scaleRange.x, scaleRange.y);

                spawnedObj.transform.rotation = Quaternion.Euler(0f, randomYRot, 0f);
                spawnedObj.transform.localScale = Vector3.one * randomScale;
            }
        }
    }

    private GameObject GetRandomPrefab(GameObject[] array)
    {
        if (array == null || array.Length == 0) return null;
        return array[Random.Range(0, array.Length)];
    }

    [ContextMenu("Clear Vegetation")]
    public void ClearVegetation()
    {
        List<GameObject> childrenToDestroy = new List<GameObject>();

        foreach (Transform child in transform)
        {
            bool isGrass = IsPrefabFromList(child.gameObject, _grassPrefabs);
            bool isBush = IsPrefabFromList(child.gameObject, _bushPrefabs);

            if (isGrass || isBush)
            {
                childrenToDestroy.Add(child.gameObject);
            }
        }

        foreach (GameObject obj in childrenToDestroy)
        {
            DestroyImmediate(obj);
        }
    }

    private bool IsPrefabFromList(GameObject obj, GameObject[] list)
    {
        if (list == null) return false;
        foreach (var prefab in list)
        {
            if (prefab != null && obj.name.StartsWith(prefab.name)) return true;
        }
        return false;
    }

    public void ClearArea(Vector3 center, Vector3 size)
    {
        foreach (Transform child in transform)
        {
            bool isGrass = IsPrefabFromList(child.gameObject, _grassPrefabs);
            bool isBush = IsPrefabFromList(child.gameObject, _bushPrefabs);

            if ((isGrass || isBush) && IsPointInBounds(child.position, center, size))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void RestoreVegetation()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private bool IsPointInBounds(Vector3 point, Vector3 center, Vector3 size)
    {
        return Mathf.Abs(point.x - center.x) <= size.x / 2f &&
               Mathf.Abs(point.z - center.z) <= size.z / 2f;
    }
}