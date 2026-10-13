using UnityEngine;
using Zenject;
using DG.Tweening;

public class TrainSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float _rollAmount = 2.5f;
    [SerializeField] private float _rollSpeed = 3.0f;
    [SerializeField] private float _pitchAmount = 1.0f;
    [SerializeField] private float _pitchSpeed = 4.0f;
    [SerializeField] private float _bounceAmount = 0.05f;
    [SerializeField] private float _bounceSpeed = 12.0f;
    [SerializeField, Range(0f, 1f)] private float _intensity = 0f;
    [SerializeField] private float _swayFadeDuration = 2.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _moveClip;
    [SerializeField] private AudioClip _stopClip;
    [SerializeField, Range(0f, 1f)] private float _maxVolume = 1f;

    [Header("Stop Settings")]
    [SerializeField] private float _stopSoundPitch = 1.3f; // Швидкість звуку зупинки (1.3 = прискорення на 30%)
    [SerializeField] private float _stopSoundDuration = 12.4f;
    [SerializeField] private float _stopFadeOutDuration = 3f;

    private Tween _intensityTween;
    private Tween _audioTween;
    private Vector3 _startLocalPos;
    private Quaternion _startLocalRot;
    private JourneyManager _journeyManager;

    [Inject]
    public void Construct(JourneyManager journeyManager)
    {
        _journeyManager = journeyManager;
    }

    private void Awake()
    {
        _startLocalPos = transform.localPosition;
        _startLocalRot = transform.localRotation;

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached += StopSway;
            _journeyManager.OnStationDeparted += StartSway;
        }
    }

    private void OnDisable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached -= StopSway;
            _journeyManager.OnStationDeparted -= StartSway;
        }
    }

    private void Start()
    {
        if (_journeyManager != null)
        {
            SetMovingState(_journeyManager.IsTraveling, 0f, playStopSound: false);
        }
    }

    private void Update()
    {
        if (_intensity <= 0.001f) return;

        float time = Time.time;
        float rollNoise = (Mathf.PerlinNoise(time * _rollSpeed, 0f) - 0.5f) * 2f;
        float rollAngle = rollNoise * _rollAmount * _intensity;

        float pitchNoise = (Mathf.PerlinNoise(0f, time * _pitchSpeed) - 0.5f) * 2f;
        float pitchAngle = pitchNoise * _pitchAmount * _intensity;

        transform.localRotation = _startLocalRot * Quaternion.Euler(pitchAngle, 0f, rollAngle);

        float bounceNoise = (Mathf.Sin(time * _bounceSpeed) + Mathf.PerlinNoise(time * 20f, 0f) - 0.5f);
        float bounceY = bounceNoise * _bounceAmount * _intensity;

        transform.localPosition = _startLocalPos + new Vector3(0f, bounceY, 0f);
    }

    private void StartSway() => SetMovingState(true, _swayFadeDuration);
    private void StopSway() => SetMovingState(false, _swayFadeDuration, playStopSound: true);

    public void SetMovingState(bool isMoving, float fadeDuration = 1.5f, bool playStopSound = true)
    {
        float targetIntensity = isMoving ? 1f : 0f;

        _intensityTween?.Kill();
        _audioTween?.Kill();

        if (_audioSource != null)
        {
            if (isMoving)
            {
                _audioSource.pitch = 1f; // Повертаємо стандартну швидкість для руху

                if (_moveClip != null)
                {
                    _audioSource.clip = _moveClip;
                    _audioSource.loop = true;

                    if (!_audioSource.isPlaying)
                    {
                        _audioSource.Play();
                    }

                    if (fadeDuration <= 0f)
                    {
                        _audioSource.volume = _maxVolume;
                    }
                    else
                    {
                        _audioTween = _audioSource.DOFade(_maxVolume, fadeDuration);
                    }
                }
            }
            else
            {
                if (playStopSound && _stopClip != null)
                {
                    _audioSource.pitch = _stopSoundPitch; // Встановлюємо прискорений pitch
                    _audioSource.clip = _stopClip;
                    _audioSource.loop = false;
                    _audioSource.volume = _maxVolume;
                    _audioSource.Play();

                    // Рахуємо фактичну тривалість з урахуванням прискорення
                    float effectiveDuration = _stopSoundDuration / Mathf.Max(0.1f, _stopSoundPitch);
                    float delayBeforeFade = Mathf.Max(0f, effectiveDuration - _stopFadeOutDuration);

                    Sequence stopSequence = DOTween.Sequence();
                    stopSequence.AppendInterval(delayBeforeFade);
                    stopSequence.Append(_audioSource.DOFade(0f, _stopFadeOutDuration));
                    stopSequence.OnComplete(() =>
                    {
                        _audioSource.Stop();
                        _audioSource.pitch = 1f;
                    });

                    _audioTween = stopSequence;
                }
                else
                {
                    _audioSource.Stop();
                    _audioSource.pitch = 1f;
                }
            }
        }

        if (fadeDuration <= 0f)
        {
            _intensity = targetIntensity;

            if (!isMoving)
            {
                transform.localPosition = _startLocalPos;
                transform.localRotation = _startLocalRot;
            }
            return;
        }

        _intensityTween = DOTween.To(() => _intensity, x => _intensity = x, targetIntensity, fadeDuration)
            .OnComplete(() =>
            {
                if (!isMoving)
                {
                    transform.DOLocalMove(_startLocalPos, 0.5f);
                    transform.DOLocalRotateQuaternion(_startLocalRot, 0.5f);
                }
            });
    }
}