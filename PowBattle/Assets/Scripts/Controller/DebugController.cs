using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DebugController : SingletonMonoBehaviour<DebugController>
{
    public bool isDebugMode;
    [SerializeField]
    public GameObject debugCanvas;

    [HideInInspector]
    public bool isEnableDebugMenu;

    //UI
    private GameObject btnBattleRestart;
    private GameObject btnDrawTarget;

    //デバッグメニュー
    private float debugBtnSize = Screen.width / 6;
    private float btnDownTime = 0;
    private float btnReleaseTime = 0;
    private const float NEED_BUTTON_DOWN_TIME = 1;

    //ログ設定
    private Queue logQueue = new Queue();
    private int logCount = 300;
    private string preCondition = "";
    private float preLogTime = 0;

    //FPS
    private Queue frameTime = new Queue();

    //FLG
    private bool isOpenMenu = false;
    private bool isOpenLog = false;
    private bool isDispFps = false;
    
    protected override void Awake()
    {
        base.Awake();
        if (debugCanvas)
        {
            DontDestroyOnLoad(debugCanvas);
            Transform debugMenuTran = debugCanvas.transform.Find("DebugMenuList");
            btnBattleRestart = debugMenuTran.Find("BattleRestart").gameObject;
            btnDrawTarget = debugMenuTran.Find("BattleDrawTarget").gameObject;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }
    }

    void OnEnable()
    {
        SetDebugMenu(false);
        isEnableDebugMenu = (UserManager.isAdmin || isDebugMode);
        if (!isEnableDebugMenu) return;
        SwitchMenu();
        Application.logMessageReceived += HandleLog;
    }
    void OnDisable()
    {
        SetDebugMenu(false);
        Application.logMessageReceived -= HandleLog;
    }

    //デバッグメニュー表示
    public void SetDebugMenu(bool flg)
    {
        isOpenMenu = flg;
        if (debugCanvas != null)
        {
            debugCanvas.SetActive(flg);
            isOpenLog = false;
        }
        else
        {
            isOpenLog = isOpenMenu;
        }
    }


    //ログ出力
    public void AdminLog(object key, object value)
    {
        AdminLog(key + " >> " + value);
    }
    public void AdminLog(object log)
    {
        if (!isEnableDebugMenu) return;
        Debug.Log(log);
    }
    
    //ログ格納
    void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (condition == preCondition)
        {
            //同じメッセージは続けて出さない
            if (Time.time - preLogTime < 1.0f) return;
        }
        preCondition = condition;
        preLogTime = Time.time;
        //stackTrace += "\n"+UnityEngine.StackTraceUtility.ExtractStackTrace();
        // 必要な変数を宣言する
        //string dtNow = System.DateTime.Now.ToString("yyyy/MM/dd (ddd) HH:mm:ss");
        string dtNow = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        string trace = stackTrace.Remove(0, (stackTrace.IndexOf("\n") + 1));
        string log = "### START ### -- "+ type.ToString() + " -- " + dtNow + "\n【condition】" + condition + "\n【stackTrace】" + trace + "\n### END ###\n";
        //string log = "### START ### -- " + dtNow + "\n" + stackTrace + "\ntype : " + type.ToString() + "\n### END ###\n";
        PushLog(log, false);
    }
    private void PushLog(string str, bool console = true)
    {
        if (logQueue.Count >= logCount) logQueue.Dequeue();

        logQueue.Enqueue(str);
        if (console) Debug.Log(str);
    }

    void OnGUI()
    {
        if (!isEnableDebugMenu) return;

        ////GUISkin設定
        //GUI.skin = (guiSkin != null) ? guiSkin : Instantiate(GUI.skin);

        //デバッグボタン
        Rect debugBtnRect = new Rect(0, 0, debugBtnSize, debugBtnSize);

        //FPS
        if (isDispFps)
        {
            float fps = GetFps(Time.deltaTime);
            float w = Screen.width / 2.0f;
            float h = debugBtnSize;
            GUI.skin.label.fontSize = 20;
            GUI.Label(new Rect(0, h, w, h), "FPS:" + fps);
        }

        //デバッグメニュー表示ボタン
        if (!isOpenMenu)
        {
            GUI.skin.button.normal.background = null;
            GUI.skin.button.hover.background = null;
            GUI.skin.button.active.background = null;
            if (GUI.RepeatButton(debugBtnRect, ""))
            {
                //デバッグメニューボタン押下中
                btnDownTime += Time.deltaTime;
                btnReleaseTime = 0;
                if (btnDownTime >= NEED_BUTTON_DOWN_TIME)
                {
                    SetDebugMenu(true);
                }
            }
            else
            {
                btnReleaseTime += Time.deltaTime;
                if (btnReleaseTime >= 1.0f) btnDownTime = 0;
            }
            return;
        }
        else
        {
            //デバッグメニュー非表示ボタン
            if (GUI.Button(debugBtnRect, "-"))
            {
                SetDebugMenu(false);
                btnDownTime = 0;
                return;
            }

            //ログ
            if (isOpenLog)
            {
                Rect logRect = new Rect(0, debugBtnSize, Screen.width, Screen.height - debugBtnSize * 2);
                string logText = "";
                foreach (string log in logQueue)
                {
                    logText = log + logText;
                }
                GUI.TextArea(logRect, logText);
                return;
            }
        }
    }

    void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
    {
        SwitchMenu(nextScene.name);
    }

    //ボタン表示切替
    private void SwitchMenu(string sceneName = "")
    {
        bool isBattle = Common.Func.IsBattleScene(sceneName);
        btnBattleRestart.SetActive(isBattle);
        btnDrawTarget.SetActive(isBattle);
    }

    //### 共通 ###

    //ログ表示
    public void OnClickOpenLog()
    {
        if (!isEnableDebugMenu) return;
        if (debugCanvas != null) debugCanvas.SetActive(false);
        isOpenLog = true;
    }

    //FPS表示
    public void OnClickOpenFps()
    {
        if (!isEnableDebugMenu) return;
        if (debugCanvas != null) debugCanvas.SetActive(false);
        frameTime = new Queue();
        isDispFps = !isDispFps;
        isOpenMenu = false;
    }
    private float GetFps(float deltaTime)
    {
        if (frameTime.Count >= 60) frameTime.Dequeue();
        frameTime.Enqueue(deltaTime);

        float sumTime = 0;
        foreach (float time in frameTime)
        {
            sumTime += time;
        }
        return ((int)(frameTime.Count * 10 / sumTime)) / 10.0f;
    }

    //### バトル ###

    //リスタート
    public void OnClickBattleRestart()
    {
        if (!isEnableDebugMenu) return;
        if (!Common.Func.IsBattleScene()) return;
        ScreenManager.Instance.SceneLoad(SceneManager.GetActiveScene().name);
        SetDebugMenu(false);
    }

    //ターゲット可視化
    public void OnClickDrawTarget()
    {
        if (!isEnableDebugMenu) return;
        if (!Common.Func.IsBattleScene()) return;
        BattleManager.Instance.isVisibleTarget = !BattleManager.Instance.isVisibleTarget;
        SetDebugMenu(false);
    }
}