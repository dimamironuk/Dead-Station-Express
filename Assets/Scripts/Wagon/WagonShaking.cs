using UnityEngine;
using Zenject;
using DG.Tweening;

public class WagonShaking : MonoBehaviour
{
    [SerializeField] private Transform _visualTransform;
    [SerializeField] private float _rollAmount = 3.5f;
    [SerializeField] private float _rollSpeed = 2.5f;
    [SerializeField] private float _pitchAmount = 1.5f;
    [SerializeField] private float _pitchSpeed = 3.5f;
    [SerializeField] private float _bounceAmount = 0.04f;
    [SerializeField] private float _bounceSpeed = 10f;

    private Vector3 _initialLocalPosition;
    private Quaternion _initialLocalRotation;
    private float _randomPhase;
    private float _intensity = 0f;
    private Tween _intensityTween;

    private JourneyManager _journeyManager;

    [Inject]
    public void Construct(JourneyManager journeyManager)
    {
        _journeyManager = journeyManager;
    }

    private void Awake()
    {
        if (_visualTransform == null)
            _visualTransform = transform;

        _initialLocalPosition = _visualTransform.localPosition;
        _initialLocalRotation = _visualTransform.localRotation;
        _randomPhase = Random.Range(100f, 999f);
    }

    private void OnEnable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached += StopShaking;
            _journeyManager.OnStationDeparted += StartShaking;
        }
    }

    private void OnDisable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached -= StopShaking;
            _journeyManager.OnStationDeparted -= StartShaking;
        }
    }

    private void Start()
    {
        if (_journeyManager != null)
        {
            SetState(_journeyManager.IsTraveling, 0f);
        }
    }

    private void Update()
    {
        if (_intensity <= 0.001f) return;

        float time = Time.time + _randomPhase;

        float rollNoise = (Mathf.PerlinNoise(time * _rollSpeed, 0f) - 0.5f) * 2f;
        float rollAngle = rollNoise * _rollAmount * _intensity;

        float pitchNoise = (Mathf.PerlinNoise(0f, time * _pitchSpeed) - 0.5f) * 2f;
        float pitchAngle = pitchNoise * _pitchAmount * _intensity;

        float bounceNoise = (Mathf.Sin(time * _bounceSpeed) + Mathf.PerlinNoise(time * 15f, 0f) - 0.5f);
        float bounceY = bounceNoise * _bounceAmount * _intensity;

        _visualTransform.localPosition = _initialLocalPosition + new Vector3(0f, bounceY, 0f);
        _visualTransform.localRotation = _initialLocalRotation * Quaternion.Euler(pitchAngle, 0f, rollAngle);
    }

    private void StartShaking() => SetState(true);
    private void StopShaking() => SetState(false);

    public void SetState(bool isMoving, float fadeDuration = 1.5f)
    {
        float targetIntensity = isMoving ? 1f : 0f;

        _intensityTween?.Kill();

        if (fadeDuration <= 0f)
        {
            _intensity = targetIntensity;
            if (!isMoving)
            {
                _visualTransform.localPosition = _initialLocalPosition;
                _visualTransform.localRotation = _initialLocalRotation;
            }
            return;
        }

        _intensityTween = DOTween.To(() => _intensity, x => _intensity = x, targetIntensity, fadeDuration)
            .OnComplete(() =>
            {
                if (!isMoving)
                {
                    _visualTransform.DOLocalMove(_initialLocalPosition, 0.5f);
                    _visualTransform.DOLocalRotateQuaternion(_initialLocalRotation, 0.5f);
                }
            });
    }
}