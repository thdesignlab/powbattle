using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MoveScene : EditorWindow
{
    static EditorWindow window;
    static bool isGetScene = false;

    const string SCENE_FOLDER = "Assets/Scenes";
    const string BATTLE_SCENE_FOLDER = "Assets/AssetBundles/AB_Scenes";

    static List<string> sceneList = new List<string>();
    static Dictionary<int, List<int>> battleSceneList = new Dictionary<int, List<int>>();

    [MenuItem("Tools/MoveScene &w")]
    public static void ShowWindow()
    {
        if (!isGetScene)
        {
            sceneList = new List<string>();
            battleSceneList = new Dictionary<int, List<int>>();

            //シーンリスト取得
            GetScenes(SCENE_FOLDER, SetSceneList);
            GetScenes(BATTLE_SCENE_FOLDER, SetBattleSceneList);
            isGetScene = true;
        }

        if (window == null) window = EditorWindow.GetWindow(typeof(MoveScene), true, "MoveScene");
    }

    //シーン取得
    private static void GetScenes(string folder, UnityAction<string> callback)
    {
        GetScenes(new string[] { folder }, callback);
    }
    private static void GetScenes(string[] folders, UnityAction<string> callback)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Scene", folders))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            string sceneName = AssetDatabase.LoadMainAssetAtPath(path).ToString();
            Regex r = new Regex(@"^(.*)\(", RegexOptions.IgnoreCase);
            Match m = r.Match(sceneName);
            if (m.Groups.Count < 2) continue;
            callback(m.Groups[1].ToString().Trim());
        }
    }

    //シーン格納
    private static void SetSceneList(string sceneName)
    {
        sceneList.Add(sceneName);
    }

    //バトルシーン格納
    private static void SetBattleSceneList(string sceneName)
    {
        int story = 0;
        int stage = 0;
        if (Common.Func.CheckBattleSceneNo(sceneName, out story, out stage))
        {
            if (!battleSceneList.ContainsKey(story)) battleSceneList.Add(story, new List<int>());
            battleSceneList[story].Add(stage);
        }
    }

    //シーン移動
    public static bool OpenScene(string sceneName, string path = "")
    {
        bool isSuccess = false;
        try
        {
            //変更チェック
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                //Cancel以外
                EditorSceneManager.OpenScene(path + sceneName + ".unity");
                isSuccess = true;
                if (window != null) window.Close();
            }
        }
        catch
        {
            Debug.Log("[Err]OpenScene >> " + path + sceneName);
        }
        return isSuccess;
    }

    //描画
    void OnGUI()
    {
        float limitX = 5;
        float indexX = 1;
        float btnW = 50;
        float btnH = 25;
        float lblW = 100;
        float lblH = 15;
        float space = 10;
        float x = space;
        float y = space;

        //更新ボタン
        if (GUI.Button(new Rect(x, y, btnW, btnH), "Reload"))
        {
            isGetScene = false;
            ShowWindow();
            return;
        }
        y += space + btnH;

        if (sceneList.Count > 0)
        {
            btnW = 80;
            btnH = 25;
            GUI.Label(new Rect(x, y, lblW, lblH), "Scene");
            y += space + lblH;
            foreach (string sceneName in sceneList)
            {
                if (GUI.Button(new Rect(x, y, btnW, btnH), sceneName))
                {
                    OpenScene(sceneName, SCENE_FOLDER + "/");
                }
                if (indexX < limitX)
                {
                    x += space + btnW;
                    indexX++;
                }
                else
                {
                    x = space;
                    indexX = 1;
                    y += space + btnH;
                }
            }
        }

        if (battleSceneList.Count > 0)
        {
            x = space;
            GUI.Label(new Rect(x, y, lblW, lblH), "Battle");
            y += space + lblH;
            btnW = 50;
            foreach (int story in battleSceneList.Keys)
            {
                foreach (int stage in battleSceneList[story])
                {
                    string battleName = story + "-" + stage;
                    if (GUI.Button(new Rect(x, y, btnW, btnH), battleName))
                    {
                        OpenScene(Common.Func.GetBattleSceneName(story, stage), BATTLE_SCENE_FOLDER + "/Battle" + story + "/");
                    }
                    x += space + btnW;
                }
                y += space + btnH;
            }
        }
    }
}