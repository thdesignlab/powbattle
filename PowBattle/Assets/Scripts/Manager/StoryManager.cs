using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;

public class StoryManager : SingletonMonoBehaviour<StoryManager>
{
    protected Transform player;
    protected StoryPlayerController playerCtrl;
    protected Transform camTran;
    protected int selectedStory;
    protected int selectedStage;

    const string STORY_NAME = "Story";
    const string STAGE_NAME = "Stage";

    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();

        Init();
    }

    protected void Init()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCtrl = player.GetComponent<StoryPlayerController>();
        camTran = Camera.main.transform;
        selectedStory = 1;
        selectedStage = 1;

        Transform t = GetStageTran(1);
        playerCtrl.SetTargetCenter(t);
    }

    //ステージ選択
    public void SelectStage(Transform stageTran)
    {
        int no = GetStageNo(stageTran.name);
        if (no <= 0) return;
        SelectStage(no, stageTran);
    }
    public void SelectStage(int no, Transform stageTran = null)
    {
        //★選択可能かチェック



        selectedStage = no;

        //確認ダイアログ
        string message = "バトルスタート";
        UnityAction ok = () => StartBattle();
        //DialogManager.OpenDialog(message, ok, null);
        DialogManager.SetDialogLowPosition(100);

        //カメラ位置
        SetCameraPosition(selectedStage, () => DialogManager.OpenDialog(message, ok, null));
    }

    //カメラ位置移動
    public void SetCameraPosition(int stageNo = -1, UnityAction callback = null)
    {
        Transform target = GetStageTran(stageNo);
        SetCameraPosition(target, callback);
    }
    public void SetCameraPosition(Transform target, UnityAction callback = null)
    {
        playerCtrl.SetTargetCenter(target, callback);
    }

    //ストーリー選択
    public void SelectStory(int no)
    {
        selectedStory = no;
    }

    //バトルスタート
    public void StartBattle()
    {
        ScreenManager.Instance.SceneLoad(Common.CO.SCENE_BATTLE);
    }

    //ステージNo取得
    private int GetStageNo(string name)
    {
        int no = -1;
        Regex r = new Regex(STAGE_NAME+"([0-9]*)", RegexOptions.IgnoreCase);
        Match m = r.Match(name);
        if (m.Groups.Count >= 2) no = int.Parse(m.Groups[1].ToString());
        return no;
    }

    //ステージ名取得
    private Transform GetStageTran(int no)
    {
        if (no <= 0) no = selectedStage;
        GameObject stageObj = GameObject.Find(STAGE_NAME + no.ToString());
        return (stageObj == null) ? null : stageObj.transform;
    }
}
