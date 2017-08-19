using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    protected List<AudioClip> clips;
    [SerializeField]
    protected float editVolume;
    [SerializeField]
    protected bool isPlayOnAwake;
    [SerializeField]
    protected bool isLoop;
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        Init();
    }

    protected virtual void Init()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = isLoop;
        audioSource.volume = 0.5f + editVolume;
    }

    protected void OnEnable()
    {
        if (isPlayOnAwake) Play();
    }

    public virtual void Play(int no = 0, bool isNoRepeat = false)
    {
        AudioClip playClip = GetAudioClip(no, isNoRepeat);
        if (playClip == null) return;
        audioSource.clip = playClip;
        audioSource.Play();
    }

    public virtual void Stop()
    {
        if (audioSource == null) return;
        audioSource.Stop();
    }

    protected AudioClip GetAudioClip(int no = 0, bool isNoRepeat = false)
    {
        if (audioSource == null || clips.Count <= 0) return null;
        if (isNoRepeat)
        {
            no = no % clips.Count;
        }
        else
        {
            if (clips.Count <= no) return null;
        }
        return clips[no];
    }
}
