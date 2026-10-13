using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

[Serializable]
public class DialogLine
{
    public string CharacterName;
    public Sprite CharacterPortrait;
    [TextArea(3, 5)]
    public string Text;
}

[Serializable]
public class DialogSequence
{
    public string Title; 
    public List<DialogLine> Lines;
}

public class DialogController : MonoBehaviour
{
    public event Action OnDialogEnded;

    [Header("UI Elements")]
    [SerializeField] private GameObject _dialogPanel;
    [SerializeField] private Image _portraitImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogText;

    [Header("Settings")]
    [SerializeField] private float _typingSpeed = 0.03f;
    [SerializeField] private string _introPlayerPrefsKey = "HasSeenIntro";

    [Header("Intro Dialog Content (Перший запуск)")]
    [SerializeField] private List<DialogLine> _introDialogLines;

    [Header("Station Dialog Content (Різні діалоги станцій)")]
    [Tooltip("Список діалогів для кожної наступної станції.")]
    [SerializeField] private List<DialogSequence> _stationDialogs;

    private JourneyManager _journeyManager;

    private List<DialogLine> _activeDialogLines;
    private int _currentLineIndex = 0;
    private int _currentStationDialogIndex = 0;
    private bool _isTyping = false;
    private string _currentSentence = "";
    private Coroutine _typingCoroutine;

    [Inject]
    public void Construct(JourneyManager journeyManager)
    {
        _journeyManager = journeyManager;
    }

    private void Awake()
    {
        if (_dialogPanel != null)
        {
            _dialogPanel.SetActive(false);
        }
    }

    private void Start()
    {
        CheckAndStartIntroDialog();
    }

    private void OnEnable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached += HandleStationReached;
        }
    }

    private void OnDisable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.OnStationReached -= HandleStationReached;
        }
    }

    private void Update()
    {
        if (_dialogPanel != null && _dialogPanel.activeInHierarchy && Input.GetMouseButtonDown(0))
        {
            if (_isTyping)
            {
                FinishTyping();
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    private void CheckAndStartIntroDialog()
    {
        if (PlayerPrefs.GetInt(_introPlayerPrefsKey, 0) == 0)
        {
            if (_introDialogLines != null && _introDialogLines.Count > 0)
            {
                PlayerPrefs.SetInt(_introPlayerPrefsKey, 1);
                PlayerPrefs.Save();

                StartDialog(_introDialogLines);
            }
        }
    }

    private void HandleStationReached()
    {
        if (_stationDialogs != null && _stationDialogs.Count > 0)
        {
            List<DialogLine> currentStationLines = _stationDialogs[_currentStationDialogIndex % _stationDialogs.Count].Lines;

            _currentStationDialogIndex++;

            StartDialog(currentStationLines);
        }
        else
        {
            OnDialogEnded?.Invoke();
        }
    }

    public void StartDialog(List<DialogLine> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            EndDialog();
            return;
        }

        _activeDialogLines = lines;
        _currentLineIndex = 0;

        _dialogPanel.SetActive(true);
        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (_activeDialogLines == null || _currentLineIndex >= _activeDialogLines.Count)
        {
            EndDialog();
            return;
        }

        DialogLine line = _activeDialogLines[_currentLineIndex];

        if (_nameText != null) _nameText.text = line.CharacterName;
        if (_portraitImage != null && line.CharacterPortrait != null)
        {
            _portraitImage.sprite = line.CharacterPortrait;
        }

        _currentSentence = line.Text;
        _currentLineIndex++;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        _typingCoroutine = StartCoroutine(TypeSentence(_currentSentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;
        _dialogText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            _dialogText.text += letter;
            yield return new WaitForSeconds(_typingSpeed);
        }

        _isTyping = false;
    }

    private void FinishTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _dialogText.text = _currentSentence;
        _isTyping = false;
    }

    private void EndDialog()
    {
        _dialogPanel.SetActive(false);
        _dialogText.text = "";

        OnDialogEnded?.Invoke();
    }

    [ContextMenu("Reset Intro Dialog State")]
    public void ResetIntroState()
    {
        PlayerPrefs.DeleteKey(_introPlayerPrefsKey);
        Debug.Log("[DialogController] 🔄 Стан першого запуску скинуто!");
    }
}