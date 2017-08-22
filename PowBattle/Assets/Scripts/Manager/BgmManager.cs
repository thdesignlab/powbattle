using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BgmManager : SingletonMonoBehaviour<BgmManager>
{
    [SerializeField]
    public float volume = 0.5f;
    private AudioSource audioSource;

    //BGM
    private BgmSettingManager nowBgm;
    private List<BgmSettingManager> bgmSettingMgrList = new List<BgmSettingManager>();

    IEnumerator Start()
    {
        for (;;)
        {
            if (AppManager.Instance.isReadyGame) break;
            yield return null;
        }

        //AudioSource設定
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = Common.Var.audioMixer.FindMatchingGroups(Common.CO.AUDIO_MIXER_BGM)[0];
        audioSource.spatialBlend = 0.0f;
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = volume;

        //BGMリスト取得
        bgmSettingMgrList = new List<BgmSettingManager>(transform.GetComponentsInChildren<BgmSettingManager>());

        //シーン遷移イベント
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
    {
        //BGM再生
        PlayBgm();
    }
    
    //BGM再生
    public void PlayBgm(string bgmName)
    {
        if (bgmName == "") return;
        BgmSettingManager bgmSettingMgr = GetBgmSettingManager(bgmName);
        Play(bgmSettingMgr);
    }
    public void PlayBgm()
    {
        BgmSettingManager bgmSettingMgr = GetSceneBgm();
        Play(bgmSettingMgr);
    }

    //BGM再生処理
    private void Play(BgmSettingManager bgm)
    {
        if (audioSource == null || nowBgm == bgm) return;

        if (bgm == null)
        {
            audioSource.Stop();
            return;
        }
        nowBgm = bgm;
        audioSource.clip = bgm.clip;
        audioSource.Play();
    }

    //BGM停止
    public void StopBgm(string nextScene = "")
    {
        if (audioSource == null || nowBgm == null) return;

        if (nextScene != "")
        {
            BgmSettingManager bgm = GetSceneBgm(nextScene);
            if (nowBgm == bgm) return;
        }
        nowBgm = null;
        audioSource.Stop();
    }

    //BGM設定取得
    private BgmSettingManager GetBgmSettingManager(string bgmName)
    {
        BgmSettingManager bgm = null;
        foreach (BgmSettingManager bgmSettingMgr in bgmSettingMgrList)
        {
            if (bgmSettingMgr.clip == null || bgmSettingMgr.clip.name != bgmName) continue;
            bgm = bgmSettingMgr;
            break;
        }
        return bgm;
    }

    //シーンBGM取得
    public BgmSettingManager GetSceneBgm(string sceneName = "")
    {
        if (sceneName == "") sceneName = SceneManager.GetActiveScene().name;
        if (!Common.CO.sceneBgmDic.ContainsKey(sceneName)) return null;
        return GetBgmSettingManager(Common.CO.sceneBgmDic[sceneName]);
    }

    void Update()
    {
        // 再生中のBGMの再生時間を監視する
        if (audioSource == null || nowBgm == null || !audioSource.isPlaying || nowBgm.loopEndTime <= 0) return;
        if (audioSource.time >= nowBgm.loopEndTime) audioSource.time = nowBgm.loopStartTime;
    }
}
