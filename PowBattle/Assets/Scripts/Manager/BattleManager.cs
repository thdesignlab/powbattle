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

    private GameObject[] spawnPointsMine;
    private GameObject[] spawnPointsEnemy;
    private int spawnIndexMine = 0;
    private int spawnIndexEnemy = 0;
    private int unitCnt = 0;
    private int enemyCnt = 0;

    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();

        battleGage = battleCanvas.transform.Find("Gage").GetComponent<Slider>();

        StartCoroutine(test());
    }

    private void Update()
    {
        unitCnt = GameObject.FindGameObjectsWithTag(Common.CO.TAG_UNIT).Length;
        enemyCnt = GameObject.FindGameObjectsWithTag(Common.CO.TAG_ENEMY).Length;

        if (unitCnt == 0 && enemyCnt == 0)
        {
            battleGage.value = 0.5f;
        }
        else
        {
            battleGage.value = unitCnt / (float)(unitCnt + enemyCnt);
        }
    }

    IEnumerator test()
    {
        int limit = 30;
        for (;;)
        {
            //自軍生成
            if (unitCnt < limit)
            {
                spawnPointsMine = GameObject.FindGameObjectsWithTag(Common.CO.TAG_SP_MINE);
                List<int> mineUnits = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                SpawnUnits(mineUnits, spawnPointsMine, true);
            }

            //敵軍生成
            if (enemyCnt < limit)
            {
                spawnPointsEnemy = GameObject.FindGameObjectsWithTag(Common.CO.TAG_SP_ENEMY);
                List<int> enemyUnits = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                SpawnUnits(enemyUnits, spawnPointsEnemy, false);
            }

            yield return new WaitForSeconds(4);
        }
    }

    private void SpawnUnits(List<int> unitNoList, GameObject[] spawnPoints, bool isMine)
    {
        int index = isMine ? spawnIndexMine : spawnIndexEnemy;
        foreach (int unitNo in unitNoList)
        {
            index = UnityEngine.Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[index].transform;
            SpawnUnit(unitNo, spawnPoint, isMine);
        }
    }
    private GameObject SpawnUnit(int unitNo, Transform spawnPoint, bool isMine)
    {
        GameObject unitPref = Resources.Load<GameObject>(Common.CO.RESOURCE_UNIT_DIR + Common.Unit.unitInfo[unitNo]);
        GameObject unit = Instantiate<GameObject>(unitPref, spawnPoint.position, spawnPoint.rotation);
        if (!isMine)
        {
            unit.tag = Common.CO.TAG_ENEMY;
            Material[] mats = unit.transform.Find("Body").GetComponent<Renderer>().materials;
            mats[0].color = Color.red;
            unit.transform.Find("Body").GetComponent<Renderer>().materials = mats;
        }
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


}
