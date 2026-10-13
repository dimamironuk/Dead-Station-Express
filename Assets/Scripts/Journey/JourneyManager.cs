using System;
using UnityEngine;

public class JourneyManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _distanceToStation = 100f;
    [SerializeField] private float _travelSpeed = 10f;

    public event Action<float> OnProgressChanged;
    public event Action OnStationReached;
    public event Action OnStationDeparted;

    private float _currentDistance;

    public bool IsTraveling { get; private set; }
    public bool IsBossActive { get; set; }

    public float Progress => Mathf.Clamp01(_currentDistance / _distanceToStation);

    private void Update()
    {
        if (!IsTraveling || IsBossActive)
            return;

        _currentDistance += _travelSpeed * Time.deltaTime;

        float progress = Progress;

        Debug.Log($"Journey Progress = {progress:F4}");

        OnProgressChanged?.Invoke(progress);

        if (_currentDistance >= _distanceToStation)
        {
            ReachStation();
        }
    }

    private void ReachStation()
    {
        IsTraveling = false;
        _currentDistance = _distanceToStation;

        OnProgressChanged?.Invoke(1f);
        OnStationReached?.Invoke();
    }

    public void DepartFromStation()
    {
        _currentDistance = 0;
        IsTraveling = true;

        OnProgressChanged?.Invoke(0f);
        OnStationDeparted?.Invoke();
    }
}