using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShopController : MonoBehaviour
{
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private Button _openShopButton;
    [SerializeField] private Button _closeShopButton;

    [SerializeField] private float _animationDuration = 0.3f;
    [SerializeField] private Ease _openEase = Ease.OutBack;
    [SerializeField] private Ease _closeEase = Ease.InBack;

    [System.Serializable]
    public class TabData
    {
        public string TabName;
        public Button TabButton;
        public GameObject PagePanel;
        public Image ButtonImage;
    }

    [SerializeField] private TabData[] _tabs;
    [SerializeField] private Color _activeTabColor = Color.white;
    [SerializeField] private Color _inactiveTabColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Vector3 _activeTabScale = new Vector3(1.05f, 1.05f, 1f);
    [SerializeField] private Vector3 _inactiveTabScale = Vector3.one;

    private Tween _shopTween;

    private void Start()
    {
        if (_openShopButton != null)
            _openShopButton.onClick.AddListener(OpenShop);

        if (_closeShopButton != null)
            _closeShopButton.onClick.AddListener(CloseShop);

        if (_tabs != null)
        {
            for (int i = 0; i < _tabs.Length; i++)
            {
                int index = i;
                if (_tabs[i].TabButton != null)
                {
                    _tabs[i].TabButton.onClick.AddListener(() => SelectTab(index));
                }
            }
        }
    }

    public void OpenShop()
    {
        if (_shopPanel == null) return;

        _shopTween?.Kill();
        _shopPanel.SetActive(true);
        _shopPanel.transform.localScale = Vector3.zero;

        _shopTween = _shopPanel.transform.DOScale(Vector3.one, _animationDuration)
            .SetEase(_openEase)
            .SetUpdate(true);

        if (_tabs != null && _tabs.Length > 0)
        {
            SelectTab(0);
        }
    }

    public void CloseShop()
    {
        if (_shopPanel == null) return;

        _shopTween?.Kill();
        _shopTween = _shopPanel.transform.DOScale(Vector3.zero, _animationDuration)
            .SetEase(_closeEase)
            .SetUpdate(true)
            .OnComplete(() => _shopPanel.SetActive(false));
    }

    public void SelectTab(int tabIndex)
    {
        for (int i = 0; i < _tabs.Length; i++)
        {
            bool isActive = (i == tabIndex);

            if (_tabs[i].PagePanel != null)
            {
                if (isActive)
                {
                    if (!_tabs[i].PagePanel.activeSelf)
                    {
                        _tabs[i].PagePanel.SetActive(true);
                        _tabs[i].PagePanel.transform.DOKill();
                        _tabs[i].PagePanel.transform.localScale = Vector3.one * 0.95f;
                        _tabs[i].PagePanel.transform.DOScale(Vector3.one, _animationDuration)
                            .SetEase(Ease.OutQuad)
                            .SetUpdate(true);
                    }
                }
                else
                {
                    _tabs[i].PagePanel.SetActive(false);
                }
            }

            if (_tabs[i].ButtonImage != null)
            {
                _tabs[i].ButtonImage.DOKill();
                Color targetColor = isActive ? _activeTabColor : _inactiveTabColor;
                _tabs[i].ButtonImage.DOColor(targetColor, _animationDuration)
                    .SetUpdate(true);
            }

            if (_tabs[i].TabButton != null)
            {
                _tabs[i].TabButton.transform.DOKill();
                Vector3 targetScale = isActive ? _activeTabScale : _inactiveTabScale;
                _tabs[i].TabButton.transform.DOScale(targetScale, _animationDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);
            }
        }
    }
}