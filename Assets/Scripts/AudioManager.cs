using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource _sfx;
    public AudioClip _up;
    public AudioClip _down;
    public AudioClip _spawn;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySpawn()
    {
        _sfx.DOKill(); 
        _sfx.volume = 0f;
        _sfx.pitch = Random.Range(0.95f, 1.05f);
        _sfx.PlayOneShot(_spawn);

        DOTween.To(() => _sfx.volume, x => _sfx.volume = x, 1f, 0.1f);
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
}
