using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [SerializeField] private float time = 3f;
    [SerializeField] private TextMeshProUGUI textLoading;
    [SerializeField] private Image loadingBar;
    private void Start()
    {
        loadingBar.DOFillAmount(1f, time).OnComplete(() =>
        {
            SceneManager.LoadSceneAsync("Gameplay");
        })
        ;
        string[] texts = { "Loading.", "Loading..", "Loading..." };
        var seq = DOTween.Sequence();
        foreach (var t in texts)
        {
            seq.AppendCallback(() => textLoading.text = t);
            seq.AppendInterval(0.5f);
        }
        seq.SetLoops(-1);

    }

    [ContextMenu("Clear Data")]
    public void ClearData()
    {
        PlayerPrefs.DeleteAll();
    }    
}
