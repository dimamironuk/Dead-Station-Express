using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class GameOverPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private RectTransform _panelTransform;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _traveledText;
    [SerializeField] private TMP_Text _bestDistanceText;
    [SerializeField] private Button _tryAgainButton;

    [Header("Audio References")]
    [SerializeField] private AudioSource _trainAudioSource;

    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.5f;

    private const string BEST_DISTANCE_KEY = "BestDistance";

    private void Awake()
    {
        if (_tryAgainButton != null)
        {
            _tryAgainButton.onClick.AddListener(RestartGame);
        }

        if (_panel != null)
        {
            _panel.SetActive(false);
        }
    }

    public void ShowGameOver(float currentDistance)
    {
        if (_trainAudioSource != null)
        {
            _trainAudioSource.Stop();
        }

        Time.timeScale = 0f;

        float bestDistance = PlayerPrefs.GetFloat(BEST_DISTANCE_KEY, 0f);

        if (currentDistance > bestDistance)
        {
            bestDistance = currentDistance;
            PlayerPrefs.SetFloat(BEST_DISTANCE_KEY, bestDistance);
            PlayerPrefs.Save();
        }

        if (_traveledText != null)
        {
            _traveledText.text = $"{currentDistance:F1} km";
        }

        if (_bestDistanceText != null)
        {
            _bestDistanceText.text = $"{bestDistance:F1} km";
        }

        if (_panel != null)
        {
            _panel.SetActive(true);
            AnimatePanel();
        }
    }

    private void AnimatePanel()
    {
        if (_panelTransform != null)
        {
            _panelTransform.localScale = Vector3.zero;
            _panelTransform.DOScale(Vector3.one, _animationDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, _animationDuration)
                .SetUpdate(true);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}