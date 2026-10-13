using UnityEngine;
using Zenject;

public class StationSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _stationPrefab;
    [SerializeField] private RailLoopController _railLoopController;

    [Header("Spawn Position")]
    [SerializeField] private float _spawnZ = 70f;
    [SerializeField] private float _xPos = 21f;
    [SerializeField] private float _yPos = 0.42f;

    [Header("Hide")]
    [SerializeField] private float _hideZ = -50f;

    [Header("Spawn Progress")]
    [Range(0, 1)]
    [SerializeField] private float _spawnProgress = 0.96f;

    [Header("Clearance Settings")]
    [SerializeField] private Vector3 _stationClearanceSize = new Vector3(45f, 10f, 75f);

    private JourneyManager _journeyManager;
    private GameObject _stationInstance;
    private bool _isSpawned;
    private bool _isDeparting;

    [Inject]
    public void Construct(JourneyManager journeyManager)
    {
        _journeyManager = journeyManager;
        _journeyManager.OnProgressChanged += HandleProgress;
        _journeyManager.OnStationReached += HandleStationReached;
        _journeyManager.OnStationDeparted += HandleStationDeparted;
    }

    private void Awake()
    {
        _stationInstance = Instantiate(
            _stationPrefab,
            new Vector3(_xPos, _yPos, _hideZ - 20),
            Quaternion.Euler(0, 90, 0));

        _stationInstance.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_journeyManager == null)
            return;

        _journeyManager.OnProgressChanged -= HandleProgress;
        _journeyManager.OnStationReached -= HandleStationReached;
        _journeyManager.OnStationDeparted -= HandleStationDeparted;
    }

    private void HandleProgress(float progress)
    {
        if (_isSpawned)
            return;

        if (progress >= _spawnProgress)
        {
            _stationInstance.transform.position = new Vector3(_xPos, _yPos, _spawnZ);
            _stationInstance.SetActive(true);
            _isSpawned = true;

            ClearAreaForStation();
        }
    }

    private void ClearAreaForStation()
    {
        Vector3 center = _stationInstance.transform.position;

        var recyclers = FindObjectsOfType<RailDecorationRecycler>();
        foreach (var recycler in recyclers)
        {
            recycler.ClearArea(center, _stationClearanceSize);
        }

        var grassSpawners = FindObjectsOfType<GrassChunkSpawner>();
        foreach (var spawner in grassSpawners)
        {
            spawner.ClearArea(center, _stationClearanceSize);
        }
    }

    private void HandleStationReached()
    {
        _isDeparting = false;
    }

    private void HandleStationDeparted()
    {
        _isDeparting = true;
        _isSpawned = false;
    }

    private void Update()
    {
        if (!_stationInstance.activeSelf)
            return;

        if (_railLoopController.CurrentSpeed > 0)
        {
            _stationInstance.transform.Translate(
                Vector3.back *
                _railLoopController.CurrentSpeed *
                Time.deltaTime,
                Space.World);
        }

        if (_isDeparting &&
            _stationInstance.transform.position.z <= _hideZ)
        {
            _stationInstance.SetActive(false);
            _isDeparting = false;
        }
    }
}