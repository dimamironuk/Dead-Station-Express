using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class WagonHPBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Position Offset")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 2.5f, 0f);

    [Header("DOTween Settings")]
    [SerializeField] private float _fadeDuration = 0.25f;
    [SerializeField] private float _hpFillDuration = 0.35f;

    private Transform _targetWagon;
    private Camera _mainCamera;

    private Tween _fadeTween;
    private Tween _sliderTween;
    private Tween _hideDelayTween;
    private Tween _scaleTween;

    private void Awake()
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public void Initialize(Transform targetWagon)
    {
        _targetWagon = targetWagon;
        _mainCamera = Camera.main;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    private void LateUpdate()
    {
        if (_targetWagon == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = _targetWagon.position + _offset;

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera != null)
        {
            transform.rotation = _mainCamera.transform.rotation;
        }
    }

    public void UpdateHealth(float currentHp, float maxHp, bool showTemp = true)
    {
        float safeHp = Mathf.Max(0f, currentHp);

        if (_slider != null)
        {
            _slider.maxValue = maxHp;
            _sliderTween?.Kill();
            _sliderTween = _slider.DOValue(safeHp, _hpFillDuration).SetEase(Ease.OutQuad);
        }

        if (_hpText != null)
        {
            _hpText.text = $"{Mathf.CeilToInt(safeHp)} / {Mathf.CeilToInt(maxHp)}";
        }

        if (showTemp)
        {
            Show(2f);
        }
    }

    public void Show(float duration = 2f)
    {
        if (_canvasGroup == null) return;

        _fadeTween?.Kill();
        _hideDelayTween?.Kill();
        _scaleTween?.Kill();

        _fadeTween = _canvasGroup.DOFade(1f, _fadeDuration).SetEase(Ease.OutCubic);

        transform.localScale = Vector3.one;
        _scaleTween = transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.2f, 5, 0.5f);

        _hideDelayTween = DOVirtual.DelayedCall(duration, () =>
        {
            if (_canvasGroup != null)
            {
                _fadeTween = _canvasGroup.DOFade(0f, _fadeDuration * 1.5f).SetEase(Ease.InCubic);
            }
        });
    }

    private void OnDestroy()
    {
        _fadeTween?.Kill();
        _sliderTween?.Kill();
        _hideDelayTween?.Kill();
        _scaleTween?.Kill();
    }
}