﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class DialogManager : MonoBehaviour
{
    //ダイアログ
    private static GameObject dialog;
    private static Transform dialogTran;
    private static Transform btnAreaTran;

    //リソース
    const string RESOURCE_DIALOG = "DialogCanvas";
    const string RESOURCE_POSITIVE_BUTTON = "PositiveButton";
    const string RESOURCE_NEGATIVE_BUTTON = "NegativeButton";

    //ダイアログ構造
    const string DIALOG_AREA = "DialogArea";
    const string DIALOG_MESSAGE = DIALOG_AREA + "/Message";
    const string DIALOG_H_BUTTON = DIALOG_AREA + "/HButtonArea";
    const string DIALOG_V_BUTTON = DIALOG_AREA + "/VButtonArea";
    const string DIALOG_FILTER = "Filter";

    //デフォルトボタン文言
    const string BUTTON_OK_TEXT = "OK";
    const string BUTTON_CANCEL_TEXT = "Cancel";

    //ボタンSE
    const string SE_SELECT = "Select";
    const string SE_CANCEL = "Cancel";


    //ダイアログオプション
    private static bool isVertical = false;
    //private static Color msgColor = default(Color);
    //private static List<Color> btnTextColorList = new List<Color>();
    //private static List<GameObject> btnObjList = new List<GameObject>();
    private static float dialogLowPosition = 0;


    //##### ダイアログ表示 #####

    public static GameObject OpenDialog(string message, UnityAction action)
    {
        Dictionary<string, UnityAction> positiveBtns = new Dictionary<string, UnityAction>() { { BUTTON_OK_TEXT, action } };
        return OpenDialog(message, positiveBtns);
    }

    public static GameObject OpenDialog(string message, UnityAction action, UnityAction cancelAction = null)
    {
        Dictionary<string, UnityAction> positiveBtns = new Dictionary<string, UnityAction>() { { BUTTON_OK_TEXT, action } };
        Dictionary<string, UnityAction> negativeBtns = new Dictionary<string, UnityAction>() { { BUTTON_CANCEL_TEXT, cancelAction } };
        return OpenDialog(message, positiveBtns, negativeBtns);
    }

    public static GameObject OpenDialog(string message, Dictionary<string, UnityAction> positiveBtns, Dictionary<string, UnityAction> negativeBtns = null)
    {
        if (dialog != null) CloseDialog();
        if (negativeBtns == null) negativeBtns = new Dictionary<string, UnityAction>();
        int btnCnt = positiveBtns.Count + negativeBtns.Count;
        if (btnCnt <= 0) return null;
        if (btnCnt >= 3) isVertical = true;

        //ダイアログ作成
        dialog = Instantiate(Common.Resource.GetUIResource(RESOURCE_DIALOG));
        dialogTran = dialog.transform;
        ExecDialogLowPosition();
        Transform dialogArea = dialogTran.Find(DIALOG_AREA);
        ScreenManager.Instance.StartCoroutine(ScreenManager.Instance.VerticalFade(dialogArea, true));


        //メッセージセット
        SetMessage(message);

        //ボタン作成
        string btnAreaName = (isVertical) ? DIALOG_V_BUTTON : DIALOG_H_BUTTON;
        btnAreaTran = dialogTran.Find(btnAreaName);
        if (isVertical)
        {
            SetBtn(positiveBtns);
            SetBtn(negativeBtns, false);
        }
        else
        {
            SetBtn(negativeBtns, false);
            SetBtn(positiveBtns);
        }

        return dialog;
    }

    //ダイアログ削除
    public static void CloseDialog()
    {
        if (dialog == null) return;
        Transform dialogArea = dialog.transform.Find(DIALOG_AREA);
        ScreenManager.Instance.StartCoroutine(ScreenManager.Instance.VerticalFade(dialogArea, false, () => Destroy(dialog)));

        //オプション初期化
        InitOption();
    }


    //メッセージセット
    private static void SetMessage(string message)
    {
        if (dialog == null) return;
        Text messageText = dialogTran.Find(DIALOG_MESSAGE).GetComponent<Text>();
        messageText.text = message;
    }

    //ボタン作成
    private static void SetBtn(string text, UnityAction action = null, bool isPositive = true)
    {
        if (dialog == null) return;
        string resourceName = (isPositive) ? RESOURCE_POSITIVE_BUTTON : RESOURCE_NEGATIVE_BUTTON;
        GameObject btnObj = Instantiate(Common.Resource.GetUIResource(resourceName), btnAreaTran);
        UnityAction callback = () =>
        {
            SystemSeManager.Instance.PlayYesNoSe(isPositive);
            OnClickButton(action);
        };

        btnObj.GetComponent<Button>().onClick.AddListener(callback);
        Text btnText = btnObj.transform.GetComponentInChildren<Text>();
        if (btnText != null) btnText.text = text;
    }
    private static void SetBtn(Dictionary<string, UnityAction> btnInfo, bool isPositive = true)
    {
        foreach (string text in btnInfo.Keys)
        {
            SetBtn(text, btnInfo[text], isPositive);
        }
    }

    //ボタン押下時処理
    private static void OnClickButton(UnityAction unityAction = null)
    {
        dialogTran.Find(DIALOG_FILTER).gameObject.SetActive(true);
        CloseDialog();
        if (unityAction != null) unityAction.Invoke();
    }

    //オプション設定
    private static void InitOption()
    {
        SetOptionIsVertical();
        //SetOptionMsgColor();
        //SetOptionBtnTextColor();
        //SetOptionBtnObj();
        SetDialogLowPosition();
    }
    public static void SetOptionIsVertical(bool flg = false)
    {
        isVertical = flg;
    }
    //public static void SetOptionMsgColor(Color color = default(Color))
    //{
    //    msgColor = default(Color);
    //}
    //public static void SetOptionBtnTextColor(List<Color> colors = null)
    //{
    //    btnTextColorList = (colors != null) ? colors : new List<Color>();
    //}
    //public static void SetOptionBtnObj(List<GameObject> objs = null)
    //{
    //    btnObjList = (objs != null) ? objs : new List<GameObject>();
    //}

    //ダイアログ画面下表示
    public static void SetDialogLowPosition(float h = -1)
    {
        dialogLowPosition = h;
    }
    private static void ExecDialogLowPosition()
    {
        if (dialog == null || dialogLowPosition <= 0) return;
        Transform dialogArea = dialogTran.Find(DIALOG_AREA);
        RectTransform rectTran = dialogArea.GetComponent<RectTransform>();
        rectTran.pivot = new Vector2(0.5f, 0.0f);
        rectTran.anchorMin = new Vector2(0.0f, 0.0f);
        rectTran.anchorMax = new Vector2(1.0f, 0.0f);
        rectTran.localPosition += new Vector3(rectTran.localPosition.x, rectTran.rect.height / 2 + dialogLowPosition, rectTran.localPosition.z);
    }
}
