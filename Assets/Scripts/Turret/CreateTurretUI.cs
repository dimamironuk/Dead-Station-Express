using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[Serializable]
public class TurretData
{
    public string Name;
    public Sprite Icon;
    public int Price;
    [TextArea] public string Parameters;
    public GameObject Prefab;
    public GameObject PrefabGreen;
    public bool IsPurchased;
}

public class CreateTurretUI : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _shopCardPrefab;
    [SerializeField] private GameObject _selectButtonPrefab;

    [Header("Spawn Containers")]
    [SerializeField] private Transform _shopContentContainer;
    [SerializeField] private Transform _selectionPanelContainer;

    [Header("Turret Database")]
    [SerializeField] private List<TurretData> _turrets;

    private DiContainer _container;
    private CurrencyManager _currencyManager;

    [Inject]
    public void Construct(DiContainer container, CurrencyManager currencyManager)
    {
        _container = container;
        _currencyManager = currencyManager;
    }

    private void Start()
    {
        InitShopAndSelection();
    }

    public List<string> GetPurchasedTurretNames()
    {
        List<string> purchased = new List<string>();
        foreach (var turret in _turrets)
        {
            if (turret.IsPurchased)
            {
                purchased.Add(turret.Name);
            }
        }
        return purchased;
    }

    public void LoadPurchasedTurrets(List<string> purchasedNames)
    {
        if (purchasedNames == null) return;

        foreach (var turret in _turrets)
        {
            turret.IsPurchased = purchasedNames.Contains(turret.Name);
        }

        RebuildUI();
    }

    private void InitShopAndSelection()
    {
        RebuildUI();
    }

    private void RebuildUI()
    {
        if (_shopContentContainer != null)
        {
            foreach (Transform child in _shopContentContainer)
            {
                Destroy(child.gameObject);
            }
        }

        if (_selectionPanelContainer != null)
        {
            foreach (Transform child in _selectionPanelContainer)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (var turretData in _turrets)
        {
            if (_shopCardPrefab != null && _shopContentContainer != null)
            {
                GameObject cardObj = _container.InstantiatePrefab(_shopCardPrefab, _shopContentContainer);

                if (cardObj.TryGetComponent<BuyTurretCardUI>(out var shopCard))
                {
                    shopCard.Setup(turretData, TryBuyTurret);
                }
            }

            if (turretData.IsPurchased)
            {
                AddTurretToSelectionPanel(turretData);
            }
        }
    }

    private void TryBuyTurret(TurretData turretData, BuyTurretCardUI shopCard)
    {
        if (_currencyManager != null && _currencyManager.TrySpendNuts(turretData.Price))
        {
            turretData.IsPurchased = true;
            shopCard.UpdateState();
            AddTurretToSelectionPanel(turretData);
        }
    }

    private void AddTurretToSelectionPanel(TurretData turretData)
    {
        if (_selectionPanelContainer == null || _selectButtonPrefab == null) return;

        GameObject selectBtnObj = _container.InstantiatePrefab(_selectButtonPrefab, _selectionPanelContainer);

        if (selectBtnObj.TryGetComponent<TurretUI>(out var turretUI))
        {
            turretUI.Setup(turretData);
        }
    }
}