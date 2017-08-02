using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserManager
{
    public static Dictionary<int, string> userInfo;         //ユーザー情報
    public static Dictionary<string, int> userEquipment;    //ユーザー装備
    public static Dictionary<int, int> userConfig;          //設定

    public static bool isAdmin;                     //管理者FLG
    public static int userPoint;                    //所持ポイント
    public static bool isGachaFree = false;         //ガチャFreeFlg
    public static string apiToken;                  //API接続用Token

    public static bool isCheckCustomSceneNetwork = false;      //ネットワーク接続チェックFLG

    //ユーザー情報初期値設定
    private static void InitPlayerPrefs()
    {
        userInfo = new Dictionary<int, string>();
        userEquipment = new Dictionary<string, int>();
        userConfig = new Dictionary<int, int>();
        isAdmin = false;
        apiToken = "";
        userPoint = -1;
    }

    //ユーザー情報設定
    public static void SetPlayerPrefs()
    {
        //初期化
        InitPlayerPrefs();

        //データロード
        SetUserInfo();
        SetUserConfig();
        PlayerPrefs.Save();
    }

    //◆UserInfo
    private static void SetUserInfo()
    {
        string key = Common.PP.USER_INFO;
        bool isUpdate = false;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userInfo = PlayerPrefsUtility.LoadDict<int, string>(key);
        }

        //ユーザーID
        if (!userInfo.ContainsKey(Common.PP.INFO_USER_ID))
        {
            userInfo.Add(Common.PP.INFO_USER_ID, "-1");
            isUpdate = true;
        }
        //ユーザー名
        if (!userInfo.ContainsKey(Common.PP.INFO_USER_NAME))
        {
            userInfo.Add(Common.PP.INFO_USER_NAME, "Guest" + Random.Range(1, 99999));
            isUpdate = true;
        }
        //UUID
        if (!userInfo.ContainsKey(Common.PP.INFO_UUID))
        {
            userInfo.Add(Common.PP.INFO_UUID, "");
            isUpdate = true;
        }
        //password
        if (!userInfo.ContainsKey(Common.PP.INFO_PASSWORD))
        {
            userInfo.Add(Common.PP.INFO_PASSWORD, "");
            isUpdate = true;
        }

        //保存
        if (isUpdate) PlayerPrefsUtility.SaveDict<int, string>(key, userInfo);
    }

    //◆UserConfig
    private static void SetUserConfig()
    {
        string key = Common.PP.USER_CONFIG;
        bool isUpdate = false;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userConfig = PlayerPrefsUtility.LoadDict<int, int>(key);
        }

        //BGMValue
        if (!userConfig.ContainsKey(Common.PP.CONFIG_BGM_VALUE))
        {
            userConfig.Add(Common.PP.CONFIG_BGM_VALUE, ConfigManager.SLIDER_COUNT / 2);
            isUpdate = true;
        }
        //BGMMute
        if (!userConfig.ContainsKey(Common.PP.CONFIG_BGM_MUTE))
        {
            userConfig.Add(Common.PP.CONFIG_BGM_MUTE, 0);
            isUpdate = true;
        }
        //SEValue
        if (!userConfig.ContainsKey(Common.PP.CONFIG_SE_VALUE))
        {
            userConfig.Add(Common.PP.CONFIG_SE_VALUE, ConfigManager.SLIDER_COUNT / 2);
            isUpdate = true;
        }
        //SEMute
        if (!userConfig.ContainsKey(Common.PP.CONFIG_SE_MUTE))
        {
            userConfig.Add(Common.PP.CONFIG_SE_MUTE, 0);
            isUpdate = true;
        }
        //VoiceValue
        if (!userConfig.ContainsKey(Common.PP.CONFIG_VOICE_VALUE))
        {
            userConfig.Add(Common.PP.CONFIG_VOICE_VALUE, ConfigManager.SLIDER_COUNT / 2);
            isUpdate = true;
        }
        //VoiceMute
        if (!userConfig.ContainsKey(Common.PP.CONFIG_VOICE_MUTE))
        {
            userConfig.Add(Common.PP.CONFIG_VOICE_MUTE, 0);
            isUpdate = true;
        }

        //保存
        if (isUpdate) PlayerPrefsUtility.SaveDict<int, int>(key, userConfig);
    }



    //##### ユーザー情報 #####

    //API用情報
    public static void SetApiInfo(string uuid, string password)
    {
        userInfo[Common.PP.INFO_UUID] = uuid;
        userInfo[Common.PP.INFO_PASSWORD] = password;
        PlayerPrefsUtility.SaveDict<int, string>(Common.PP.USER_INFO, userInfo);
        PlayerPrefs.Save();
    }

    //ユーザー名変更
    public static void SetUserName(string userName)
    {
        userInfo[Common.PP.INFO_USER_NAME] = userName;
        PlayerPrefsUtility.SaveDict<int, string>(Common.PP.USER_INFO, userInfo);
        PlayerPrefs.Save();
    }

    //ユーザーID取得
    public static int GetUserId()
    {
        int uid = -1;
        if (userInfo.ContainsKey(Common.PP.INFO_USER_ID))
        {
            uid = int.Parse(userInfo[Common.PP.INFO_USER_ID]);
        }
        return uid;
    }

    //##### ユーザーコンフィグ #####

    public static void SaveConfig(bool isSave = true)
    {
        PlayerPrefsUtility.SaveDict<int, int>(Common.PP.USER_CONFIG, userConfig);
        if (isSave) PlayerPrefs.Save();
    }

    //##### デバッグ #####

    public static void DispUserInfo()
    {
        foreach (int key in userInfo.Keys)
        {
            Debug.Log(key+" >> "+userInfo[key]);
        }
        Debug.Log("isAdmin >> "+isAdmin);
    }
    public static void DispUserConfig()
    {
        foreach (int key in userConfig.Keys)
        {
            Debug.Log(key + " >> " + userConfig[key]);
        }
    }
    public static void DeleteUser()
    {
        //データ削除(debug用)
        PlayerPrefs.DeleteAll();
        InitPlayerPrefs();
        PlayerPrefs.Save();
    }
}
