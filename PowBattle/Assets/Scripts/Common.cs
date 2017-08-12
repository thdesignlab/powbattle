using UnityEngine;
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

        //自軍・敵軍
        public const int SIDE_UNKNOWN = -1;
        public const int SIDE_MINE = 0;
        public const int SIDE_ENEMY = 1;
        public static readonly int[] sideArray = new int[] { SIDE_MINE, SIDE_ENEMY };

        //優勢・劣勢
        public const int SITUATION_DRAW = 0;
        public const int SITUATION_WIN = 1;
        public const int SITUATION_LOSE = -1;

        //タグ
        public const string TAG_PLAYER = "Player";
        //タグ ユニット
        public const string TAG_UNIT = "Unit";
        public const string TAG_ENEMY = "Enemy";
        //タグ HQ
        public const string TAG_HQ = "HQ";
        public const string TAG_ENEMY_HQ = "EnemyHQ";
        //タグ ユニットパーツ
        public const string TAG_UNIT_BODY = "UnitBody";
        public const string TAG_WEAPON_JOINT = "WeaponJoint";
        //タグ 出現位置
        public const string TAG_SP_MINE = "SpawnPointMine";
        public const string TAG_SP_ENEMY = "SpawnPointEnemy";
        public const string TAG_EXSP_MINE = "ExtraSpawnPointMine";
        public const string TAG_EXSP_ENEMY = "ExtraSpawnPointEnemy";
        public const string TAG_SP_PLAYER = "SpawnPointPlayer";
        //タグ その他
        public const string TAG_EFFECT = "Effect";
        public const string TAG_DAMAGE_EFFECT = "DamageEffect";
        public const string TAG_MUZZLE = "Muzzle";
        public const string TAG_OBSTACLE = "Obstacle";
        public const string TAG_BREAK_OBSTACLE = "BreakableObstacle";
        

        public static readonly string[] tagUnitArray = new string[] { TAG_UNIT, TAG_ENEMY };
        public static readonly string[] tagHQArray = new string[] { TAG_HQ, TAG_ENEMY_HQ };
        public static readonly string[] tagSpawnPointArray = new string[] { TAG_SP_MINE, TAG_SP_ENEMY };
        public static readonly string[] tagExSpawnPointArray = new string[] { TAG_EXSP_MINE, TAG_EXSP_ENEMY };

        //レイヤー
        public const string LAYER_UNIT = "Unit";
        public const string LAYER_ENEMY = "Enemy";
        public const string LAYER_OBSTACLE = "Obstacle";
        public const string LAYER_BREAK_OBSTACLE = "BreakableObstacle";

        public static readonly string[] layerUnitArray = new string[] { LAYER_UNIT, LAYER_ENEMY };

        //ステータスタイプ
        public const int STATUS_ATTACK = 0;
        public const int STATUS_DEFENCE = 1;
        public const int STATUS_SPEED = 2;

        //カメラモード
        public const int CAM_MODE_FREE = 0;
        public const int CAM_MODE_FIRST = 1;
        public const int CAM_MODE_THIRD = 2;
        public const int CAM_MODE_VR = 3;

        //ユニットモーションFLG
        public const string MOTION_FLG_WAIT = "Wait";
        public const string MOTION_FLG_RUN = "Run";
        public const string MOTION_FLG_JUMP = "Jump";
        public const string MOTION_FLG_ATTACK = "Attack";
        public const string MOTION_FLG_DEAD = "Dead";

        //ユニットモーションTAG
        public const string MOTION_TAG_WAIT = "Wait";
        public const string MOTION_TAG_RUN = "Run";
        public const string MOTION_TAG_JUMP = "Jump";
        public const string MOTION_TAG_ATTACK = "Attack";
        public const string MOTION_TAG_DEAD = "Dead";

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

        //配列内存在チェック
        public static int InArray<T>(T[] array, T target)
        {
            int index = -1;
            //foreach (T a in array)
            for (int i = 0; i < array.Length; i++)
            {
                if (target.Equals(array[i]))
                {
                    index = i;
                    break;
                }
            }
            return index;
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

        //ランダム抽選(Dictionary)
        public static TKey RandomDic<TKey, TValue>(Dictionary<TKey, TValue> dic)
        {
            return dic.ElementAt(Random.Range(0, dic.Count)).Key;
        }

        //ランダム抽選(List)
        public static T RandomList<T>(List<T> list)
        {
            int index = Random.Range(0, list.Count);
            return list[index];
        }

        //レイヤーセット
        public static void SetLayer(GameObject obj, string layerName, bool needSetChildrens = true)
        {
            int layerNo = LayerMask.NameToLayer(layerName);
            SetLayer(obj, layerNo, needSetChildrens);
        }
        public static void SetLayer(GameObject obj, int layerNo, bool needSetChildrens = true)
        {
            if (obj == null) return;

            obj.layer = layerNo;

            //子に設定する必要がない場合はここで終了
            if (!needSetChildrens) return;

            //子のレイヤーにも設定する
            foreach (Transform child in obj.transform)
            {
                SetLayer(child.gameObject, layerNo, needSetChildrens);
            }
        }

        //パーセント取得
        public static int GetPer(int target, int total, int def = 0)
        {
            if (total == 0) return def;
            return (int)(Mathf.Ceil(target * 100 / (float)total));
        }

        //視線判定するレイヤーマスクを取得する
        public static int GetSightLayerMask(int side)
        {
            string targetLayer = CO.layerUnitArray[side];
            return LayerMask.GetMask(new string[] { targetLayer, CO.LAYER_OBSTACLE, CO.LAYER_BREAK_OBSTACLE });
        }

        //陣判定
        public static int GetMySide(string tag)
        {
            int side = CO.SIDE_UNKNOWN;
            switch (tag)
            {
                case CO.TAG_UNIT:
                case CO.TAG_HQ:
                    side = CO.SIDE_MINE;
                    break;

                case CO.TAG_ENEMY:
                case CO.TAG_ENEMY_HQ:
                    side = CO.SIDE_ENEMY;
                    break;
            }
            return side;
        }

        //自陣判定
        public static bool IsMySide(string myTag, string targetTag)
        {
            if (myTag == targetTag) return true;

            int mySide = GetMySide(myTag);
            int targetSide = GetMySide(targetTag);
            if (mySide == CO.SIDE_UNKNOWN || targetSide == CO.SIDE_UNKNOWN) return false;

            return (mySide == targetSide);
        }

        //子供からタグ検索
        public static Transform SearchChildTag(Transform parent, string tag)
        {
            if (parent == null) return null;
            Transform child = null;
            foreach (Transform t in parent.transform)
            {
                if (t.tag == tag)
                {
                    child = t;
                    break;
                }
            }
            return child;
        }

        //一番近いTransformを取得
        public static Transform GetNearest(List<Transform> tranList, Vector3 basePos)
        {
            float distance = 0;
            Transform nearTran = null;
            foreach (Transform tran in tranList)
            {
                if (tran == null) continue;
                float tmpDistance = Vector3.Distance(tran.position, basePos);
                if (distance == 0 || distance > tmpDistance)
                {
                    distance = tmpDistance;
                    nearTran = tran;
                }
            }
            return nearTran;
        }
    }

    //### ユニット ###
    public static class Unit
    {
        public static Dictionary<int, string> unitInfo = new Dictionary<int, string>()
        {
            { 0, "UnitKnight" },
            { 1, "UnitKnight2" },
            { 2, "UnitGunner" },
            { 3, "UnitTank" },
        };
    }

}