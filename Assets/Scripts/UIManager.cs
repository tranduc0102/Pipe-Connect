using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _txtLevel;
    [SerializeField] private TextMeshProUGUI _txtTimer;

    [Space]
    [Header("Setting")]
    [SerializeField] private Button _settingBtn;
    [SerializeField] private CanvasGroup _popSetting;
    [SerializeField] private Button _btnCloseSetting;
    [SerializeField] private Slider _valueVolume;

    [Space]
    [Header("Replay")]
    [SerializeField] private Button _replayBtn;
    [SerializeField] private CanvasGroup _popWarning;
    [SerializeField] private Button _btnClosePopWaring;
    [SerializeField] private Button _btnRestartGame;

    [Space]
    [Header("Win")]
    [SerializeField] private CanvasGroup _winpop;
    [SerializeField] private Button _btnNextLevel;

    [Space]
    [Header("Lose")]
    [SerializeField] private CanvasGroup _lose;
    [SerializeField] private Button _btnReplay;


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _btnNextLevel.onClick.AddListener(() =>
        {
            ShowResult(true, false, GameManager.Instance.NextLevel);
        });
        _btnReplay.onClick.AddListener(() =>
        {
            ShowResult(false, false, GameManager.Instance.Replay);
        });
        _settingBtn.onClick.AddListener(() =>
        {
            Time.timeScale = 0;
            ShowPopup(_popSetting, true);
        });
        _btnCloseSetting.onClick.AddListener(() => { Time.timeScale = 1; ShowPopup(_popSetting, false); });

        _replayBtn.onClick.AddListener(() =>
        {
            Time.timeScale = 0; 
            ShowPopup(_popWarning, true);
        });
        _btnClosePopWaring.onClick.AddListener(() =>
        {
            Time.timeScale = 1;
            ShowPopup(_popWarning, false);
        });
        _btnRestartGame.onClick.AddListener(() =>
        {
            Time.timeScale = 1;
            ShowPopup(_popWarning, false);
            ShowResult(false, false, GameManager.Instance.Replay);
        });
        _valueVolume.value = PlayerPrefs.GetFloat("VolumnSFX", 1);
        _valueVolume.onValueChanged.AddListener(value => {
            AudioManager.Instance.SetValue(value);
        });

    }

    public void UpdateTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        _txtTimer.text = $"{minutes:00}:{seconds:00}";
    }

    public void UpdateViewLevel(int level, float time)
    {
        _txtLevel.text = "Level " + level;
        UpdateTime(time);
        _txtLevel.transform.DOScale(Vector3.zero, 0.25f)
                   .SetEase(Ease.OutBack).OnComplete(delegate
                   {
                       _txtLevel.transform.DOScale(Vector3.one, 0.4f)
               .SetEase(Ease.InBack);
                   });
    }
    public void ShowResult(bool result, bool show, Action onFinish = null)
    {
        CanvasGroup target = result ? _winpop : _lose;
        if (result)
        {
        }
        else
        {
            AudioManager.Instance.PlayLose();
        }
         ShowPopup(target, show, onFinish);
    }
    public void ShowPopup(CanvasGroup canvas, bool show, Action onFinish = null)
    {
        float duration = 0.35f;

        canvas.DOKill();

        if (show)
        {
            canvas.gameObject.SetActive(true);
            canvas.blocksRaycasts = true;
            canvas.interactable = true;

            canvas.alpha = 0;
            canvas.transform.localScale = Vector3.one * 0.8f;

            canvas.DOFade(1f, duration).SetEase(Ease.OutQuad).SetUpdate(true);
            canvas.transform.DOScale(1f, duration).SetUpdate(true).SetEase(Ease.OutBack)
                .OnComplete(() => onFinish?.Invoke());
        }
        else
        {
            canvas.blocksRaycasts = false;
            canvas.interactable = false;

            canvas.DOFade(0f, duration).SetEase(Ease.InQuad);
            canvas.transform.DOScale(0.8f, duration).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    canvas.gameObject.SetActive(false);
                    onFinish?.Invoke();
                });
        }
    }
}
