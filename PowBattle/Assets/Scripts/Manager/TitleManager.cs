using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;


public class TitleManager : MonoBehaviour
{
    private bool isSplashFinished = false;


    [SerializeField]
    private Transform titleCanvas;



    private float processTime = 0;


    private string moveScene = "";


    protected void Awake()
    {
        //フレームレート
        Application.targetFrameRate = 30;
    }

    IEnumerator Start()
    {
        if (isSplashFinished) yield break;

#if UNITY_EDITOR
#elif UNITY_IOS || UNITY_ANDROID
        //スプラッシュ終了待ち
        for (;;)
        {
            Debug.Log("Splash >> "+UnityEngine.Rendering.SplashScreen.isFinished);
            if (!UnityEngine.Rendering.SplashScreen.isFinished) break;
            yield return null;
        }
#else
#endif
        yield return new WaitForSeconds(1.0f);
        ScreenManager.Instance.SceneLoad(Common.CO.SCENE_BATTLE);

        isSplashFinished = true;

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
                    float alpha = Common.Func.GetSin(processTime, 270, 45);
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
            if (Input.GetMouseButtonDown(0))
            {
                messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1);
                ScreenManager.Instance.SceneLoad(Common.CO.SCENE_BATTLE);
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

    //void Update()
    //{
    //    if (!isSplashFinished) return;
    //    Debug.Log("update");

    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        ScreenManager.Instance.SceneLoad(Common.CO.SCENE_BATTLE);
    //    }

    //}


    // ##### モードセレクト #####


}
