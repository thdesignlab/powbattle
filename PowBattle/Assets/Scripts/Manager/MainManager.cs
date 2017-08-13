using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;

public class MainManager : SingletonMonoBehaviour<MainManager>
{

    //ストーリーボタン押下
    public void ObClickStoryBtn()
    {
        ScreenManager.Instance.SceneLoad(Common.CO.SCENE_STORY);
    }

    public void ObClickBattleBtn()
    {
        ScreenManager.Instance.SceneLoad(Common.CO.SCENE_BATTLE);
    }
}
