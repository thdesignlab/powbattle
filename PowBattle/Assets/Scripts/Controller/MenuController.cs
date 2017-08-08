using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class MenuController : SingletonMonoBehaviour<MenuController>
{
    private bool isEnabledDebug = false;
    private Transform _myTran;
    private Transform myTran
    {
        get { return _myTran ? _myTran : _myTran = transform; }
    }
    private GameObject _battleCanvas;
    private GameObject battleCanvas
    {
        get { return _battleCanvas ? _battleCanvas : _battleCanvas = BattleManager.Instance.GetBattleCanvas(); }
    }

    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();
    }

    void Start()
    {
        //デバッグボタンON/OFF
        if (MyDebug.Instance.isDebugMode || UserManager.isAdmin) isEnabledDebug = true;
        //if (debugButton != null) debugButton.SetActive(isEnabledDebug);
    }

    //MENUボタン押下
    public void OnClickMenuButton()
    {
        BattleManager.Instance.Pause();
    }

    //##### 通常メニュー #####

    //アプリ終了
    public void OnClickExitButton()
    {
        //DialogController.OpenDialog("アプリを終了します", () => GameController.Instance.Exit(), true);
    }

    //タイトルへ戻る
    public void OnClickTitleButton()
    {
        //UnityAction callback = () =>
        //{
        //    GameController.Instance.GoToTitle();
        //};

        //DialogController.OpenDialog("タイトルに戻ります", callback, true);
    }


    //##### カメラメニュー #####

    [SerializeField]
    private GameObject camMenu;
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


    //
    public void OpenCamMenu()
    {
        camMenu.SetActive(true);
    }

    //
    public void CloseCamMenu()
    {
        camMenu.SetActive(false);
    }

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
        if (!isEnabledDebug) return;
    }

}
