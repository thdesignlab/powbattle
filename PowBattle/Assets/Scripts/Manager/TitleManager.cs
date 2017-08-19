using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;


public class TitleManager : MonoBehaviour
{
    private bool isGameStart = false;


    //[SerializeField]
    private Transform titleCanvas;

    private float processTime = 0;

    IEnumerator Start()
    {

#if UNITY_EDITOR
#elif UNITY_IOS || UNITY_ANDROID
        //スプラッシュ終了待ち
        for (;;)
        {
            if (UnityEngine.Rendering.SplashScreen.isFinished) break;
            yield return null;
        }
#else
#endif
        AppManager.Instance.isSplashFinished = true;
        //フレームレート
        Application.targetFrameRate = 30;
        //BGM
        GetComponent<AudioSource>().volume = BgmManager.Instance.volume;
        //UI
        titleCanvas = GameObject.Find("TitleCanvas").transform;

        //TapToStart点灯
        Text messageText = null;
        for (;;)
        {
            if (messageText != null)
            {
                processTime += Time.deltaTime;

                if (processTime > 1.0f)
                {
                    //一定時間ごとに点滅
                    float alpha = Common.Func.GetSinCycle(processTime, 270, 45);
                    messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, alpha);
                    //messageImage.color = new Color(messageImage.color.r, messageImage.color.g, messageImage.color.b, alpha);
                }
            }
            else
            {
                //message = DialogController.OpenMessage(DialogController.MESSAGE_TOP, DialogController.MESSAGE_POSITION_CENTER);
                //messageImage = DialogController.GetMessageImageObj();
                messageText = titleCanvas.Find("StartLabel").GetComponent<Text>();
            }

            //タップ判定
            //if (Input.GetMouseButtonDown(0))
            if (isGameStart)
            {
                messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1);
                //ScreenManager.Instance.SceneLoad(Common.CO.SCENE_BATTLE);
                ScreenManager.Instance.SceneLoad(Common.CO.SCENE_MAIN);
                yield break;
            }
            yield return null;
        }
        //messageImage.color = new Color(messageImage.color.r, messageImage.color.g, messageImage.color.b, 1);

        //初期設定読み込み
        //DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        //InitApi();
        //for (;;)
        //{
        //    if (isReadyGame)
        //    {
        //        TapToStart();
        //        break;
        //    }
        //    yield return null;
        //}

    }

    public void StartGame()
    {
        if (!AppManager.Instance.isSplashFinished) return;
        isGameStart = true;
    }

    // ##### モードセレクト #####


}
