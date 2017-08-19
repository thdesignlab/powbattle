using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SystemSeManager : SingletonMonoBehaviour<SystemSeManager>
{
    [SerializeField]
    public float volume = 0.5f;
    private AudioSource audioSource;

    //タグ
    const string BUTTON_TAG_POSITIVE = "PositiveButton";
    const string BUTTON_TAG_NEGATIVE = "NegativeButton";
    const string BUTTON_TAG_SELECT = "SelectButton";

    //SE
    const string BUTTON_SE_POSITIVE = "Positive";
    const string BUTTON_SE_NEGATIVE = "Negative";
    const string BUTTON_SE_SELECT = "Select";


    protected override void Awake()
    {
        base.Awake();

        //AudioSource設定
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = Common.Var.audioMixer.FindMatchingGroups(Common.CO.AUDIO_MIXER_SE)[0];
        audioSource.spatialBlend = 0.0f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
    {
        SetSceneButtonSe();
    }

    //シーン全体のボタンにSEを付与
    public void SetSceneButtonSe()
    {
        foreach (Button btn in Resources.FindObjectsOfTypeAll<Button>())
        {
            UnityAction action = null;
            switch (btn.tag)
            {
                case BUTTON_TAG_SELECT:
                    action = PlaySelectSe;
                    break;

                case BUTTON_TAG_POSITIVE:
                    action = () => PlayYesNoSe(true);
                    break;

                case BUTTON_TAG_NEGATIVE:
                    action = () => PlayYesNoSe(false);
                    break;
            }

            if (action == null) continue;
            btn.onClick.AddListener(action);
        }
    }

    //再生
    private void Play(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.clip = clip;
        audioSource.Play();
    }
    private void Play(string clipName)
    {
        AudioClip clip = Common.Func.GetAudioResource(clipName);
        Play(clip);
    }

    //ボタンSE(Yes,No)
    public void PlayYesNoSe(bool isPositive = true)
    {
        string clipName = (isPositive) ? BUTTON_SE_POSITIVE : BUTTON_SE_NEGATIVE;
        Play(clipName);
    }

    //ボタンSE(選択)
    public void PlaySelectSe()
    {
        Play(BUTTON_SE_SELECT);
    }
}
