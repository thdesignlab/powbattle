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

        //プレイヤー準備
        ReadyPlayer();

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

    [SerializeField]
    int unitLimit;
    [SerializeField]
    int enemyLimit;
    [SerializeField]
    int respawn;
    [SerializeField]
    int respawnAdd;
    IEnumerator test()
    {
        int rand = 1;
        for (;;)
        {
            //自軍生成
            if (unitCnt < unitLimit)
            {
                spawnPointsMine = GameObject.FindGameObjectsWithTag(Common.CO.TAG_SP_MINE);
                rand = (UnityEngine.Random.Range(0, 100) > 95) ? 2 : 1 ;
                List<int> mineUnits = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1};
                SpawnUnits(mineUnits, spawnPointsMine, true);
            }

            //敵軍生成
            if (enemyCnt < enemyLimit)
            {
                spawnPointsEnemy = GameObject.FindGameObjectsWithTag(Common.CO.TAG_SP_ENEMY);
                rand = (UnityEngine.Random.Range(0, 100) > 0) ? 2 : 1;
                List<int> enemyUnits = new List<int> { 1, 1, 1, rand };
                SpawnUnits(enemyUnits, spawnPointsEnemy, false);
            }

            respawn += respawnAdd;
            yield return new WaitForSeconds(respawn);
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
        GameObject unit = Instantiate(unitPref, spawnPoint.position, spawnPoint.rotation);
        if (!isMine)
        {
            unit.tag = Common.CO.TAG_ENEMY;
            Common.Func.SetLayer(unit, Common.CO.LAYER_ENEMY, true);

            Transform unitBody = null;
            foreach (Transform child in unit.transform)
            {
                if (child.tag == Common.CO.TAG_UNIT_BODY)
                {
                    unitBody = child;
                }
            }
            Material[] mats = unitBody.GetComponent<Renderer>().materials;
            mats[0].color = Color.red;
            unitBody.GetComponent<Renderer>().materials = mats;
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
