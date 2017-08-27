using UnityEngine;
using System.Collections;

public class AppManager : SingletonMonoBehaviour<AppManager>
{
    [HideInInspector]
    public bool isReadyGame = false;
    [HideInInspector]
    public bool isFinishedSplash = false;

    [SerializeField]
    private GameObject touchCousor;

    IEnumerator Start()
    {
        for (;;)
        {
            if (isReadyGame) break;
            yield return null;
        }

        //ステータスバー
        Common.Func.SetStatusbar();

        //タッチ可視化
        if (touchCousor != null && DebugController.Instance.isDebugMode) DontDestroyOnLoad(touchCousor);
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

    //タイトル画面へ
    public void GoToTitle()
    {
        ScreenManager.Instance.SceneLoad(Common.CO.SCENE_TITLE);
    }

    //ホーム画面へ
    public void GoToHome()
    {
        ScreenManager.Instance.SceneLoad(Common.CO.SCENE_MAIN);
    }
}
