using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;

public class AppManager : SingletonMonoBehaviour<AppManager>
{
    public static int gameMode = 0;

    [SerializeField]
    private GameObject touchCousor;

    protected override void Awake()
    {
        base.Awake();

        //ステータスバー
        Common.Func.SetStatusbar();

        //タッチ可視化
        if (touchCousor != null) DontDestroyOnLoad(touchCousor);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            //ステータスバー
            Common.Func.SetStatusbar();
        }
    }

    // ##### 各操作 #####

    //ゲーム終了
    public void ExitGame()
    {
        Application.Quit();
    }
}
