using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;

public class Shortcut
{
    private static string[] sceneList = new string[]
    {
        Common.CO.SCENE_TITLE,      //1
        Common.CO.SCENE_BATTLE,     //2
        Common.CO.SCENE_MAIN,      //3
        Common.CO.SCENE_STORY,      //4
        Common.CO.SCENE_BATTLE+"Castel",     //2
    };

    [MenuItem("Tools/PlayGame &q")]
    public static void PlayGame()
    {
        try
        {
            //実行中の場合は停止
            if (EditorApplication.isPlaying == true)
            {
                EditorApplication.isPlaying = false;
                return;
            }
            //実行したいシーンへ移動後に再生
            if (OpenScene(1)) EditorApplication.isPlaying = true;
        }
        catch
        {
            Debug.Log("[Err]PlayFromPrelaunchScene");
        }
    }

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
            }
        }
        catch
        {
            Debug.Log("[Err]OpenScene >> " + path + sceneName);
        }
        return isSuccess;
    }
    private static bool OpenScene(int sceneIndex)
    {
        return OpenScene("Assets/Scenes/" + sceneList[sceneIndex - 1]);
    }

    [MenuItem("Tools/OpenScene/Title &1")]
    public static void OpenScene1() { OpenScene(1); }
    [MenuItem("Tools/OpenScene/Battle &2")]
    public static void OpenScene2() { OpenScene(2); }
    [MenuItem("Tools/OpenScene/Custom &3")]
    public static void OpenScene3() { OpenScene(3); }
    [MenuItem("Tools/OpenScene/Store &4")]
    public static void OpenScene4() { OpenScene(4); }
    [MenuItem("Tools/OpenScene/Ranking &5")]
    public static void OpenScene5() { OpenScene(5); }
    [MenuItem("Tools/OpenScene/6 &6")]
    public static void OpenScene6() { OpenScene(6); }
    [MenuItem("Tools/OpenScene/7 &7")]
    public static void OpenScene7() { OpenScene(7); }
    [MenuItem("Tools/OpenScene/8 &8")]
    public static void OpenScene8() { OpenScene(8); }
}
