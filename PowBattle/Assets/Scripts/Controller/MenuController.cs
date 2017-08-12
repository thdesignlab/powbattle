using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MenuController : SingletonMonoBehaviour<MenuController>
{
    [SerializeField]
    private GameObject menuList;
    [SerializeField]
    private GameObject debugMenuList;
    [SerializeField]
    private GameObject debugBtn;
    [SerializeField]
    private GameObject camMenu;
    //private Transform _myTran;
    //private Transform myTran
    //{
    //    get { return _myTran ? _myTran : _myTran = transform; }
    //}
    //private GameObject _battleCanvas;
    //private GameObject battleCanvas
    //{
    //    get { return _battleCanvas ? _battleCanvas : _battleCanvas = BattleManager.Instance.GetBattleCanvas(); }
    //}

    private bool isAdmin = false;

    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();
    }

    void Start()
    {
        //デバッグボタンON/OFF
        if (MyDebug.Instance.isDebugMode || UserManager.isAdmin) isAdmin = true;

        //初期表示
        menuList.SetActive(false);
        debugMenuList.SetActive(false);
        debugBtn.SetActive(isAdmin);
        camMenu.SetActive(false);
    }

    //MENUボタン押下
    public void OnClickMenuButton()
    {
        if (menuList.activeSelf)
        {
            //非表示
            CloseMenu();
        }
        else
        {
            //表示
            BattleManager.Instance.Pause();
            menuList.SetActive(true);
        }
    }

    private void CloseMenu()
    {
        menuList.SetActive(false);
        debugMenuList.SetActive(false);
        BattleManager.Instance.ResetPause();
    }

    //##### 通常メニュー #####

    //アプリ終了
    public void OnClickExitButton()
    {
        //DialogController.OpenDialog("アプリを終了します", () => GameController.Instance.Exit(), true);
        AppManager.Instance.ExitGame();
    }

    //タイトルへ戻る
    public void OnClickTitleButton()
    {
        //UnityAction callback = () =>
        //{
        //    GameController.Instance.GoToTitle();
        //};

        //DialogController.OpenDialog("タイトルに戻ります", callback, true);
        AppManager.Instance.GoToTitle();
    }


    //##### カメラメニュー #####

    private GameObject _player;
    private GameObject player
    {
        get { return _player ? _player : _player = BattleManager.Instance.GetPlayer(); }
    }
    private PlayerController _playerCtrl;
    private PlayerController playerCtrl
    {
        get { return _playerCtrl ? _playerCtrl : _playerCtrl = player.GetComponent<PlayerController>(); }
    }

    //カメラメニュー表示
    public void OpenCamMenu()
    {
        camMenu.SetActive(true);
    }

    //カメラメニュー非表示
    public void CloseCamMenu()
    {
        camMenu.SetActive(false);
    }

    //カメラモード切替
    public void ChangeCamFree()
    {
        playerCtrl.SwitchCameraMode(Common.CO.CAM_MODE_FREE);
    }
    public void ChangeCamFirst()
    {
        playerCtrl.SwitchCameraMode(Common.CO.CAM_MODE_FIRST);
    }
    public void ChangeCamThird()
    {
        playerCtrl.SwitchCameraMode(Common.CO.CAM_MODE_THIRD);
    }
    public void ChangeCamVr()
    {

    }

    //##### デバッグメニュー(管理者のみ実行可能) #####

    //デバッグメニュー表示切り替え
    public void OnClickDebugButton(bool flg)
    {
        if (!isAdmin) return;
        debugMenuList.SetActive(true);
    }

    //リスタート
    public void OnClickBattleRestart()
    {
        if (!isAdmin) return;
        ScreenManager.Instance.SceneLoad(SceneManager.GetActiveScene().name);
        CloseMenu();
    }
}
