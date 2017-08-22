using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [HideInInspector]
    public bool isReadyGame = false;
    private bool isFinishedSplash = false;
    private bool isGameStart = false;

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

        isFinishedSplash = true;

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
                messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 0);
                //ScreenManager.Instance.SceneLoad(Common.CO.SCENE_BATTLE);
                break;
            }
            yield return null;
        }


        //APIロード
        yield return null;

        //アセットロード
        yield return StartCoroutine(LoadAssetManager.Instance.LoadAssets());

        AppManager.Instance.isReadyGame = true;

        ScreenManager.Instance.SceneLoad(Common.CO.SCENE_MAIN);

    }

    public void StartGame()
    {
        if (!isFinishedSplash) return;
        isGameStart = true;
    }
}
