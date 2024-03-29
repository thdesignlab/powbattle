﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.AI;

public class BattleManager : SingletonMonoBehaviour<BattleManager>
{
    //UI
    private Transform battleCanvasTran;
    private List<Slider> situationGages = new List<Slider>() { null, null};
    private GameObject textLine;
    private GameObject textWin;
    private GameObject textLose;
    private GameObject spawnEffect;

    //援軍設定
    [SerializeField]
    private List<int> extraRateList;
    private int CALL_EXTRA_DIFF = 3;

    //ステージ情報
    private List<List<Transform>> spawnPoints = new List<List<Transform>>() { new List<Transform>() { }, new List<Transform>() { } };
    private List<List<Transform>> exSpawnPoints = new List<List<Transform>>() { new List<Transform>() { }, new List<Transform>() { } };
    private List<int> spawnIndex = new List<int>() { 0, 0 };
    [HideInInspector]
    public List<ObstacleController> obstacleCtrls = new List<ObstacleController>();

    //バトル情報
    private List<int> unitCnt = new List<int>() { 0, 0 };
    [HideInInspector]
    public List<int> unitCntRate = new List<int>() { 50, 50 };
    private List<HQController> mainHqCtrl = new List<HQController>() { null, null };
    private List<List<HQController>> subHqCtrlList = new List<List<HQController>>() { new List<HQController>(), new List<HQController>() };
    [HideInInspector]
    public List<List<Transform>> unitList = new List<List<Transform>> { new List<Transform>() { }, new List<Transform>() { } };
    [SerializeField]
    protected List<int> unitDeadDamageRate = new List<int>() { 0, 0 };
    private const float SPAWN_UNIT_INTERVAL = 0.3f;

    //プレイヤー情報
    private GameObject player;
    private List<List<int>> spawnUnits = new List<List<int>>() { new List<int>() { }, new List<int>() { } };
    private List<List<int>> extraUnits = new List<List<int>>() { new List<int>() { }, new List<int>() { } };

    //バトル状況FLG
    [HideInInspector]
    public bool isBattleStart = false;
    [HideInInspector]
    public bool isBattleEnd = false;

    //test用
    [HideInInspector]
    public bool isVisibleTarget = false;
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
    List<int> testExtraUnits = new List<int>() { 0, 1, 2, 3, 4 };


    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();

        //プレイヤー準備
        ReadyPlayer();

        //HQ準備
        SetHQ();

        //UI準備
        SetUI();

        //マップオブジェクト
        SetMapObstacle();

        //ユニット準備
        SetSpawnPoint();
        SetSpawnUnit();
        UpdateSituation();

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        isBattleStart = true;

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

    private void SetUI()
    {
        battleCanvasTran = GameObject.Find("BattleCanvas").transform;
        situationGages[Common.CO.SIDE_MINE] = battleCanvasTran.Find("HQGage/Mine").GetComponent<Slider>();
        situationGages[Common.CO.SIDE_ENEMY] = battleCanvasTran.Find("HQGage/Enemy").GetComponent<Slider>();
        textLine = battleCanvasTran.Find("TextLine").gameObject;
        textLine.SetActive(false);
        textWin = textLine.transform.Find("Win").gameObject;
        textLose = textLine.transform.Find("Lose").gameObject;

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
            situationGages[side].value = mainHqCtrl[side].GetHpRate();
            if (isBattleStart && situationGages[side].value <= 0)
            {
                BattleResult(side);
            }
        }
        //ユニット数割合
        unitCntRate[Common.CO.SIDE_MINE] = Common.Func.GetPer(unitCnt[Common.CO.SIDE_MINE], totalUnitCnt, 50);
        unitCntRate[Common.CO.SIDE_ENEMY] = 100 - unitCntRate[Common.CO.SIDE_MINE];

    }

    //バトル終了
    private void BattleResult(int loseSide)
    {
        if (isBattleEnd) return;

        isBattleEnd = true;
        if (loseSide == Common.CO.SIDE_MINE)
        {
            //負け
            textWin.SetActive(false);
            textLose.SetActive(true);
        }
        else
        {
            //勝ち
            textWin.SetActive(true);
            textLose.SetActive(false);
        }
        textLine.gameObject.SetActive(true);
        StartCoroutine(ReturnMainScene());
    }
    IEnumerator ReturnMainScene()
    {
        yield return new WaitForSeconds(10.0f);
        ScreenManager.Instance.SceneLoad(Common.CO.SCENE_MAIN);
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
    public bool IsSuperioritySituation(int mySide)
    {
        int enemySide = GetEnemySide(mySide);
        if (mainHqCtrl[mySide] == null) return false;
        if (mainHqCtrl[enemySide] == null) return true;
        return (mainHqCtrl[mySide].GetHpRate() > mainHqCtrl[enemySide].GetHpRate());
    }

    //優勢判定(ユニット数)
    public bool IsSuperiorityUnit(int mySide)
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
            GameObject[] hqObjs = GameObject.FindGameObjectsWithTag(Common.CO.tagHQArray[side]);
            foreach (GameObject hqObj in hqObjs)
            {
                HQController ctrl = hqObj.GetComponent<HQController>();
                if (ctrl.IsMainHQ())
                {
                    mainHqCtrl[side] = ctrl;
                }
                else
                {
                    subHqCtrlList[side].Add(ctrl);
                }
            }
        }
    }

    //HQ選択
    public HQController SelectTargetHQ(int side, Transform t)
    {
        HQController ctrl = mainHqCtrl[side];

        float distance = 0;
        foreach (HQController subHqCtrl in subHqCtrlList[side])
        {
            if (subHqCtrl == null) continue;
            float tmpDistance = Vector3.Distance(subHqCtrl.transform.position, t.position);
            if (distance == 0 || tmpDistance < distance)
            {
                distance = tmpDistance;
                ctrl = subHqCtrl;
            }
        }
        return (ctrl == null) ? null : ctrl;
    }
    public Transform SelectTargetHQTran(int side, Transform t)
    {
        HQController ctrl = SelectTargetHQ(side, t);
        return (ctrl != null) ? ctrl.transform : null;
    }

    //ユニット死亡ダメージ
    public void DeadDamage(int side, Transform t)
    {
        if (unitDeadDamageRate[side] <= 0) return;

        HQController target = SelectTargetHQ(side, t);
        if (target == null) return;

        int maxHP = target.GetMaxHp();
        float situationRate = (IsSuperioritySituation(side)) ? 1.0f : 0.8f;
        int damage = (int)(maxHP * (unitDeadDamageRate[side] / 100.0f) * situationRate);
        if (damage <= 0) damage = 1;
        target.Hit(damage, null);
    }

    //マップオブジェクト取得
    private void SetMapObstacle()
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(Common.CO.TAG_BREAK_OBSTACLE);
        foreach (GameObject obstacle in obstacles)
        {
            ObstacleController obstacleCtrl = obstacle.GetComponent<ObstacleController>();
            if (obstacleCtrl.GetCamouflageRange() <= 0) continue;
            obstacleCtrls.Add(obstacleCtrl);
        }
    }

    //ユニット出現位置取得
    private void SetSpawnPoint()
    {
        spawnEffect = Common.Resource.GetEffectResource("SpawnEffect");
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
        if (player == null) player = Instantiate(Common.Resource.GetPlayerResource("Player"));
        player.transform.position = pos;
        player.transform.rotation = rot;
    }

    //出現位置選択
    private Transform SelectSpawnPoint(int side, bool isExtra = false)
    {
        List<Transform> points = new List<Transform>();
        int index = 0;
        Transform sp = null;
        if (!isExtra || exSpawnPoints[side].Count == 0)
        {
            //通常
            subHqCtrlList[side].RemoveAll(a => a == null);
            spawnPoints[side].RemoveAll(a => a == null);
            if (subHqCtrlList[side].Count > 0)
            {
                //サブHQから取得
                foreach (HQController subHqCtrl in subHqCtrlList[side])
                {
                    points.Add(subHqCtrl.transform);
                }
            }
            else if (spawnPoints[side].Count > 0)
            {
                //出現ポイントから取得
                points = spawnPoints[side];
            }

            //インデックス
            if (points.Count > 0)
            {
                spawnIndex[side] = (spawnIndex[side] + 1) % points.Count;
                index = spawnIndex[side];
            }
        }
        else
        {
            //援軍
            exSpawnPoints[side].RemoveAll(a => a == null);
            if (exSpawnPoints[side].Count > 0)
            {
                //出現ポイントから取得
                points = exSpawnPoints[side];
            }
            else if (unitList[side].Count > 0)
            {
                //自軍付近
                points = unitList[side];
            }

            //インデックス
            if (points.Count > 0) index = UnityEngine.Random.Range(0, points.Count);
        }

        //ポイント決定
        if(points.Count <= index || points[index] == null)
        {
            //HQ
            sp = mainHqCtrl[side].transform;
        }
        else
        {
            if (!points[index].gameObject.activeSelf)
            {
                return SelectSpawnPoint(side, isExtra);
            }
            sp = points[index];
        }
        return sp;
    }

    //ユニット生成
    IEnumerator SpawnUnits(int side, bool isExtra = false)
    {
        //生成ユニット
        List<int> units = (isExtra) ? extraUnits[side] : spawnUnits[side];
        //生成位置
        Transform sp = SelectSpawnPoint(side, isExtra);
        Vector3 spawnPos = sp.position;
        if (IsHq(sp.tag)) spawnPos += sp.forward * 2.0f;
        Quaternion spawnRot = sp.rotation;

        foreach (int unit in units)
        {
            SpawnUnit(unit, spawnPos, spawnRot, side, isExtra);
            yield return new WaitForSeconds(SPAWN_UNIT_INTERVAL);
        }
    }

    //対象がHQか判定
    private bool IsHq(Transform t)
    {
        if (t == null) return false;
        return IsHq(t.tag);
    }
    private bool IsHq(string tag)
    {
        bool flg = false;
        switch (tag)
        {
            case Common.CO.TAG_HQ:
            case Common.CO.TAG_ENEMY_HQ:
                flg = true;
                break;
        }
        return flg;
    }

    //ユニット生成処理
    private GameObject SpawnUnit(int unitNo, Vector3 spawnPos, int side, bool isExtra = false)
    {
        return SpawnUnit(unitNo, spawnPos, Quaternion.identity, side, isExtra);
    }

    private GameObject SpawnUnit(int unitNo, Vector3 spawnPos, Quaternion spawnRot, int side, bool isExtra = false)
    {
        //生成
        Vector3 pos = PickAroundPosition(spawnPos);
        if (spawnEffect != null) Instantiate(spawnEffect, pos, spawnRot);
        GameObject unitPref = Common.Resource.GetUnitResource(Common.Unit.unitInfo[unitNo]);
        GameObject unit = Instantiate(unitPref, pos, spawnRot);
        unitList[side].Add(unit.transform);

        //情報変更
        UnitController unitCtrl = unit.GetComponent<UnitController>();
        unitCtrl.SetSide(side);
        if (isExtra) unitCtrl.SetExtraBuff();

        //★ボディ色変え
        Color[] bodyColors = new Color[] { Color.white, Color.red };
        Transform unitBody = Common.Func.SearchChildTag(unit.transform, "UnitBody");
        if (unitBody != null)
        {
            Renderer unitRenderer = unitBody.GetComponent<Renderer>();
            if (unitRenderer != null)
            {
                Material[] mats = unitBody.GetComponent<Renderer>().materials;
                mats[0].color = bodyColors[side];
                unitBody.GetComponent<Renderer>().materials = mats;
            }
        }

        return unit;
    }

    //周辺ポイント決定
    private Vector3 PickAroundPosition(Vector3 basePos, float range = 2.0f)
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

    //★援軍生成
    private void CallExtraUnits()
    {
        if (extraRateList.Count == 0) return;

        int mine = (int)situationGages[Common.CO.SIDE_MINE].value;
        int enemy = (int)situationGages[Common.CO.SIDE_ENEMY].value;
        List<int> callExtraSides = new List<int>();

        if (mine <= extraRateList[0])
        {
            callExtraSides.Add(Common.CO.SIDE_MINE);
            if (enemy - mine <= CALL_EXTRA_DIFF) callExtraSides.Add(Common.CO.SIDE_ENEMY);
        }
        else if (enemy <= extraRateList[0])
        {
            callExtraSides.Add(Common.CO.SIDE_ENEMY);
            if (mine - enemy <= CALL_EXTRA_DIFF) callExtraSides.Add(Common.CO.SIDE_MINE);
        }

        foreach (int side in callExtraSides)
        {
            StartCoroutine(SpawnUnits(side, true));
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

    public GameObject GetPlayer()
    {
        return player;
    }

    public void Pause()
    {
        //DialogManager.OpenDialog("一時停止中", "再開", () => ResetPause(), false);
        Time.timeScale = 0;
    }

    public void ResetPause()
    {
        Time.timeScale = 1;
    }


    //★テスト用
    IEnumerator test()
    {
        for (int i = 0; i < testRespawnCount; i++)
        {
            if (isBattleEnd) break;

            //ユニット生成
            for (int j = 0; j < 2; j++)
            {
                foreach (int side in Common.CO.sideArray)
                {
                    if (unitCnt[side] < testUnitLimit)
                    {
                        StartCoroutine(SpawnUnits(side, false));
                    }
                }
                yield return new WaitForSeconds(1.0f);
            }

            testRespawnInterval += testRespawnIntervalAdd;
            yield return new WaitForSeconds(testRespawnInterval);
        }
    }
}
