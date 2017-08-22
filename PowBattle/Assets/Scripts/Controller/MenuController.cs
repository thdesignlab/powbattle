using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuController : SingletonMonoBehaviour<MenuController>
{
    private Transform battleCanvasTran;
    private GameObject menuList;
    private GameObject camMenu;
    private Slider hpSlider;
    
    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();
    }

    void Start()
    {
        //UI取得
        Transform battleCanvasTran = transform;
        menuList = battleCanvasTran.Find("Menu/MenuList").gameObject;
        camMenu = battleCanvasTran.Find("Menu/CamMenu").gameObject;
        hpSlider = camMenu.transform.Find("HP").GetComponent<Slider>();

        //初期表示
        menuList.SetActive(false);
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
        BattleManager.Instance.ResetPause();
    }

    //##### 通常メニュー #####

    //アプリ終了
    public void OnClickExitButton()
    {
        UnityAction callback = () =>
        {
            BattleManager.Instance.ResetPause();
            AppManager.Instance.ExitGame();
        };
        DialogManager.OpenDialog("アプリを終了します", callback, null);
    }

    //タイトルへ戻る
    public void OnClickTitleButton()
    {
        UnityAction callback = () =>
        {
            BattleManager.Instance.ResetPause();
            AppManager.Instance.GoToTitle();
        };
        DialogManager.OpenDialog("タイトルへ戻ります", callback, null);
    }

    //ホームへ戻る
    public void OnClickHomeButton()
    {
        UnityAction callback = () =>
        {
            BattleManager.Instance.ResetPause();
            AppManager.Instance.GoToHome();
        };
        DialogManager.OpenDialog("ホームへ戻ります", callback, null);
    }


    //##### カメラメニュー #####

    private GameObject _player;
    private GameObject player
    {
        get { return _player ? _player : _player = BattleManager.Instance.GetPlayer(); }
    }
    private BattlePlayerController _playerCtrl;
    private BattlePlayerController playerCtrl
    {
        get { return _playerCtrl ? _playerCtrl : _playerCtrl = player.GetComponent<BattlePlayerController>(); }
    }

    //カメラメニュー表示
    private Coroutine camTargetHpCoroutine;
    public void OpenCamMenu(Transform tran)
    {
        camMenu.SetActive(true);
        camTargetHpCoroutine = StartCoroutine(CamTargetHpCoroutine(tran));
    }

    //カメラメニュー非表示
    public void CloseCamMenu()
    {
        camMenu.SetActive(false);
        StopCoroutine(camTargetHpCoroutine);
    }

    IEnumerator CamTargetHpCoroutine(Transform tran)
    {
        UnitController unitCtrl = tran.GetComponent<UnitController>();
        for (;;)
        {
            if (unitCtrl == null) break;
            hpSlider.value = unitCtrl.GetHpRate();
            yield return null;
        }
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
}
