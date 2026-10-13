using UnityEngine;
using Zenject;
using DG.Tweening;
using TMPro;

public class HealthGaugeUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform _needleTransform;
    [SerializeField] private TextMeshProUGUI _healthText;

    [Header("Angle Settings")]
    [SerializeField] private float _minHealthAngle = 90f;
    [SerializeField] private float _maxHealthAngle = 0f;

    [Header("Animation Settings")]
    [SerializeField] private float _moveDuration = 0.45f;
    [SerializeField] private float _vibrationStrength = 1.5f;

    private TrainEngine _trainEngine;
    private Tween _moveTween;
    private Tween _vibrationTween;
    private float _targetAngle;

    [Inject]
    public void Construct(TrainEngine trainEngine)
    {
        _trainEngine = trainEngine;
    }

    private void OnEnable()
    {
        if (_trainEngine != null)
        {
            _trainEngine.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(_trainEngine.CurrentHealth, _trainEngine.MaxHealth);
        }
    }

    private void OnDisable()
    {
        if (_trainEngine != null)
        {
            _trainEngine.OnHealthChanged -= HandleHealthChanged;
        }

        KillTweens();
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
        _targetAngle = Mathf.Lerp(_minHealthAngle, _maxHealthAngle, healthPercent);

        if (_healthText != null)
        {
            _healthText.text = $"{Mathf.RoundToInt(currentHealth)} HP";
        }

        AnimateNeedle();
    }

    private void AnimateNeedle()
    {
        if (_needleTransform == null) return;

        KillTweens();

        _moveTween = _needleTransform
            .DORotate(new Vector3(0f, 0f, _targetAngle), _moveDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(StartEngineVibration);
    }

    private void StartEngineVibration()
    {
        if (_needleTransform == null) return;

        _vibrationTween = _needleTransform
            .DOPunchRotation(new Vector3(0f, 0f, _vibrationStrength), 0.8f, 6, 0.5f)
            .SetLoops(-1, LoopType.Restart);
    }

    private void KillTweens()
    {
        if (_moveTween != null && _moveTween.IsActive()) _moveTween.Kill();
        if (_vibrationTween != null && _vibrationTween.IsActive()) _vibrationTween.Kill();
    }
}