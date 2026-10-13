using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _settingsPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button _resumeBtn;
    [SerializeField] private Button _settingsBtn;
    [SerializeField] private Button _aboutBtn;
    [SerializeField] private Button _exitBtn;

    [Header("Settings Controls")]
    [SerializeField] private Slider _audioSlider;
    [SerializeField] private Slider _fpsSlider;
    [SerializeField] private Toggle _shadowToggle;
    [SerializeField] private Toggle _vibrationToggle;
    [SerializeField] private Toggle _showFpsToggle;
    [SerializeField] private TMP_Dropdown _langDropdown;
    [SerializeField] private TMP_Dropdown _graphicsDropdown;
    [SerializeField] private TextMeshProUGUI _fpsCounterText; 
    [SerializeField] private Button _backBtn;

    private Image _bgImage;
    private Coroutine _activeAnimation;

    private float _fpsDeltaTime = 0.0f;

    private void Awake()
    {
        _bgImage = GetComponent<Image>();
        AutoFindUIElements();
        InitGraphicsDropdownOptions();
        BindEvents();
    }

    private void Start()
    {
        LoadSavedSettings();
        ResumeGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0f)
                ResumeGame();
            else
                PauseGame();
        }

        UpdateFpsCounter();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (_bgImage) _bgImage.enabled = true;
        OpenMenu();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (_bgImage) _bgImage.enabled = false;
        if (_menuPanel) _menuPanel.SetActive(false);
        if (_settingsPanel) _settingsPanel.SetActive(false);
    }

    public void OpenMenu() => ShowPanel(_menuPanel);
    public void OpenSettings() => ShowPanel(_settingsPanel);

    private void ShowPanel(GameObject targetPanel)
    {
        if (_menuPanel) _menuPanel.SetActive(_menuPanel == targetPanel);
        if (_settingsPanel) _settingsPanel.SetActive(_settingsPanel == targetPanel);

        if (targetPanel != null)
        {
            if (_activeAnimation != null) StopCoroutine(_activeAnimation);
            _activeAnimation = StartCoroutine(AnimatePopIn(targetPanel.transform));
        }
    }

    private void AutoFindUIElements()
    {
        if (_menuPanel == null) _menuPanel = transform.Find("Menu")?.gameObject;
        if (_settingsPanel == null) _settingsPanel = transform.Find("Settings")?.gameObject;

        if (_menuPanel != null)
        {
            if (_resumeBtn == null) _resumeBtn = _menuPanel.transform.Find("B_Resume")?.GetComponent<Button>();
            if (_settingsBtn == null) _settingsBtn = _menuPanel.transform.Find("B_Settings")?.GetComponent<Button>();
            if (_aboutBtn == null) _aboutBtn = _menuPanel.transform.Find("B_About")?.GetComponent<Button>();
            if (_exitBtn == null) _exitBtn = _menuPanel.transform.Find("B_Exit")?.GetComponent<Button>();
        }

        if (_settingsPanel != null)
        {
            if (_audioSlider == null) _audioSlider = _settingsPanel.transform.Find("S_Audio")?.GetComponent<Slider>();
            if (_fpsSlider == null) _fpsSlider = _settingsPanel.transform.Find("S_FPS")?.GetComponent<Slider>();
            if (_backBtn == null) _backBtn = _settingsPanel.transform.Find("B_Back")?.GetComponent<Button>();

            if (_shadowToggle == null) _shadowToggle = _settingsPanel.transform.Find("Toggle_Shadow")?.GetComponent<Toggle>() ?? _settingsPanel.transform.Find("Toggle")?.GetComponent<Toggle>();
            if (_vibrationToggle == null) _vibrationToggle = _settingsPanel.transform.Find("Toggle_Vibration")?.GetComponent<Toggle>() ?? _settingsPanel.transform.Find("Toggle (1)")?.GetComponent<Toggle>();
            if (_showFpsToggle == null) _showFpsToggle = _settingsPanel.transform.Find("Toggle_FPS")?.GetComponent<Toggle>() ?? _settingsPanel.transform.Find("Toggle (2)")?.GetComponent<Toggle>();

            if (_langDropdown == null) _langDropdown = _settingsPanel.transform.Find("D_Language")?.GetComponent<TMP_Dropdown>() ?? _settingsPanel.transform.Find("Dropdown")?.GetComponent<TMP_Dropdown>();
            if (_graphicsDropdown == null) _graphicsDropdown = _settingsPanel.transform.Find("D_Graphics")?.GetComponent<TMP_Dropdown>() ?? _settingsPanel.transform.Find("Dropdown (1)")?.GetComponent<TMP_Dropdown>();
        }
    }

    private void InitGraphicsDropdownOptions()
    {
        if (_graphicsDropdown != null)
        {
            _graphicsDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>();
            foreach (var level in QualitySettings.names)
            {
                options.Add(level);
            }
            _graphicsDropdown.AddOptions(options);
        }
    }

    private void BindEvents()
    {
        if (_resumeBtn) _resumeBtn.onClick.AddListener(ResumeGame);
        if (_settingsBtn) _settingsBtn.onClick.AddListener(OpenSettings);
        if (_backBtn) _backBtn.onClick.AddListener(OpenMenu);
        if (_exitBtn) _exitBtn.onClick.AddListener(ExitGame);

        if (_audioSlider) _audioSlider.onValueChanged.AddListener(SetVolume);

        if (_fpsSlider)
        {
            _fpsSlider.minValue = 0;
            _fpsSlider.maxValue = 2;
            _fpsSlider.wholeNumbers = true;
            _fpsSlider.onValueChanged.AddListener(SetFPS);
        }

        if (_shadowToggle) _shadowToggle.onValueChanged.AddListener(SetShadows);
        if (_vibrationToggle) _vibrationToggle.onValueChanged.AddListener(SetVibration);
        if (_showFpsToggle) _showFpsToggle.onValueChanged.AddListener(SetShowFpsCounter);

        if (_langDropdown) _langDropdown.onValueChanged.AddListener(SetLanguage);
        if (_graphicsDropdown) _graphicsDropdown.onValueChanged.AddListener(SetGraphicsQuality);
    }

    private void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("AudioVolume", value);
    }

    private void SetFPS(float step)
    {
        int fps = step == 0 ? 30 : (step == 1 ? 60 : 120);
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt("FPSStep", (int)step);
    }

    private void SetShadows(bool isEnabled)
    {
        QualitySettings.shadows = isEnabled ? ShadowQuality.All : ShadowQuality.Disable;
        QualitySettings.shadowDistance = isEnabled ? 50f : 0f;
        PlayerPrefs.SetInt("Shadows", isEnabled ? 1 : 0);
    }

    private void SetVibration(bool isEnabled)
    {
        PlayerPrefs.SetInt("Vibration", isEnabled ? 1 : 0);
        if (isEnabled && SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
    }

    private void SetShowFpsCounter(bool isEnabled)
    {
        PlayerPrefs.SetInt("ShowFPSCounter", isEnabled ? 1 : 0);
        if (_fpsCounterText != null)
        {
            _fpsCounterText.gameObject.SetActive(isEnabled);
        }
    }

    private void SetGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("GraphicsQuality", index);
    }

    private void SetLanguage(int index)
    {
        PlayerPrefs.SetInt("Language", index);
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    private void LoadSavedSettings()
    {
        if (_audioSlider)
        {
            float vol = PlayerPrefs.GetFloat("AudioVolume", 1f);
            _audioSlider.value = vol;
            AudioListener.volume = vol;
        }

        if (_fpsSlider)
        {
            int step = PlayerPrefs.GetInt("FPSStep", 1);
            _fpsSlider.value = step;
            SetFPS(step);
        }

        if (_shadowToggle)
        {
            bool shadows = PlayerPrefs.GetInt("Shadows", 1) == 1;
            _shadowToggle.isOn = shadows;
            SetShadows(shadows);
        }

        if (_vibrationToggle)
        {
            _vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
        }

        if (_showFpsToggle)
        {
            bool showFps = PlayerPrefs.GetInt("ShowFPSCounter", 0) == 1;
            _showFpsToggle.isOn = showFps;
            SetShowFpsCounter(showFps);
        }

        if (_graphicsDropdown)
        {
            int defaultQuality = QualitySettings.GetQualityLevel();
            int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", defaultQuality);
            _graphicsDropdown.value = savedQuality;
            SetGraphicsQuality(savedQuality);
        }

        if (_langDropdown)
        {
            _langDropdown.value = PlayerPrefs.GetInt("Language", 0);
        }
    }

   private void UpdateFpsCounter()
    {
        if (_fpsCounterText == null || !_fpsCounterText.gameObject.activeInHierarchy) return;

        _fpsDeltaTime += (Time.unscaledDeltaTime - _fpsDeltaTime) * 0.1f;
        float fps = 1.0f / _fpsDeltaTime;
        _fpsCounterText.text = $"{Mathf.RoundToInt(fps)} FPS";
    }

    private IEnumerator AnimatePopIn(Transform panelTransform)
    {
        panelTransform.localScale = Vector3.zero;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            float scale = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.05f;
            if (t >= 0.9f) scale = Mathf.Lerp(scale, 1f, (t - 0.9f) * 10f);

            panelTransform.localScale = Vector3.one * scale;
            yield return null;
        }

        panelTransform.localScale = Vector3.one;
    }
}