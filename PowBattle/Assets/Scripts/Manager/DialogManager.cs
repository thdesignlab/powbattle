using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogManager : MonoBehaviour
{
    //ダイアログ
    private static GameObject dialog;
    private static Text textMessage;

    //メッセージ
    private static GameObject messageObj;
    private static Image messageImage;
    private static Text messageText;

    const string RESOURCE_SELECT_DIALOG = "UI/SelectDialog";
    const string RESOURCE_SELECT_BUTTON = "UI/SelectDialogButton";
    const string RESOURCE_DIALOG = "UI/Dialog";
    const string RESOURCE_MESSAGE = "UI/Message";
    const string RESOURCE_IMAGE_DIR = "Image/";


    //ボタン
    const string BUTTON_OK_TEXT = "OK";
    const string BUTTON_CANCEL_TEXT = "Cancel";

    //ボタンText色
    public static Color cancelColor = new Color32(255, 255, 255, 255);
    public static Color blueColor = new Color32(0, 255, 234, 255);
    public static Color redColor = new Color32(255, 79, 79, 255);
    public static Color yellowColor = new Color32(238, 255, 79, 255);
    public static Color greenColor = new Color32(79, 255, 109, 255);
    public static Color purpleColor = new Color32(255, 79, 221, 255);

    //##### 選択ダイアログ表示 #####
    public static GameObject OpenSelectDialog(string text, Dictionary<string, UnityAction> btnInfoDic, bool isCancel)
    {
        return OpenSelectDialog("", text, "", btnInfoDic, isCancel);
    }
    public static GameObject OpenSelectDialog(string text, Dictionary<string, UnityAction> btnInfoDic, bool isCancel, List<GameObject> btnObjects)
    {
        return OpenSelectDialog("", text, "", btnInfoDic, isCancel, btnObjects);
    }
    public static GameObject OpenSelectDialog(string text, Dictionary<string, UnityAction> btnInfoDic, bool isCancel, List<Color> btnColors)
    {
        return OpenSelectDialog("", text, "", btnInfoDic, isCancel, null, btnColors);
    }
    public static GameObject OpenSelectDialog(string title, string text, string imageName, Dictionary<string, UnityAction> btnInfoDic, bool isCancel, List<Color> btnColors)
    {
        return OpenSelectDialog(title, text, imageName, btnInfoDic, isCancel, null, btnColors);
    }
    public static GameObject OpenSelectDialog(string title, string text, string imageName, Dictionary<string, UnityAction> btnInfoDic, bool isCancel, List<GameObject> btnObjects = null, List<Color> btnColors = null)
    {
        if (dialog != null) CloseDialog();
        if (btnInfoDic.Count <= 0) return null;

        //ダイアログ生成
        dialog = Instantiate((GameObject)Resources.Load(RESOURCE_SELECT_DIALOG));
        Transform dialogTran = dialog.transform;

        //タイトル
        if (!string.IsNullOrEmpty(title))
        {
            Transform titleTran = dialogTran.Find("DialogArea/Title");
            titleTran.gameObject.SetActive(true);
            titleTran.GetComponent<Text>().text = title;
        }

        //画像
        if (!string.IsNullOrEmpty(imageName))
        {
            Sprite imgSprite = Resources.Load<Sprite>(RESOURCE_IMAGE_DIR + imageName);
            if (imgSprite != null)
            {
                Transform imageTran = dialogTran.Find("DialogArea/Image");
                imageTran.gameObject.SetActive(true);
                Image img = imageTran.GetComponent<Image>();
                img.sprite = imgSprite;
                img.preserveAspect = true;
            }
        }

        //メッセージ
        textMessage = dialogTran.Find("DialogArea/Message").GetComponent<Text>();
        textMessage.text = text;

        //セレクトボタン
        int index = 0;
        foreach (string btnText in btnInfoDic.Keys)
        {
            Object btnObj = null;
            Color btnColor = default(Color);
            if (btnObjects != null)
            {
                if (btnObjects.Count > 0 && btnObjects.Count > index) btnObj = btnObjects[index];
            }
            if (btnColors != null)
            {
                if (btnColors.Count > 0 && btnColors.Count > index) btnColor = btnColors[index];
            }
            SetSelectBtn(btnText, btnInfoDic[btnText], btnObj, btnColor);
            index++;
        }

        //Cancelボタン
        if (isCancel)
        {
            SetSelectBtn(BUTTON_CANCEL_TEXT, null, null, cancelColor);
        }
        return dialog;
    }

    private static void SetSelectBtn(string btnText, UnityAction action = null, Object btnObj = null, Color btnColor = default(Color))
    {
        GameObject btn = null;
        if (btnObj != null)
        {
            btn = (GameObject)Instantiate(btnObj);
        }
        else
        {
            btn = (GameObject)Instantiate(Resources.Load(RESOURCE_SELECT_BUTTON));
        }
        btn.GetComponent<Button>().onClick.AddListener(() => OnClickButton(action));
        Text buttonText = btn.transform.GetComponentInChildren<Text>();
        if (buttonText != null) buttonText.text = btnText;
        if (btnColor != default(Color)) buttonText.color = btnColor;
        btn.transform.SetParent(dialog.transform.Find("DialogArea"), false);
    }


    //##### ダイアログ表示 #####

    public static GameObject OpenDialog(string text, UnityAction okAction = null, bool isCancel = false)
    {
        return OpenDialog(text, BUTTON_OK_TEXT, okAction, isCancel);
    }
    public static GameObject OpenDialog(string text, string btnText, UnityAction okAction, bool isCancel)
    {
        return OpenDialog(text, new List<string>() { btnText }, new List<UnityAction>() { okAction }, isCancel);
    }
    public static GameObject OpenDialog(string text, UnityAction okAction, UnityAction cancelAction, bool isCancel = false)
    {
        return OpenDialog(text, new List<string>() { BUTTON_OK_TEXT, BUTTON_CANCEL_TEXT }, new List<UnityAction>() { okAction, cancelAction }, isCancel);
    }
    public static GameObject OpenDialog(string text, List<string> buttons, List<UnityAction> actions, bool isCancel = false)
    {
        if (dialog != null) CloseDialog();
        if (buttons.Count <= 0 || actions.Count <= 0) return null;

        dialog = Instantiate((GameObject)Resources.Load(RESOURCE_DIALOG));
        textMessage = dialog.transform.Find("DialogArea/Message").GetComponent<Text>();
        GameObject buttonOkObj = dialog.transform.Find("DialogArea/ButtonArea/OK").gameObject;
        GameObject buttonCancelObj = dialog.transform.Find("DialogArea/ButtonArea/Cancel").gameObject;

        //テキスト設定
        textMessage.text = text;

        //OKボタン
        SetBtn(buttonOkObj, buttons[0], actions[0]);
             
        //Cancelボタン
        if (isCancel || buttons.Count >= 2)
        {
            string btnText = BUTTON_CANCEL_TEXT;
            UnityAction btnAction = null;
            if (buttons.Count >= 2)
            {
                btnText = buttons[1];
                btnAction = actions[1];
            }
            SetBtn(buttonCancelObj, btnText, btnAction);
            buttonCancelObj.SetActive(true);
        }
        else
        {
            buttonCancelObj.SetActive(false);
        }
        return dialog;
    }

    public static void CloseDialog(bool isFadeOut = true)
    {
        if (dialog == null) return;
        Destroy(dialog);
    }

    private static void SetBtn(GameObject btnObj, string btnText, UnityAction btnAction)
    {
        btnObj.GetComponent<Button>().onClick.AddListener(() => OnClickButton(btnAction));
        Text buttonOkText = btnObj.transform.GetComponentInChildren<Text>();
        if (buttonOkText != null) buttonOkText.text = btnText;
    }

    private static void OnClickButton(UnityAction unityAction = null)
    {
        dialog.transform.Find("Filter").gameObject.SetActive(true);
        CloseDialog();
        if (unityAction != null)
        {
            unityAction.Invoke();
        }
    }

    public static Text GetDialogText()
    {
        return textMessage;
    }



}
