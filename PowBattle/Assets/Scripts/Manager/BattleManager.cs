using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.AI;

public class BattleManager : SingletonMonoBehaviour<BattleManager>
{
    [SerializeField]
    private Canvas battleCanvas;
    //[SerializeField]
    //private Slider battleGage;
    [SerializeField]
    private List<Slider> situationGages;
    [SerializeField]
    private List<int> extraRateList;
    private int callExtraDiff = 3;
    private GameObject player;

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
    //[HideInInspector]
    //public List<int> battleSituation = new List<int>() { 50, 50 };

    private List<List<int>> spawnUnits = new List<List<int>>() { new List<int>() { }, new List<int>() { } };
    private List<List<int>> extraUnits = new List<List<int>>() { new List<int>() { }, new List<int>() { } };

    [HideInInspector]
    public List<Transform> breakableObstacles = new List<Transform>();
    [HideInInspector]
    public List<List<Transform>> unitList = new List<List<Transform>> { new List<Transform>() { }, new List<Transform>() { } };


    //test用
    [SerializeField]
    int testUnitLimit;
    [SerializeField]
    int testRespawnCount;
    [SerializeField]
    int testRespawnInterval;
    [SerializeField]
    int testRespawnIntervalAdd;
    [SerializeField]
    List<int> testRespawnUnits;
    [SerializeField]
    List<int> testEnemyRespawnUnits;
    List<int> testExtraUnits = new List<int>() { 0, 0,  1, 1, 1, 1, 1, 2 };


    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();

        //プレイヤー準備
        ReadyPlayer();

        //HQ準備
        SetHQ();

        //マップオブジェクト
        SetMapObstacle();

        //ユニット準備
        SetSpawnPoint();
        SetSpawnUnit();
        UpdateSituation();
    }

    private void Start()
    {
        //援軍チェックルーチン
        StartCoroutine(CheckExtra());

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

            //HQゲージ
            situationGages[side].value = hqCtrl[side].GetHpRate();
        }
        //ユニット数割合
        unitCntRate[Common.CO.SIDE_MINE] = Common.Func.GetPer(unitCnt[Common.CO.SIDE_MINE], totalUnitCnt, 50);
        unitCntRate[Common.CO.SIDE_ENEMY] = 100 - unitCntRate[Common.CO.SIDE_MINE];

        //戦況ゲージ
        //battleGage.value = unitCntRate[Common.CO.SIDE_MINE] / 100.0f;
    }

    //敵陣営No取得
    public int GetEnemySide(int mySide)
    {
        int enemySide = Common.CO.SIDE_UNKNOWN;
        if (mySide != Common.CO.SIDE_UNKNOWN)
        {
            enemySide = (mySide + 1) % 2;
        }
        return enemySide;
    }

    //優勢判定(HQHP)
    public bool JugdeBattleSituation(int mySide)
    {
        int enemySide = GetEnemySide(mySide);
        if (hqCtrl[mySide] == null) return false;
        if (hqCtrl[enemySide] == null) return true;
        return (hqCtrl[mySide].GetHpRate() > hqCtrl[enemySide].GetHpRate());
    }

    //優勢判定(ユニット数)
    public bool JugdeUnitSituation(int mySide)
    {
        int enemySide = GetEnemySide(mySide);
        return (unitCnt[mySide] > unitCnt[enemySide]);
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

    //HQ生成
    private GameObject SpawnHQ(int side)
    {
        return null;
    }

    //マップオブジェクト取得
    private void SetMapObstacle()
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(Common.CO.TAG_BREAK_OBSTACLE);
        foreach (GameObject obstacle in obstacles)
        {
            breakableObstacles.Add(obstacle.transform);
        }
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
                for (int i = 0; i <testRespawnUnits.Count; i++)
                {
                    for (int j = 0; j < testRespawnUnits[i]; j++)
                    {
                        spawnUnits[side].Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < testEnemyRespawnUnits.Count; i++)
                {
                    for (int j = 0; j < testEnemyRespawnUnits[i]; j++)
                    {
                        spawnUnits[side].Add(i);
                    }
                }
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
        player = GameObject.FindGameObjectWithTag(Common.CO.TAG_PLAYER);
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
            GameObject unitObj = SpawnUnit(unit, sp, side);
            if (isExtra)
            {
                //援軍バフ
                UnitController unitCtrl = unitObj.GetComponent<UnitController>();
                unitCtrl.AttackEffect(50, 15);
                unitCtrl.DefenceEffect(75, 30);
            }
        }

    }

    //ユニット生成処理
    private GameObject SpawnUnit(int unitNo, Transform spawnPoint, int side)
    {
        //生成
        GameObject unitPref = Resources.Load<GameObject>(Common.CO.RESOURCE_UNIT_DIR + Common.Unit.unitInfo[unitNo]);
        GameObject unit = Instantiate(unitPref, PickAroundPosition(spawnPoint.position), spawnPoint.rotation);
        unitList[side].Add(unit.transform);

        //情報変更
        unit.tag = Common.CO.tagUnitArray[side];
        Common.Func.SetLayer(unit, Common.CO.layerUnitArray[side], false);
        //★ボディ色変え
        Color[] bodyColors = new Color[] { Color.white, Color.red };
        Transform unitBody = Common.Func.SearchChildTag(unit.transform, Common.CO.TAG_UNIT_BODY);
        Renderer unitRenderer = unitBody.GetComponent<Renderer>();
        if (unitRenderer != null)
        {
            Material[] mats = unitBody.GetComponent<Renderer>().materials;
            mats[0].color = bodyColors[side];
            unitBody.GetComponent<Renderer>().materials = mats;
        }
        ////★HPGage色変え
        //Color[] gageColors = new Color[] { Color.cyan, Color.red };
        //unit.GetComponent<UnitController>().SetHpGageColor(gageColors[side]);
        return unit;
    }

    //周辺ポイント決定
    private Vector3 PickAroundPosition(Vector3 basePos, float range = 1.0f)
    {
        Vector3 pos = basePos;
        NavMeshHit hit;
        Vector3 randomPoint = basePos + UnityEngine.Random.insideUnitSphere * range;
        //Debug.Log(basePos + " >> " + randomPoint);
        if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
        {
            pos = hit.position;
        }
        pos.y = basePos.y;
        return pos;
    }


    //ユニット削除
    public void RemoveUnit(int side, Transform tran)
    {
        if (tran == null || side == Common.CO.SIDE_UNKNOWN)
        {
            Debug.Log(side +" >> "+tran);
            return;
        }
        unitList[side].Remove(tran);
    }

    //援軍チェックルーチン
    IEnumerator CheckExtra()
    {
        for (;;)
        {
            yield return new WaitForSeconds(5.0f);
            CallExtraUnits();
        }
    }

    //援軍生成
    private void CallExtraUnits()
    {
        if (extraRateList.Count == 0) return;

        int mine = (int)situationGages[Common.CO.SIDE_MINE].value;
        int enemy = (int)situationGages[Common.CO.SIDE_ENEMY].value;
        List<int> callExtraSides = new List<int>();

        if (mine <= extraRateList[0])
        {
            callExtraSides.Add(Common.CO.SIDE_MINE);
            if (enemy - mine <= callExtraDiff) callExtraSides.Add(Common.CO.SIDE_ENEMY);
        }
        else if (enemy <= extraRateList[0])
        {
            callExtraSides.Add(Common.CO.SIDE_ENEMY);
            if (mine - enemy <= callExtraDiff) callExtraSides.Add(Common.CO.SIDE_MINE);
        }

        foreach (int side in callExtraSides)
        {
            SpawnUnitsRandom(side, true);
        }
        if (callExtraSides.Count > 0) extraRateList.RemoveAt(0);
    }

    //ユニットリスト取得
    public List<Transform> GetUnitList(int side)
    {
        List<Transform> list = new List<Transform>();
        if (side == Common.CO.SIDE_UNKNOWN)
        {
            list = unitList[Common.CO.SIDE_MINE];
            list.AddRange(unitList[Common.CO.SIDE_ENEMY]);
        }
        else
        {
            list = unitList[side];
        }
        return list;
    }

    public GameObject GetBattleCanvas()
    {
        return battleCanvas.gameObject;
    }

    public GameObject GetPlayer()
    {
        return player;
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
        for (int i = 0; i < testRespawnCount; i++)
        {
            //ユニット生成
            foreach (int side in Common.CO.sideArray)
            {
                if (unitCnt[side] < testUnitLimit)
                {
                    SpawnUnitsRandom(side, false);
                }
            }

            testRespawnInterval += testRespawnIntervalAdd;
            yield return new WaitForSeconds(testRespawnInterval);
        }
    }
}
