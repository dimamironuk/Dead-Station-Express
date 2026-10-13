using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DG.Tweening;

public class TurretPlacementController : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _wagonLayer;
    [SerializeField] private GameObject _panelPlaceTurrets;

    private CurrencyManager _currencyManager;
    private TurretData _selectedTurretData;
    private float _lastDeactivationTime = -1f;

    public bool IsPlacing => _selectedTurretData != null;
    public bool IsPlacingOrJustCancelled => IsPlacing || (Time.time - _lastDeactivationTime < 0.3f);

    private readonly List<GameObject> _activePreviews = new List<GameObject>();

    [Inject]
    public void Construct(CurrencyManager currencyManager)
    {
        _currencyManager = currencyManager;
    }

    private void Awake()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_panelPlaceTurrets != null) _panelPlaceTurrets.SetActive(false);
    }

    public void OpenPanel()
    {
        if (_panelPlaceTurrets == null) return;
        _panelPlaceTurrets.SetActive(true);
        _panelPlaceTurrets.transform.DOKill();
        _panelPlaceTurrets.transform.localScale = Vector3.zero;
        _panelPlaceTurrets.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void ClosePanel()
    {
        if (_panelPlaceTurrets == null || !_panelPlaceTurrets.activeSelf) return;
        _panelPlaceTurrets.transform.DOKill();
        _panelPlaceTurrets.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).SetUpdate(true)
            .OnComplete(() => _panelPlaceTurrets.SetActive(false));
    }

    public void BeginPlacement(GameObject turretPrefab, int price, GameObject customPreviewPrefab = null)
    {
        TurretData data = new TurretData
        {
            Prefab = turretPrefab,
            PrefabGreen = customPreviewPrefab,
            Price = price
        };
        BeginPlacement(data);
    }

    public void BeginPlacement(TurretData turretData)
    {
        CancelPlacement();
        _selectedTurretData = turretData;
        ClosePanel();
        ShowPreviewsOnFreeWagons();
    }

    private void Update()
    {
        if (_selectedTurretData == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceOnWagon();
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }

    private void TryPlaceOnWagon()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _wagonLayer))
        {
            var wagon = hit.collider.GetComponentInParent<Wagon>();

            if (wagon != null && !wagon.HasTurret)
            {
                if (_currencyManager != null && _currencyManager.TrySpendNuts(_selectedTurretData.Price))
                {
                    wagon.TryPlaceTurret(_selectedTurretData);
                }
            }
        }

        CancelPlacement();
    }

    private void ShowPreviewsOnFreeWagons()
    {
        if (_selectedTurretData == null) return;
        GameObject prefabToSpawn = _selectedTurretData.PrefabGreen != null ? _selectedTurretData.PrefabGreen : _selectedTurretData.Prefab;
        if (prefabToSpawn == null) return;

        Wagon[] allWagons = FindObjectsOfType<Wagon>();

        foreach (var wagon in allWagons)
        {
            if (!wagon.HasTurret)
            {
                Transform slot = wagon.TurretSlot;
                GameObject preview = Instantiate(prefabToSpawn, slot);
                preview.transform.localPosition = Vector3.zero;
                preview.transform.localRotation = Quaternion.identity;

                DisablePreviewComponents(preview);
                _activePreviews.Add(preview);
            }
        }
    }

    private void DisablePreviewComponents(GameObject preview)
    {
        foreach (var col in preview.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        foreach (var script in preview.GetComponentsInChildren<MonoBehaviour>())
        {
            script.enabled = false;
        }
    }

    public void CancelPlacement()
    {
        ClearPreviews();
        if (_selectedTurretData != null)
        {
            _lastDeactivationTime = Time.time;
        }
        _selectedTurretData = null;
    }

    private void ClearPreviews()
    {
        foreach (var preview in _activePreviews)
        {
            if (preview != null)
            {
                Destroy(preview);
            }
        }
        _activePreviews.Clear();
    }
}