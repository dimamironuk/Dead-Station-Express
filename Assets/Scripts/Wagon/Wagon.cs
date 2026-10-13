using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Zenject;

[RequireComponent(typeof(Collider))]
public class Wagon : MonoBehaviour
{
    [SerializeField] private Transform _turretSlot;
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private ParticleSystem _healParticle;

    [Header("UI HP Bar Prefab")]
    [SerializeField] private WagonHPBar _hpBarPrefab;

    private float _currentHealth;
    private int _wagonLevel = 1;
    private int _turretLevel = 1;

    private WagonHPBar _barInstance;
    private GameObject _installedTurret;
    private BaseTurret _installedBaseTurret;
    private Sprite _installedTurretIcon;
    private int _installedTurretPrice;
    private TrainBuilder _trainBuilder;
    private Canvas _parentCanvas;

    private WagonUpgradeUI _upgradeUI;
    private TurretPlacementController _placementController;
    private TurretDemolitionController _demolitionController;

    private Vector3 _touchStartPosition;
    private bool _isSwiping;
    private float _lastTurretModifiedTime = -1f;

    public Transform TurretSlot => _turretSlot != null ? _turretSlot : transform;
    public bool HasTurret => _installedTurret != null;
    public GameObject InstalledTurret => _installedTurret;
    public BaseTurret InstalledBaseTurret => _installedBaseTurret;
    public Sprite InstalledTurretIcon => _installedTurretIcon;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsMaxHealth => _currentHealth >= _maxHealth;
    public int InstalledTurretPrice => _installedTurretPrice;
    public int WagonLevel => _wagonLevel;
    public int TurretLevel => _turretLevel;

    [Inject]
    public void Construct(
        [InjectOptional] Canvas parentCanvas,
        [InjectOptional] WagonUpgradeUI upgradeUI,
        [InjectOptional] TurretPlacementController placementController,
        [InjectOptional] TurretDemolitionController demolitionController)
    {
        _parentCanvas = parentCanvas;
        _upgradeUI = upgradeUI;
        _placementController = placementController;
        _demolitionController = demolitionController;
    }

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void Init(TrainBuilder trainBuilder)
    {
        _trainBuilder = trainBuilder;

        if (_hpBarPrefab != null && _parentCanvas != null)
        {
            _barInstance = Instantiate(_hpBarPrefab, _parentCanvas.transform);
            _barInstance.Initialize(transform);
            _barInstance.UpdateHealth(_currentHealth, _maxHealth, false);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _touchStartPosition = Input.mousePosition;
            _isSwiping = false;
        }

        if (Input.GetMouseButton(0))
        {
            if (Vector3.Distance(_touchStartPosition, Input.mousePosition) > 15f)
            {
                _isSwiping = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_isSwiping || IsPointerOverUI() || IsActionBlocked()) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    HandleWagonTap();
                }
            }
        }
    }

    private bool IsActionBlocked()
    {
        bool placementBlocked = _placementController != null && _placementController.IsPlacingOrJustCancelled;
        bool demolitionBlocked = _demolitionController != null && _demolitionController.IsDemolitionActiveOrJustCancelled;
        bool recentlyModified = (Time.time - _lastTurretModifiedTime) < 0.3f;

        return placementBlocked || demolitionBlocked || recentlyModified;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void HandleWagonTap()
    {
        if (IsActionBlocked()) return;

        if (_barInstance != null)
        {
            _barInstance.Show(2f);
        }

        if (_upgradeUI != null)
        {
            _upgradeUI.Open(this);
        }
    }

    public bool Heal(float amount)
    {
        if (IsMaxHealth || _currentHealth <= 0f)
            return false;

        _currentHealth += amount;

        if (_healParticle != null)
        {
            _healParticle.Play();
        }

        if (_currentHealth >= _maxHealth)
        {
            _currentHealth = _maxHealth;

            if (_healParticle != null)
            {
                _healParticle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        if (_barInstance != null)
        {
            _barInstance.UpdateHealth(_currentHealth, _maxHealth, true);
        }

        return true;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        if (_barInstance != null)
        {
            _barInstance.UpdateHealth(_currentHealth, _maxHealth, true);
        }

        if (_currentHealth <= 0)
        {
            DestroyWagon();
        }
    }

    private void DestroyWagon()
    {
        if (_barInstance != null)
        {
            Destroy(_barInstance.gameObject);
        }

        if (_trainBuilder != null)
        {
            _trainBuilder.RemoveWagonAndBehind(this);
        }
    }

    public bool TryPlaceTurret(TurretData turretData)
    {
        if (turretData == null) return false;
        return TryPlaceTurret(turretData.Prefab, turretData.Price, turretData.Icon);
    }

    public bool TryPlaceTurret(GameObject turretPrefab, int price, Sprite icon = null)
    {
        if (HasTurret || turretPrefab == null)
            return false;

        _lastTurretModifiedTime = Time.time;
        _installedTurretPrice = price;
        _installedTurretIcon = icon;
        _turretLevel = 1;

        _installedTurret = Instantiate(turretPrefab, TurretSlot);
        _installedTurret.transform.localPosition = Vector3.zero;
        _installedTurret.transform.localRotation = Quaternion.identity;

        _installedTurret.transform.localScale = Vector3.zero;
        _installedTurret.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);

        if (_installedTurret.TryGetComponent<BaseTurret>(out _installedBaseTurret))
        {
            _installedBaseTurret.SetLevel(_turretLevel);
        }

        return true;
    }

    public void UpgradeWagon(float hpAmount = 20f)
    {
        _wagonLevel++;
        _maxHealth += hpAmount;
        _currentHealth += hpAmount;

        if (_barInstance != null)
        {
            _barInstance.UpdateHealth(_currentHealth, _maxHealth, true);
        }
    }

    public void UpgradeTurret()
    {
        if (!HasTurret) return;

        _turretLevel++;

        if (_installedBaseTurret != null)
        {
            _installedBaseTurret.SetLevel(_turretLevel);
        }
        else if (_installedTurret.TryGetComponent<BaseTurret>(out _installedBaseTurret))
        {
            _installedBaseTurret.SetLevel(_turretLevel);
        }
    }

    public int RemoveTurret(float refundPercentage = 0.5f)
    {
        if (!HasTurret) return 0;

        _lastTurretModifiedTime = Time.time;
        int refundAmount = Mathf.RoundToInt(_installedTurretPrice * refundPercentage);

        GameObject turretToDestroy = _installedTurret;
        _installedTurret = null;
        _installedBaseTurret = null;
        _installedTurretIcon = null;
        _installedTurretPrice = 0;
        _turretLevel = 1;

        turretToDestroy.transform.DOKill();
        turretToDestroy.transform.DOScale(Vector3.zero, 0.25f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                Destroy(turretToDestroy);
            });

        return refundAmount;
    }

    public void LoadState(int wagonLvl, int turretLvl, float curHp, float maxHp)
    {
        _wagonLevel = wagonLvl;
        _turretLevel = turretLvl;
        _maxHealth = maxHp;
        _currentHealth = curHp;

        if (_installedBaseTurret != null)
        {
            _installedBaseTurret.SetLevel(_turretLevel);
        }

        if (_barInstance != null)
        {
            _barInstance.UpdateHealth(_currentHealth, _maxHealth, false);
        }
    }

    private void OnDestroy()
    {
        if (_barInstance != null)
        {
            Destroy(_barInstance.gameObject);
        }
    }
}