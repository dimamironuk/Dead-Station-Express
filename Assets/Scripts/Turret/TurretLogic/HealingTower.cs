using UnityEngine;
using DG.Tweening;

public class HealingTower : BaseTurret
{
    [Header("Heal Settings")]
    [SerializeField] private float _healRadius = 6f;
    [SerializeField] private float _healInterval = 1.5f;
    [SerializeField] private LayerMask _wagonLayer;

    [Header("Cross Visuals")]
    [SerializeField] private Transform _crossTransform;
    [SerializeField] private float _rotationSpeed = 90f;
    [SerializeField] private float _floatAmplitude = 0.15f;
    [SerializeField] private float _floatSpeed = 2f;

    private readonly Collider[] _hitBuffer = new Collider[16];
    private float _timer;
    private Vector3 _baseCrossScale;
    private Vector3 _startLocalPos;

    private void Start()
    {
        if (_crossTransform != null)
        {
            _baseCrossScale = _crossTransform.localScale;
            _startLocalPos = _crossTransform.localPosition;
        }
    }

    private void Update()
    {
        AnimateCross();

        _timer += Time.deltaTime;
        if (_timer < _healInterval) return;

        _timer = 0f;
        TryHealNearbyWagons();
    }

    private void AnimateCross()
    {
        if (_crossTransform == null) return;

        _crossTransform.Rotate(Vector3.up * (_rotationSpeed * Time.deltaTime), Space.World);

        float newY = _startLocalPos.y + Mathf.Sin(Time.time * _floatSpeed) * _floatAmplitude;
        _crossTransform.localPosition = new Vector3(_startLocalPos.x, newY, _startLocalPos.z);
    }

    private void TryHealNearbyWagons()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _healRadius, _hitBuffer, _wagonLayer);

        bool healedAny = false;

        for (int i = 0; i < count; i++)
        {
            if (_hitBuffer[i].TryGetComponent<Wagon>(out var wagon))
            {
                if (wagon.IsMaxHealth) continue;

                if (wagon.Heal(CurrentStat))
                {
                    healedAny = true;
                }
            }
        }

        if (healedAny && _crossTransform != null)
        {
            AnimateCrossPulse();
        }
    }

    private void AnimateCrossPulse()
    {
        _crossTransform.DOKill(true);
        _crossTransform.localScale = _baseCrossScale;

        _crossTransform.DOPunchScale(Vector3.one * 0.35f, 0.4f, 5, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _healRadius);
    }
}