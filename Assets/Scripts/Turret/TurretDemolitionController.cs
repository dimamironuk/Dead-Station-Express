using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class TurretDemolitionController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Material _redDemolitionMaterial;
    [Range(0f, 1f)]
    [SerializeField] private float _refundPercentage = 0.5f;

    private CurrencyManager _currencyManager;
    private TurretPlacementController _placementController;

    private bool _isDemolitionActive = false;
    private bool _justActivated = false;
    private float _lastDeactivationTime = -1f;

    public bool IsDemolitionActive => _isDemolitionActive;
    public bool IsDemolitionActiveOrJustCancelled => _isDemolitionActive || (Time.time - _lastDeactivationTime < 0.3f);

    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

    [Inject]
    public void Construct(CurrencyManager currencyManager, TurretPlacementController placementController)
    {
        _currencyManager = currencyManager;
        _placementController = placementController;
    }

    private void Awake()
    {
        EnsureCamera();
    }

    private void EnsureCamera()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
    }

    public void ToggleDemolitionMode()
    {
        if (_isDemolitionActive)
        {
            CancelDemolition();
        }
        else
        {
            BeginDemolition();
        }
    }

    public void BeginDemolition()
    {
        EnsureCamera();

        if (_placementController != null)
        {
            _placementController.CancelPlacement();
        }

        _isDemolitionActive = true;
        _justActivated = true;

        HighlightAllTurretsRed();
    }

    private void Update()
    {
        if (!_isDemolitionActive) return;

        if (_justActivated)
        {
            _justActivated = false;
            return;
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelDemolition();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryDemolishTurret();
        }
    }

    private void HighlightAllTurretsRed()
    {
        RestoreOriginalMaterials();

        Wagon[] allWagons = FindObjectsOfType<Wagon>();

        foreach (var wagon in allWagons)
        {
            if (wagon.HasTurret && wagon.InstalledTurret != null)
            {
                Renderer[] renderers = wagon.InstalledTurret.GetComponentsInChildren<Renderer>();

                foreach (var rend in renderers)
                {
                    if (!_originalMaterials.ContainsKey(rend))
                    {
                        _originalMaterials[rend] = rend.sharedMaterials;
                    }

                    Material[] redMats = new Material[rend.sharedMaterials.Length];
                    for (int i = 0; i < redMats.Length; i++)
                    {
                        redMats[i] = _redDemolitionMaterial;
                    }
                    rend.materials = redMats;
                }
            }
        }
    }

    private void TryDemolishTurret()
    {
        EnsureCamera();
        if (_mainCamera == null) return;

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            Wagon wagon = hit.collider.GetComponentInParent<Wagon>();

            if (wagon != null && wagon.HasTurret)
            {
                RestoreOriginalMaterials();

                int refundAmount = wagon.RemoveTurret(_refundPercentage);

                if (_currencyManager != null && refundAmount > 0)
                {
                    _currencyManager.AddNuts(refundAmount);
                }

                _lastDeactivationTime = Time.time;
                _isDemolitionActive = false;
                return;
            }
        }

        CancelDemolition();
    }

    public void CancelDemolition()
    {
        RestoreOriginalMaterials();
        if (_isDemolitionActive)
        {
            _lastDeactivationTime = Time.time;
        }
        _isDemolitionActive = false;
        _justActivated = false;
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var kvp in _originalMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.materials = kvp.Value;
            }
        }
        _originalMaterials.Clear();
    }
}