using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;


public class AppManager : SingletonMonoBehaviour<AppManager>
{
    public static int gameMode = 0;

    protected override void Awake()
    {
        base.Awake();
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
