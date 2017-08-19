using UnityEngine;

public class BgmSettingManager : MonoBehaviour
{
    public AudioClip clip;

    //BGM再生設定
    public float playStartTime = 0;

    //BGMループ設定
    public float loopStartTime = 0;   //ループ再生の開始時間
    public float loopEndTime = 0;     //ループ位置に戻る時間
}
