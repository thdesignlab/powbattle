using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    //private Transform myTran;
    private AudioSource[] _audioSources;
    private AudioSource[] audioSources { get { return _audioSources != null ? _audioSources : _audioSources = transform.GetComponentsInChildren<AudioSource>(); } }

    //void Start()
    //{
    //    myTran = transform;
    //    audioSources = myTran.GetComponentsInChildren<AudioSource>();
    //    foreach (AudioSource audioSource in audioSources)
    //    {
    //        Debug.Log(name+" >> "+ audioSource.clip);
    //    }
    //}

    public void Play(int no = 0, bool isSendRPC = true)
    {
        if (!IsExistsAudio(no)) return;
        audioSources[no].Play();
    }

    public void Stop(int no = 0, bool isSendRPC = true)
    {
        if (!IsExistsAudio(no)) return;
        audioSources[no].Stop();
    }

    private bool IsExistsAudio(int no)
    {
        if (audioSources != null && (0 < audioSources.Length && no + 1 <= audioSources.Length)) return true;
        return false;
    }
}
