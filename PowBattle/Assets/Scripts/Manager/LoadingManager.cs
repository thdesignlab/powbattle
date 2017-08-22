using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : SingletonMonoBehaviour<LoadingManager>
{
    private GameObject loadingCanvas;
    private Slider downloadBar;
    private Text downloadRate;

    protected override void Awake()
    {
        base.Awake();

        loadingCanvas = GameObject.Find("LoadingCanvas");
        DontDestroyOnLoad(loadingCanvas);
        downloadBar = loadingCanvas.transform.Find("StatusArea/Download").GetComponent<Slider>();
        downloadRate = loadingCanvas.transform.Find("StatusArea/TextArea/Rate").GetComponent<Text>();
        Close();
    }

    public void SetDownloadBar(int rate)
    {
        if (loadingCanvas)
        downloadBar.value = rate;
        downloadRate.text = rate + "%";
    }

    public void Open()
    {
        downloadBar.value = 0;
        downloadRate.text = "0%";
        loadingCanvas.SetActive(true);
    }

    public void Close()
    {
        loadingCanvas.SetActive(false);
    }
}
