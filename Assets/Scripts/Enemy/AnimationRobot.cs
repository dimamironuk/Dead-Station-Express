using UnityEngine;
using DG.Tweening;
using System;

public class AnimationRobot : MonoBehaviour
{
    [Header("Model References")]
    [SerializeField] private Transform _wheel;
    [SerializeField] private Transform _mainBody;
    [SerializeField] private ParticleSystem _vfx;

    [Header("Wheel Settings")]
    [SerializeField] private float _wheelSpeed = 0.5f;

    [Header("Movement Lean")]
    [SerializeField] private float _leanBackwardAngle = -12f;
    [SerializeField] private float _leanDuration = 0.3f;

    [Header("Movement Shake & Bounce")]
    [SerializeField] private float _bounceHeight = 0.04f;
    [SerializeField] private float _wobbleAngle = 3f;
    [SerializeField] private float _shakeSpeed = 0.1f;

    [Header("Attack Settings")]
    [SerializeField] private float _punchDistance = 0.35f;
    [SerializeField] private float _punchDuration = 0.12f;

    [Header("Death Settings")]
    [SerializeField] private float _fallAngle = 80f;
    [SerializeField] private float _deathDuration = 0.4f;

    private Tween _wheelTween;
    private Tween _leanTween;
    private Sequence _shakeSequence;
    private Sequence _attackSequence;
    private Sequence _deathSequence;
    private bool _isMoving;

    private Vector3 _defaultMainEuler;
    private Vector3 _defaultMainPos;
    private Vector3 _defaultMainScale;
    private Vector3 _defaultWheelEuler;

    private void Awake()
    {
        if (_mainBody != null)
        {
            _defaultMainEuler = _mainBody.localEulerAngles;
            _defaultMainPos = _mainBody.localPosition;
            _defaultMainScale = _mainBody.localScale;
        }

        if (_wheel != null)
        {
            _defaultWheelEuler = _wheel.localEulerAngles;
        }
    }

    public void SetMoving(bool isMoving)
    {
        if (_isMoving == isMoving) return;
        _isMoving = isMoving;

        if (_isMoving)
        {
            StartMovingAnimation();
        }
        else
        {
            StopMovingAnimation();
        }
    }

    private void StartMovingAnimation()
    {
        if (_mainBody == null || _wheel == null) return;

        _wheelTween?.Kill();
        Vector3 targetWheelRotation = _defaultWheelEuler + new Vector3(360f, 0f, 0f);
        _wheelTween = _wheel.DOLocalRotate(targetWheelRotation, _wheelSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);

        _leanTween?.Kill();
        Vector3 targetEuler = _defaultMainEuler + new Vector3(_leanBackwardAngle, 0f, 0f);
        _leanTween = _mainBody.DOLocalRotate(targetEuler, _leanDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(StartShakeLoop);

        if (_vfx != null)
        {
            _vfx.Play();
        }
    }

    private void StartShakeLoop()
    {
        if (!_isMoving || _mainBody == null) return;

        _shakeSequence?.Kill();
        _shakeSequence = DOTween.Sequence();

        Vector3 baseEuler = _defaultMainEuler + new Vector3(_leanBackwardAngle, 0f, 0f);
        Vector3 posUp = _defaultMainPos + new Vector3(0f, _bounceHeight, 0f);

        _shakeSequence.Append(_mainBody.DOLocalMove(posUp, _shakeSpeed).SetEase(Ease.InOutSine))
            .Join(_mainBody.DOLocalRotate(baseEuler + new Vector3(0f, 0f, _wobbleAngle), _shakeSpeed).SetEase(Ease.InOutSine))
            .Append(_mainBody.DOLocalMove(_defaultMainPos, _shakeSpeed).SetEase(Ease.InOutSine))
            .Join(_mainBody.DOLocalRotate(baseEuler + new Vector3(0f, 0f, -_wobbleAngle), _shakeSpeed).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopMovingAnimation()
    {
        _wheelTween?.Kill();
        if (_wheel != null)
        {
            _wheel.localEulerAngles = _defaultWheelEuler;
        }

        _shakeSequence?.Kill();
        _leanTween?.Kill();

        if (_mainBody != null)
        {
            _mainBody.DOLocalMove(_defaultMainPos, _leanDuration).SetEase(Ease.OutQuad);
            _leanTween = _mainBody.DOLocalRotate(_defaultMainEuler, _leanDuration)
                .SetEase(Ease.OutQuad);
        }

        if (_vfx != null)
        {
            _vfx.Stop();
        }
    }

    public void PlayAttackAnimation()
    {
        if (_mainBody == null) return;

        _shakeSequence?.Kill();
        _attackSequence?.Kill();
        _attackSequence = DOTween.Sequence();

        Vector3 backPos = _defaultMainPos + new Vector3(0f, 0f, -0.15f);
        Vector3 punchPos = _defaultMainPos + new Vector3(0f, 0f, _punchDistance);

        _attackSequence.Append(_mainBody.DOLocalMove(backPos, 0.08f).SetEase(Ease.OutQuad));
        _attackSequence.Join(_mainBody.DOLocalRotate(_defaultMainEuler + new Vector3(-20f, 0f, 0f), 0.08f));

        _attackSequence.Append(_mainBody.DOLocalMove(punchPos, _punchDuration).SetEase(Ease.InQuad));
        _attackSequence.Join(_mainBody.DOLocalRotate(_defaultMainEuler + new Vector3(15f, 0f, 0f), _punchDuration));

        _attackSequence.Append(_mainBody.DOLocalMove(_defaultMainPos, 0.2f).SetEase(Ease.OutBack));
        _attackSequence.Join(_mainBody.DOLocalRotate(_defaultMainEuler, 0.2f));

        if (_isMoving)
        {
            _attackSequence.OnComplete(StartShakeLoop);
        }
    }

    public void PlayDeathAnimation(Action onComplete)
    {
        _wheelTween?.Kill();
        _leanTween?.Kill();
        _shakeSequence?.Kill();
        _attackSequence?.Kill();
        _deathSequence?.Kill();

        if (_vfx != null)
        {
            _vfx.Stop();
        }

        if (_mainBody == null)
        {
            onComplete?.Invoke();
            return;
        }

        _deathSequence = DOTween.Sequence();

        Vector3 fallEuler = _defaultMainEuler + new Vector3(_fallAngle, 0f, 0f);
        Vector3 fallPos = _defaultMainPos + new Vector3(0f, -0.35f, 0f);

        _deathSequence.Append(_mainBody.DOLocalRotate(fallEuler, _deathDuration).SetEase(Ease.InBounce));
        _deathSequence.Join(_mainBody.DOLocalRotate(fallEuler, _deathDuration).SetEase(Ease.InBounce));
        _deathSequence.Join(_mainBody.DOLocalMove(fallPos, _deathDuration).SetEase(Ease.InBounce));
        _deathSequence.Append(_mainBody.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));

        _deathSequence.OnComplete(() => onComplete?.Invoke());
    }

    private void OnDisable()
    {
        _wheelTween?.Kill();
        _leanTween?.Kill();
        _shakeSequence?.Kill();
        _attackSequence?.Kill();
        _deathSequence?.Kill();
        _isMoving = false;

        if (_mainBody != null)
        {
            _mainBody.localPosition = _defaultMainPos;
            _mainBody.localEulerAngles = _defaultMainEuler;
            _mainBody.localScale = _defaultMainScale;
        }

        if (_wheel != null)
        {
            _wheel.localEulerAngles = _defaultWheelEuler;
        }

        if (_vfx != null)
        {
            _vfx.Stop();
        }
    }
}