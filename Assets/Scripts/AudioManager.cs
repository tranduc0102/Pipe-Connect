using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource _sfx;
    public AudioSource _music;
    public AudioClip _up;
    public AudioClip _down;
    public AudioClip _spawn;
    public AudioClip _done;
    public AudioClip _win;
    public AudioClip _lose;

    private void Awake()
    {
        Instance = this;
        _sfx.volume = PlayerPrefs.GetFloat("VolumnSFX", 1);
        _music.volume = PlayerPrefs.GetFloat("VolumnSFX", 1);
    }

    public void PlaySpawn()
    {
        if (PlayerPrefs.GetFloat("VolumnSFX", 1) == 0) return;
        _sfx.DOKill(); 
        _sfx.volume = 0f;
        _sfx.pitch = Random.Range(0.95f, 1.05f);
        _sfx.PlayOneShot(_spawn);

        DOTween.To(() => _sfx.volume, x => _sfx.volume = x, PlayerPrefs.GetFloat("VolumnSFX", 1), 0.1f);
    }

    public void PlayUp()
    {
        _sfx.pitch = 1f;
        _sfx.PlayOneShot(_up);
    }

    public void PlayDown()
    {
        _sfx.pitch = 1f;
        _sfx.PlayOneShot(_down);
    }
    public void PlayDone()
    {
        _sfx.pitch = 1f;
        _sfx.PlayOneShot(_done);
    }
    public void PlayWin()
    {
        _sfx.pitch = 1f;
        _sfx.PlayOneShot(_win);
    }
    public void PlayLose()
    {
        _sfx.pitch = 1f;
        _sfx.PlayOneShot(_lose);
    }

    public void SetValue(float value)
    {
        _sfx.volume = value;
        _music.volume = value;
        PlayerPrefs.SetFloat("VolumnSFX", value);
    }
}
