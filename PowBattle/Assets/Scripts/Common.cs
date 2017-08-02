﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    //### 定数 ###
    public static class CO
    {
        //アプリID
        public const string APP_NAME_IOS = "";
        public const string APP_NAME_ANDROID = "com.ThDesignLab";

        //HomePage
        public const string WEBVIEW_KEY = "?webview";
        public static string HP_URL = "http://lostworks.th-designlab.com/";
        public static string HP_TUTORIAL_URL = HP_URL + "arena/howtoplay";

        //シーン名
        public const string SCENE_TITLE = "Title";
        public const string SCENE_BATTLE = "Battle";

        //リソースフォルダ
        public const string RESOURCE_DIR = "Resources/";
        public const string RESOURCE_IMAGE_DIR = "Images/";
        public const string RESOURCE_MESSAGE_DIR = RESOURCE_IMAGE_DIR + "Message/";
        public const string RESOURCE_UNIT_DIR = "Unit/";

        //タグ
        public const string TAG_UNIT = "Unit";
        public const string TAG_ENEMY = "Enemy";
        public const string TAG_SP_MINE = "SpawnPointMine";
        public const string TAG_SP_ENEMY = "SpawnPointEnemy";

        //衝突判定するタグ
        public static string[] ColliderHitTagArray = new string[]
        {
        };
    }

    //### 端末保持情報 ###
    public static class PP
    {
        //保存情報
        public const string USER_INFO = "UserInfo";
        public const string USER_CONFIG = "UserConfig";

        //ユーザー情報項目
        public const int INFO_USER_ID = 0;
        public const int INFO_USER_NAME = 1;
        public const int INFO_UUID = 3;
        public const int INFO_PASSWORD = 4;

        //コンフィグ情報項目
        public const int CONFIG_BGM_VALUE = 0;
        public const int CONFIG_BGM_MUTE = 1;
        public const int CONFIG_SE_VALUE = 2;
        public const int CONFIG_SE_MUTE = 3;
        public const int CONFIG_VOICE_VALUE = 4;
        public const int CONFIG_VOICE_MUTE = 5;

    }

    //### 共通関数 ###
    public static class Func
    {
        //platform確認
        public static bool IsAndroid()
        {
            if (Application.platform == RuntimePlatform.Android) return true;
            return false;
        }
        public static bool IsIos()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer) return true;
            return false;
        }
        public static bool IsPc()
        {
            if (IsAndroid() || IsIos()) return false;
            return true;
        }

        //storeUrl取得
        public static string GetStoreUrl()
        {
            string url = "";
#if UNITY_IOS
            if (!string.IsNullOrEmpty(CO.APP_NAME_IOS))
            {
                url= string.Format("itms-apps://itunes.apple.com/app/id{0}?mt=8", CO.APP_NAME_IOS);
            }
#elif UNITY_ANDROID
            if (!string.IsNullOrEmpty(CO.APP_NAME_ANDROID))
            {
                if (IsPc())
                {
                    url = "https://play.google.com/store/apps/details?id=" + CO.APP_NAME_ANDROID;
                }
                else
                {
                    url = "market://details?id=" + CO.APP_NAME_ANDROID;
                }
            }
#endif
            return url;
        }
        
        //ステータスバー設定
        public static void SetStatusbar()
        {
            Screen.fullScreen = true;

            //Androidステータスバー
            ApplicationChrome.statusBarState = ApplicationChrome.States.TranslucentOverContent;
            //ApplicationChrome.statusBarState = ApplicationChrome.States.Visible;
            ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
        }

        //配列チェック
        private static bool InArrayString(string[] tags, string tagName)
        {
            bool flg = false;
            foreach (string tag in tags)
            {
                if (tagName == tag)
                {
                    flg = true;
                    break;
                }
            }
            return flg;
        }

        //衝突判定タグ
        public static bool IsColliderHitTag(string tag)
        {
            return InArrayString(CO.ColliderHitTagArray, tag);
        }

        //三角関数
        public static float GetSin(float time, float anglePerSec = 360, float startAngle = 0)
        {
            float angle = (startAngle + anglePerSec * time) % 360;
            float radian = Mathf.PI / 180 * angle;
            return Mathf.Sin(radian);
        }

        //抽選
        public static T Draw<T>(Dictionary<T, int> targets)
        {
            T drawObj = default(T);
            int sumRate = 0;
            List<T> targetValues = new List<T>();
            foreach (T obj in targets.Keys)
            {
                sumRate += targets[obj];
                targetValues.Add(obj);
            }
            if (sumRate == 0) return drawObj;

            int drawNum = Random.Range(1, sumRate + 1);
            sumRate = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                int key = Random.Range(0, targetValues.Count);
                sumRate += targets[targetValues[key]];
                if (sumRate >= drawNum)
                {
                    drawObj = targetValues[key];
                    break;
                }
                targetValues.RemoveAt(key);
            }
            return drawObj;
        }

        public static TKey RandomDic<TKey, TValue>(Dictionary<TKey, TValue> dic)
        {
            return dic.ElementAt(Random.Range(0, dic.Count)).Key;
        }

    }

    //### ユニット ###
    public static class Unit
    {
        public static Dictionary<int, string> unitInfo = new Dictionary<int, string>()
        {
            { 0, "Unit" },
            { 1, "UnitGunner" },
            { 2, "Unit" },
        };
    }

}