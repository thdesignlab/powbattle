using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DebugController : SingletonMonoBehaviour<DebugController>
{
    public bool isDebugMode;
    [SerializeField]
    public GameObject debugMenu;

    [HideInInspector]
    public bool isEnableDebugMenu;

    //UI
    private GameObject btnBattleRestart;
    private GameObject btnDrawTarget;

    //デバッグメニュー
    private float debugBtnSize = Screen.width / 9;
    private float btnDownTime = 0;
    private float btnReleaseTime = 0;
    private const float NEED_BUTTON_DOWN_TIME = 1;

    //ログ設定
    private Queue logQueue = new Queue();
    private int logCount = 300;
    private string preCondition = "";
    private float preLogTime = 0;

    //FLG
    private bool isOpenMenu = false;
    private bool isOpenLog = false;

    protected override void Awake()
    {
        base.Awake();
        if (debugMenu)
        {
            Transform debugMenuTran = debugMenu.transform;
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
        if (debugMenu != null)
        {
            debugMenu.SetActive(flg);
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
        if (debugMenu != null) debugMenu.SetActive(false);
        isOpenLog = true;
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