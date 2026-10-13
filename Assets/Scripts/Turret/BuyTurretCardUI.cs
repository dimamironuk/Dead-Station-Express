using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyTurretCardUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _parametersText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private Button _buyButton;
    [SerializeField] private GameObject _boltIcon;

    private TurretData _data;
    private Action<TurretData, BuyTurretCardUI> _onBuyClicked;

    public void Setup(TurretData data, Action<TurretData, BuyTurretCardUI> onBuyClicked)
    {
        _data = data;
        _onBuyClicked = onBuyClicked;

        if (_nameText != null) _nameText.text = data.Name;
        if (_parametersText != null) _parametersText.text = data.Parameters;
        if (_iconImage != null && data.Icon != null) _iconImage.sprite = data.Icon;

        _buyButton.onClick.RemoveAllListeners();
        _buyButton.onClick.AddListener(OnButtonClick);

        UpdateState();
    }

    public void UpdateState()
    {
        if (_data.IsPurchased)
        {
            _buyButton.interactable = false;

            if (_priceText != null) _priceText.text = "<color=#888888>OWNED</color>";
            if (_boltIcon != null) _boltIcon.SetActive(false);
        }
        else
        {
            _buyButton.interactable = true;

            if (_priceText != null) _priceText.text = $"Price: <color=#FFCC00>{_data.Price}</color>";
            if (_boltIcon != null) _boltIcon.SetActive(true);
        }
    }

    private void OnButtonClick()
    {
        _onBuyClicked?.Invoke(_data, this);
    }
}