using UnityEngine;
using Zenject;
using DG.Tweening;
using TMPro;

public class RailLoopController : MonoBehaviour
{
    [SerializeField] private Transform[] _rails;

    [Header("Speed Settings")]
    [SerializeField] private float _targetSpeed = 15f;

    [Header("UI Speedometer (Optional)")]
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private RectTransform _needleTransform;
    [SerializeField] private float _maxSpeedForGauge = 100f;
    [SerializeField] private float _zeroSpeedAngle = 0f;
    [SerializeField] private float _maxSpeedAngle = -90f;

    [Header("Auto-Calculated Settings (Set > 0 to Override)")]
    [SerializeField] private float _railLength = 0f;
    [SerializeField] private float _resetZThreshold = 0f;

    public float CurrentSpeed => _currentSpeed;
    public float TraveledDistance => _traveledDistance;

    private float _currentSpeed = 0f;
    private float _traveledDistance = 0f;
    private Tween _speedTween;
    private JourneyManager _journeyManager;

    [Inject]
    public void Construct(JourneyManager journeyManager)
    {
        _journeyManager = journeyManager;
    }

    private void Awake()
    {
        CalculateRailDimensions();
    }

    private void CalculateRailDimensions()
    {
        if (_rails == null || _rails.Length == 0) return;

        if (_railLength <= 0f)
        {
            Renderer targetRenderer = null;

            Transform planeChild = _rails[0].Find("Plane");
            Transform railsChild = _rails[0].Find("Rails");

            if (planeChild != null)
            {
                targetRenderer = planeChild.GetComponent<Renderer>();
            }
            else if (railsChild != null)
            {
                targetRenderer = railsChild.GetComponent<Renderer>();
            }
            else
            {
                targetRenderer = _rails[0].GetComponentInChildren<Renderer>();
            }

            if (targetRenderer != null)
            {
                _railLength = targetRenderer.bounds.size.z;
            }
            else
            {
                _railLength = 30f;
            }
        }

        if (_resetZThreshold >= 0f)
        {
            _resetZThreshold = -_railLength;
        }
    }

    private void OnEnable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached += StopLoop;
            _journeyManager.OnStationDeparted += StartLoop;
        }
    }

    private void OnDisable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached -= StopLoop;
            _journeyManager.OnStationDeparted -= StartLoop;
        }
    }

    private void Start()
    {
        if (_journeyManager != null && _journeyManager.IsTraveling)
        {
            StartMoving();
        }
        else
        {
            _currentSpeed = 0f;
        }
    }

    private void Update()
    {
        UpdateUI();

        if (_currentSpeed <= 0.001f || _rails == null || _rails.Length == 0) return;

        float moveDistance = _currentSpeed * Time.deltaTime;

        _traveledDistance += moveDistance / 1000f;

        foreach (var rail in _rails)
        {
            rail.Translate(Vector3.back * moveDistance, Space.World);

            if (rail.position.z <= _resetZThreshold)
            {
                RecycleRail(rail);
            }
        }
    }

    private void UpdateUI()
    {
        if (_speedText != null)
        {
            _speedText.text = $"{Mathf.RoundToInt(_currentSpeed)} km/h";
        }

        if (_needleTransform != null)
        {
            float speedNormalized = Mathf.Clamp01(_currentSpeed / _maxSpeedForGauge);
            float currentAngle = Mathf.Lerp(_zeroSpeedAngle, _maxSpeedAngle, speedNormalized);

            _needleTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
    }

    private void RecycleRail(Transform rail)
    {
        float totalLoopLength = _railLength * _rails.Length;

        Vector3 newPos = rail.position;
        newPos.z += totalLoopLength;
        rail.position = newPos;

        if (rail.TryGetComponent<RailDecorationRecycler>(out var recycler))
        {
            recycler.RandomizeDecorations();
        }

        if (rail.TryGetComponent<GrassChunkSpawner>(out var grassSpawner))
        {
            grassSpawner.RestoreVegetation();
        }
    }

    private void StartLoop() => StartMoving(2f);
    private void StopLoop() => StopMoving(1.5f);

    public void StartMoving(float duration = 2f)
    {
        _speedTween?.Kill();
        _speedTween = DOTween.To(() => _currentSpeed, x => _currentSpeed = x, _targetSpeed, duration)
                             .SetEase(Ease.InQuad);
    }

    public void StopMoving(float duration = 1.5f)
    {
        _speedTween?.Kill();
        _speedTween = DOTween.To(() => _currentSpeed, x => _currentSpeed = x, 0f, duration)
                             .SetEase(Ease.OutQuad);
    }
}