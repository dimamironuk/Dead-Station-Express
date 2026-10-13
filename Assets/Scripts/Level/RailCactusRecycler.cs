using System.Collections.Generic;
using UnityEngine;

public class RailDecorationRecycler : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] _prefabs;

    [Header("Amount Settings")]
    [SerializeField] private int _minCount = 3;
    [SerializeField] private int _maxCount = 7;

    [Header("Spawn Area Boundaries (Local)")]
    [SerializeField] private float _leftMinX = -8f;
    [SerializeField] private float _leftMaxX = -2f;

    [SerializeField] private float _rightMinX = 2f;
    [SerializeField] private float _rightMaxX = 8f;

    [SerializeField] private float _minZ = -10f;
    [SerializeField] private float _maxZ = 10f;

    private readonly List<GameObject> _spawnedDecorations = new List<GameObject>();

    private void Start()
    {
        RandomizeDecorations();
    }

    public void RandomizeDecorations()
    {
        Clear();

        if (_prefabs == null || _prefabs.Length == 0) return;

        int count = Random.Range(_minCount, _maxCount + 1);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = _prefabs[Random.Range(0, _prefabs.Length)];
            if (prefab == null) continue;

            bool isLeft = Random.value > 0.5f;
            float x = isLeft ? Random.Range(_leftMinX, _leftMaxX) : Random.Range(_rightMinX, _rightMaxX);
            float z = Random.Range(_minZ, _maxZ);

            GameObject obj = Instantiate(prefab, transform);
            obj.transform.localPosition = new Vector3(x, 0f, z);
            obj.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            float scale = Random.Range(0.8f, 1.3f);
            obj.transform.localScale = Vector3.one * scale;

            _spawnedDecorations.Add(obj);
        }
    }

    private void Clear()
    {
        foreach (var decor in _spawnedDecorations)
        {
            if (decor != null)
            {
                Destroy(decor);
            }
        }
        _spawnedDecorations.Clear();
    }

    public void ClearArea(Vector3 center, Vector3 size)
    {
        foreach (var decor in _spawnedDecorations)
        {
            if (decor != null && IsPointInBounds(decor.transform.position, center, size))
            {
                decor.SetActive(false);
            }
        }
    }

    private bool IsPointInBounds(Vector3 point, Vector3 center, Vector3 size)
    {
        return Mathf.Abs(point.x - center.x) <= size.x / 2f &&
               Mathf.Abs(point.z - center.z) <= size.z / 2f;
    }
}