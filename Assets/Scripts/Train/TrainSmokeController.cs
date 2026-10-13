using UnityEngine;
using Zenject;

public class TrainSmokeController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _smokeParticle;

    private JourneyManager _journeyManager;

    [Inject]
    public void Construct(JourneyManager journeyManager)
    {
        _journeyManager = journeyManager;
    }

    private void Awake()
    {
        if (_smokeParticle == null)
            _smokeParticle = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached += StopSmoke;
            _journeyManager.OnStationDeparted += StartSmoke;
        }
    }

    private void OnDisable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached -= StopSmoke;
            _journeyManager.OnStationDeparted -= StartSmoke;
        }
    }

    private void Start()
    {
        if (_journeyManager != null)
        {
            if (_journeyManager.IsTraveling)
            {
                StartSmoke();
            }
            else
            {
                StopSmoke();
            }
        }
    }

    private void StartSmoke()
    {
        if (_smokeParticle != null)
        {
            _smokeParticle.Play();
        }
    }

    private void StopSmoke()
    {
        if (_smokeParticle != null)
        {
            _smokeParticle.Stop();
        }
    }
}