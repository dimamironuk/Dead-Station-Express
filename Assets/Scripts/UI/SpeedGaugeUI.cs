using UnityEngine;
using Zenject;
using DG.Tweening;
using TMPro;

public class SpeedGaugeUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform _needleTransform;
    [SerializeField] private TextMeshProUGUI _speedText;

    [Header("Angle Settings")]
    [SerializeField] private float _zeroSpeedAngle = 90f;
    [SerializeField] private float _maxSpeedAngle = 0f;
    [SerializeField] private float _maxSpeedValue = 60f;

    [Header("Motion Settings")]
    [SerializeField] private float _speedChangeDuration = 0.8f;
    [SerializeField] private float _maxJitterAmount = 1.5f;
    [SerializeField] private float _jitterSpeed = 14f;

    private RailLoopController _railLoopController;
    private JourneyManager _journeyManager;

    private float _currentBaseAngle;
    private float _targetBaseAngle;
    private float _lastSpeed = -1f;
    private Tween _speedTween;

    [Inject]
    public void Construct([InjectOptional] RailLoopController railLoopController, [InjectOptional] JourneyManager journeyManager)
    {
        _railLoopController = railLoopController;
        _journeyManager = journeyManager;
    }

    private void Awake()
    {
        _currentBaseAngle = _zeroSpeedAngle;
        _targetBaseAngle = _zeroSpeedAngle;
    }

    private void OnDisable()
    {
        _speedTween?.Kill();
    }

    private void Update()
    {
        UpdateCurrentSpeed();

        if (_needleTransform == null) return;

        float speedPercent = Mathf.Clamp01((_currentBaseAngle - _zeroSpeedAngle) / (_maxSpeedAngle - _zeroSpeedAngle));
        float jitter = 0f;

        if (speedPercent > 0.02f)
        {
            jitter = (Mathf.PerlinNoise(Time.time * _jitterSpeed, 0f) - 0.5f) * 2f * _maxJitterAmount * speedPercent;
        }

        _needleTransform.localRotation = Quaternion.Euler(0f, 0f, _currentBaseAngle + jitter);
    }

    private void UpdateCurrentSpeed()
    {
        float currentSpeed = 0f;

        if (_railLoopController != null)
        {
            currentSpeed = _railLoopController.CurrentSpeed;
        }
        else if (_journeyManager != null)
        {
            currentSpeed = _journeyManager.IsTraveling ? _maxSpeedValue : 0f;
        }

        if (_speedText != null)
        {
            _speedText.text = $"{Mathf.RoundToInt(currentSpeed)} km/h";
        }

        if (Mathf.Abs(currentSpeed - _lastSpeed) > 0.01f)
        {
            _lastSpeed = currentSpeed;
            float targetPercent = Mathf.Clamp01(currentSpeed / Mathf.Max(0.1f, _maxSpeedValue));
            _targetBaseAngle = Mathf.Lerp(_zeroSpeedAngle, _maxSpeedAngle, targetPercent);

            _speedTween?.Kill();
            _speedTween = DOTween.To(() => _currentBaseAngle, x => _currentBaseAngle = x, _targetBaseAngle, _speedChangeDuration)
                .SetEase(Ease.OutCubic);
        }
    }
}