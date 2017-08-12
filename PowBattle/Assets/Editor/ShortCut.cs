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
    };

    [MenuItem("Tools/PlayGame &q")]
    public static void PlayGame()
    {
        try
        {
            //���s���̏ꍇ�͒�~
            if (EditorApplication.isPlaying == true)
            {
                EditorApplication.isPlaying = false;
                return;
            }
            //���s�������V�[���ֈړ���ɍĐ�
            if (OpenScene(1)) EditorApplication.isPlaying = true;
        }
        catch
        {
            Debug.Log("[Err]PlayFromPrelaunchScene");
        }
    }

    private static bool OpenScene(int sceneIndex)
    {
        bool isSuccess = false;
        try
        {
            //�ύX�`�F�b�N
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                //Cancel�ȊO
                EditorSceneManager.OpenScene("Assets/Scenes/" + sceneList[sceneIndex - 1] + ".unity");
                isSuccess = true;
            }
        }
        catch
        {
            Debug.Log("[Err]OpenScene >> "+ sceneIndex.ToString());
        }
        return isSuccess;
    }

    [MenuItem("Tools/OpenScene/Title &1")]
    public static void OpenScene1() { OpenScene(1);}
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
