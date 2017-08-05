using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class BattleManager : SingletonMonoBehaviour<BattleManager>
{
    [SerializeField]
    private Canvas battleCanvas;
    private Slider battleGage;

    private bool isPause = false;

    private List<List<Transform>> spawnPoints = new List<List<Transform>>() { new List<Transform>() { }, new List<Transform>() { } };
    private List<List<Transform>> exSpawnPoints = new List<List<Transform>>() { new List<Transform>() { }, new List<Transform>() { } };
    private List<int> spawnIndex = new List<int>() { 0, 0 };
    private List<int> unitCnt = new List<int>() { 0, 0 };

    [HideInInspector]
    public List<int> unitCntRate = new List<int>() { 50, 50 };
    [HideInInspector]
    public List<Transform> hqInfo = new List<Transform>() { null, null };
    [HideInInspector]
    public List<HQController> hqCtrl = new List<HQController>() { null, null };
    [HideInInspector]
    public List<int> battleSituation = new List<int>() { 50, 50 };

    private List<List<int>> spawnUnits = new List<List<int>>() { new List<int>() { }, new List<int>() { } };
    private List<List<int>> extraUnits = new List<List<int>>() { new List<int>() { }, new List<int>() { } };

    //test用
    [SerializeField]
    int testUnitLimit;
    [SerializeField]
    int testRespawnInterval;
    [SerializeField]
    int testRespawnIntervalAdd;
    [SerializeField]
    List<int> testRespawnUnits;
    [SerializeField]
    List<int> testEnemyRespawnUnits;
    List<int> testExtraUnits = new List<int>() { 1, 1, 1, 1, 2 };
    [SerializeField]
    int testExtraRate;


    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();

        //プレイヤー準備
        ReadyPlayer();

        //UI
        battleGage = battleCanvas.transform.Find("Gage").GetComponent<Slider>();

        //HQ準備
        SetHQ();

        //ユニット準備
        SetSpawnPoint();
        SetSpawnUnit();

        //★テスト
        StartCoroutine(test());
    }

    private void Update()
    {
        //戦況チェック
        UpdateSituation();
    }

    //戦況更新
    private void UpdateSituation()
    {
        int totalUnitCnt = 0;
        foreach (int side in Common.CO.sideArray)
        {
            //ユニット数
            unitCnt[side] = GameObject.FindGameObjectsWithTag(Common.CO.tagUnitArray[side]).Length;
            totalUnitCnt += unitCnt[side];
        }
        //ユニット数割合
        unitCntRate[Common.CO.SIDE_MINE] = Common.Func.GetPer(unitCnt[Common.CO.SIDE_MINE], totalUnitCnt, 50);
        unitCntRate[Common.CO.SIDE_ENEMY] = 100 - unitCntRate[Common.CO.SIDE_MINE];

        //戦況判定
        JugdeSituation();

        //戦況ゲージ
        battleGage.value = unitCntRate[Common.CO.SIDE_MINE] / 100.0f;
    }

    //優劣判定
    private void JugdeSituation()
    {
        //★ユニット数で判断
        foreach (int side in Common.CO.sideArray)
        {
            battleSituation[side] = unitCntRate[side];
        }

        //★HQHPで判断
    }

    //HQ準備
    private void SetHQ()
    {
        //情報取得
        foreach (int side in Common.CO.sideArray)
        {
            GameObject hqObj = GameObject.FindGameObjectWithTag(Common.CO.tagHQArray[side]);
            if (hqObj == null) hqObj = SpawnHQ(side);
            hqInfo[side] = hqObj.transform;
            hqCtrl[side] = hqObj.GetComponent<HQController>();
        }
    }

    //HP生成
    private GameObject SpawnHQ(int side)
    {
        return null;
    }

    //ユニット出現位置取得
    private void SetSpawnPoint()
    {
        foreach (int side in Common.CO.sideArray)
        {
            //通常
            GameObject[] spObjs = GameObject.FindGameObjectsWithTag(Common.CO.tagSpawnPointArray[side]);
            foreach (GameObject spObj in spObjs)
            {
                spawnPoints[side].Add(spObj.transform);
            }
            //援軍
            GameObject[] exSpObjs = GameObject.FindGameObjectsWithTag(Common.CO.tagExSpawnPointArray[side]);
            foreach (GameObject exSpObj in exSpObjs)
            {
                exSpawnPoints[side].Add(exSpObj.transform);
            }
        }
    }

    //★ユニット情報取得
    private void SetSpawnUnit()
    {
        foreach (int side in Common.CO.sideArray)
        {
            if (side == Common.CO.SIDE_MINE)
            {
                spawnUnits[side] = testRespawnUnits;
            }
            else
            {
                spawnUnits[side] = testEnemyRespawnUnits;
            }
            extraUnits[side] = testExtraUnits;
        }
    }

    //プレイヤー準備
    private void ReadyPlayer()
    {
        GameObject playerSP = GameObject.FindGameObjectWithTag(Common.CO.TAG_SP_PLAYER);
        Vector3 pos = (playerSP != null) ? playerSP.transform.position : new Vector3(0, 15, 0);
        Quaternion rot = (playerSP != null) ? playerSP.transform.rotation : Quaternion.identity;
        GameObject player = GameObject.FindGameObjectWithTag(Common.CO.TAG_PLAYER);
        if (player == null) player = Instantiate(Resources.Load<GameObject>("Player"));
        player.transform.position = pos;
        player.transform.rotation = rot;
    }

    //出現位置選択
    private Transform SelectSpawnPoint(int side, bool isExtra = false, bool isRandom = false)
    {
        Transform sp = null;
        if (!isExtra || exSpawnPoints[side].Count == 0)
        {
            //通常
            if (isRandom)
            {
                //ランダム
                int index = UnityEngine.Random.Range(0, spawnPoints[side].Count);
                sp = spawnPoints[side][index];
            }
            else
            {
                //順番
                sp = spawnPoints[side][spawnIndex[side]];
                spawnIndex[side] = (spawnIndex[side] + 1) % spawnPoints[side].Count;
            }
        }
        else
        {
            //援軍
            int exIndex = UnityEngine.Random.Range(0, exSpawnPoints[side].Count);
            sp = exSpawnPoints[side][exIndex];
        }
        return sp;
    }

    //ユニット生成
    private void SpawnUnits(int side, bool isExtra = false)
    {
        //出現位置取得
        Transform sp = SelectSpawnPoint(side, isExtra);
        List<int> units = (isExtra) ? extraUnits[side] : spawnUnits[side];
        foreach (int unit in units)
        {
            SpawnUnit(unit, sp, side);
        }

    }
    //ユニット生成(ランダムポイント)
    private void SpawnUnitsRandom(int side, bool isExtra = false)
    {
        //出現位置取得
        List<int> units = (isExtra) ? extraUnits[side] : spawnUnits[side];
        foreach (int unit in units)
        {
            Transform sp = SelectSpawnPoint(side, isExtra, true);
            SpawnUnit(unit, sp, side);
        }

    }

    //ユニット生成処理
    private GameObject SpawnUnit(int unitNo, Transform spawnPoint, int side)
    {
        //生成
        GameObject unitPref = Resources.Load<GameObject>(Common.CO.RESOURCE_UNIT_DIR + Common.Unit.unitInfo[unitNo]);
        GameObject unit = Instantiate(unitPref, spawnPoint.position, spawnPoint.rotation);

        //情報変更
        unit.tag = Common.CO.tagUnitArray[side];
        Common.Func.SetLayer(unit, Common.CO.layerUnitArray[side], true);
        //★色変え
        Transform unitBody = null;
        Color[] bodyColors = new Color[] { Color.white, Color.red };
        foreach (Transform child in unit.transform)
        {
            if (child.tag == Common.CO.TAG_UNIT_BODY)
            {
                unitBody = child;
                break;
            }
        }
        Material[] mats = unitBody.GetComponent<Renderer>().materials;
        mats[0].color = bodyColors[side];
        unitBody.GetComponent<Renderer>().materials = mats;
        return unit;
    }

    public void Pause()
    {
        isPause = true;
        DialogManager.OpenDialog("一時停止中", "再開", () => ResetPause(), false);
        Time.timeScale = 0;
    }

    public void ResetPause()
    {
        isPause = false;
        Time.timeScale = 1;
    }


    //★テスト用
    IEnumerator test()
    {
        for (;;)
        {
            //ユニット生成
            foreach (int side in Common.CO.sideArray)
            {
                if (unitCnt[side] < testUnitLimit)
                {
                    SpawnUnitsRandom(side, false);
                    if (battleSituation[side] < testExtraRate)
                    {
                        yield return null;
                        SpawnUnits(side, true);
                    }
                }
            }

            testRespawnInterval += testRespawnIntervalAdd;
            yield return new WaitForSeconds(testRespawnInterval);
        }
    }

}
