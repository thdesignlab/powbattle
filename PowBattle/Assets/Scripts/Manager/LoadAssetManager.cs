using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using AssetBundles;
using System.Collections.Generic;

public class LoadAssetManager : SingletonMonoBehaviour<LoadAssetManager>
{
	public const string AssetBundlesOutputPath = "/AssetBundles/";

    private const string ASSET_BUNDLE_MATERIALS = "materials";
    private const string ASSET_BUNDLE_PREFABS = "prefabs";
    private const string ASSET_BUNDLE_RESOURCES = "resources";
    private const string ASSET_BUNDLE_SOUNDS = "sounds";
    private const string ASSET_BUNDLE_TEXTURES = "textures";
    private const string ASSET_BUNDLE_TT_ITEM = "tt_item";
    private const string ASSET_BUNDLE_SCENES = "scenes";


    public IEnumerator LoadAssets()
	{
		yield return StartCoroutine(Initialize() );

        // Load asset.
        List<string> assetBundleNameList = new List<string>() {
            ASSET_BUNDLE_SOUNDS,
            ASSET_BUNDLE_MATERIALS,
            ASSET_BUNDLE_TEXTURES,
            ASSET_BUNDLE_TT_ITEM,
            ASSET_BUNDLE_PREFABS,
            ASSET_BUNDLE_RESOURCES,
        };
        yield return StartCoroutine(InstantiateGameObjectAsync(assetBundleNameList) );
	}

	// Initialize the downloading url and AssetBundleManifest object.
	protected IEnumerator Initialize()
	{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
		AssetBundleManager.SetDevelopmentAssetBundleServer ();
#else
		AssetBundleManager.SetSourceAssetBundleURL(Common.CO.APP_ASSET_BUNDLE_URL);
        
#endif
        var request = AssetBundleManager.Initialize();
		if (request != null)
			yield return StartCoroutine(request);
	}

    protected IEnumerator InstantiateGameObjectAsync(string assetBundleName, bool loadingBar = true)
    {
        yield return StartCoroutine(InstantiateGameObjectAsync(new List<string>() { assetBundleName }, loadingBar));
    }
    protected IEnumerator InstantiateGameObjectAsync (List<string> assetBundleNameList, bool loadingBar = true)
	{
        int totalProgress = assetBundleNameList.Count * 100;
        int completeCount = 0;
        int nowProgress = 0;
        int rate = 0;
        if (loadingBar)
        {
            LoadingManager.Instance.Open();
            LoadingManager.Instance.SetDownloadBar(rate);
            yield return new WaitForSeconds(1.0f);
        }

        foreach (string name in assetBundleNameList)
        {
            AssetBundleManager.LoadAssetBundle(name);
            for (;;)
            {
                if (AssetBundleManager.m_DownloadingWWWs.ContainsKey(name))
                {
                    //ダウンロード中
                    nowProgress = (int)(AssetBundleManager.m_DownloadingWWWs[name].progress * 100);
                    rate = (completeCount * 100 + nowProgress) * 100 / totalProgress;
                    if (loadingBar) LoadingManager.Instance.SetDownloadBar(rate);
                }
                else
                {
                    //ダウンロード完了
                    completeCount++;
                    rate = (completeCount * 100) * 100 / totalProgress;
                    if (loadingBar) LoadingManager.Instance.SetDownloadBar(rate);
                    nowProgress = 0;
                    break;
                }
                yield return null;
            }
        }

        if (loadingBar)
        {
            yield return new WaitForSeconds(1.0f);
            LoadingManager.Instance.Close();
        }
    }
    
    public Object LoadResource(string levelName)
    {
        Object obj = null;
#if UNITY_EDITOR
        if (AssetBundleManager.SimulateAssetBundleInEditor)
        {
            AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(ASSET_BUNDLE_RESOURCES, levelName, typeof(Object));
            if (request == null)
            {
                Debug.Log("LoadResource error:" + levelName);
                return null;
            }
            obj = request.GetAsset<Object>();
        }
        else
#endif
        {
            string error;
            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(ASSET_BUNDLE_RESOURCES, out error);
            if (bundle == null || !string.IsNullOrEmpty(error))
            {
                Debug.Log("LoadResource error:" + levelName + " >> " + error);
                return null;
            }
            obj = bundle.m_AssetBundle.LoadAsset<Object>(levelName);
        }
        return obj;
    }

    public IEnumerator LoadScene(string levelName)
    {
        Debug.Log("### start LoadScene >> " + levelName);

        AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync(ASSET_BUNDLE_SCENES, levelName, true);
        if (request == null) yield break;
        yield return StartCoroutine(request);

        Debug.Log("finish LoadScene >> " + levelName);
    }

    //protected IEnumerator InstantiateGameObjectAsync(string assetBundleName, string assetName)
    //{
    //    // This is simply to get the elapsed time for this phase of AssetLoading.
    //    float startTime = Time.realtimeSinceStartup;

    //    // Load asset from assetBundle.
    //    AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
    //    if (request == null)
    //        yield break;
    //    yield return StartCoroutine(request);

    //    // Get the asset.
    //    GameObject prefab = request.GetAsset<GameObject>();

    //    if (prefab != null)
    //        GameObject.Instantiate(prefab);

    //    // Calculate and display the elapsed time.
    //    float elapsedTime = Time.realtimeSinceStartup - startTime;
    //    Debug.Log(assetName + (prefab == null ? " was not" : " was") + " loaded successfully in " + elapsedTime + " seconds");
    //}
}
