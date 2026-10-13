using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Zenject;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class TrainBuilder : MonoBehaviour
{
    [Header("Wagon Spawning")]
    [SerializeField] private GameObject _wagonPrefab;
    [SerializeField] private Transform _engineWagon;
    [SerializeField] private int _maxWagonCount = 6;
    [SerializeField] private float _wagonOffset = -3.2f;
    [SerializeField] private float _wagonOffsetY = 0.15f;
    [SerializeField] private float _spawnAnimationDuration = 0.4f;

    [Header("Turrets Database (For Load)")]
    [SerializeField] private List<GameObject> _turretPrefabs = new List<GameObject>();

    [Header("Detachment Animation")]
    [SerializeField] private float _detachRollDistance = -12f;
    [SerializeField] private float _detachDuration = 2.0f;

    private TrainCameraController _cameraController;
    private DiContainer _container;
    private readonly List<GameObject> _spawnedWagons = new List<GameObject>();

    public List<GameObject> SpawnedWagons => _spawnedWagons;
    public int MaxWagonCount => _maxWagonCount;
    public int CurrentWagonCount => _spawnedWagons.Count;

    [Inject]
    public void Construct(TrainCameraController cameraController, DiContainer container)
    {
        _cameraController = cameraController;
        _container = container;
    }

    public void AddWagon()
    {
        if (_wagonPrefab == null || _spawnedWagons.Count >= _maxWagonCount) return;

        Transform lastWagonTransform = GetLastWagonTransform();
        if (lastWagonTransform == null) return;

        float newX = lastWagonTransform.localPosition.x + _wagonOffset;
        Vector3 spawnLocalPosition = new Vector3(newX, _wagonOffsetY, 0f);

        Wagon newWagon = _container.InstantiatePrefabForComponent<Wagon>(_wagonPrefab, transform);
        if (newWagon == null) return;

        newWagon.transform.localPosition = spawnLocalPosition;
        newWagon.transform.localRotation = Quaternion.identity;
        newWagon.transform.localScale = Vector3.one;

        if (newWagon.TryGetComponent<Wagon>(out var wagonScript))
        {
            wagonScript.Init(this);
        }

        _spawnedWagons.Add(newWagon.gameObject);

        if (_cameraController != null)
        {
            _cameraController.SetWagonCount(_spawnedWagons.Count);
        }

        AnimateNewWagon(newWagon.transform);
    }

    public void RemoveWagonAndBehind(Wagon destroyedWagon)
    {
        if (destroyedWagon == null) return;

        int index = _spawnedWagons.IndexOf(destroyedWagon.gameObject);
        if (index == -1) return;

        for (int i = _spawnedWagons.Count - 1; i >= index; i--)
        {
            GameObject wagonObj = _spawnedWagons[i];
            _spawnedWagons.RemoveAt(i);

            if (wagonObj == null) continue;

            wagonObj.transform.SetParent(null);

            float targetZ = wagonObj.transform.position.z + _detachRollDistance;

            DG.Tweening.Sequence sequence = DOTween.Sequence();
            sequence.Append(wagonObj.transform.DOMoveZ(targetZ, _detachDuration).SetEase(Ease.OutCubic));
            sequence.Join(wagonObj.transform.DOScale(Vector3.zero, 0.5f).SetDelay(_detachDuration - 0.5f));
            sequence.OnComplete(() => Destroy(wagonObj));
        }

        if (_cameraController != null)
        {
            _cameraController.SetWagonCount(_spawnedWagons.Count);
        }
    }

    public void LoadWagons(List<WagonSaveData> savedWagons)
    {
        Debug.Log($"[TrainBuilder] Початок завантаження вагонів. Кількість у збереженні: {savedWagons?.Count ?? 0}");

        foreach (var wagonObj in _spawnedWagons)
        {
            if (wagonObj != null) Destroy(wagonObj);
        }
        _spawnedWagons.Clear();

        if (savedWagons == null || savedWagons.Count == 0) return;

        foreach (var wagonData in savedWagons)
        {
            AddWagon();

            GameObject lastWagonObj = _spawnedWagons[_spawnedWagons.Count - 1];
            if (lastWagonObj != null && lastWagonObj.TryGetComponent<Wagon>(out var wagon))
            {
                if (!string.IsNullOrEmpty(wagonData.turretPrefabName))
                {
                    Debug.Log($"[TrainBuilder] Вагон має збережену турель: '{wagonData.turretPrefabName}'. Шукаємо префаб у списку (_turretPrefabs: {_turretPrefabs.Count})...");

                    string cleanSavedName = wagonData.turretPrefabName.Replace("(Clone)", "").Trim();

                    GameObject turretPrefab = _turretPrefabs.Find(t =>
                        t != null && (t.name.Equals(cleanSavedName, StringComparison.OrdinalIgnoreCase) ||
                                      t.name.Equals(wagonData.turretPrefabName, StringComparison.OrdinalIgnoreCase)));

                    if (turretPrefab != null)
                    {
                        Sprite turretIcon = null;
                        if (turretPrefab.TryGetComponent<BaseTurret>(out var baseTurret))
                        {
                            turretIcon = baseTurret.TurretIcon; 
                        }

                        bool placed = wagon.TryPlaceTurret(turretPrefab, wagonData.turretPrice, turretIcon);
                        Debug.Log($"[TrainBuilder] Результат спавну турелі '{turretPrefab.name}': {placed}");

                        if (!placed)
                        {
                            Debug.LogWarning("[TrainBuilder] TryPlaceTurret повернув false! Перевірте, чи не була турель вже присутня на вагоні.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[TrainBuilder] ❌ НЕ ЗНАЙДЕНО префаб з назвою '{wagonData.turretPrefabName}' у списку _turretPrefabs!");
                    }
                }
                else
                {
                    Debug.Log("[TrainBuilder] Цей вагон у збереженні був без турелі.");
                }

                wagon.LoadState(
                    wagonData.wagonLevel,
                    wagonData.turretLevel,
                    wagonData.currentHealth,
                    wagonData.maxHealth
                );
            }
        }
    }

    private Transform GetLastWagonTransform()
    {
        _spawnedWagons.RemoveAll(w => w == null);

        if (_spawnedWagons.Count > 0)
        {
            return _spawnedWagons[_spawnedWagons.Count - 1].transform;
        }

        return _engineWagon;
    }

    private void AnimateNewWagon(Transform wagonTransform)
    {
        wagonTransform.localScale = Vector3.zero;
        wagonTransform.DOScale(Vector3.one, _spawnAnimationDuration).SetEase(Ease.OutBack);
    }
}