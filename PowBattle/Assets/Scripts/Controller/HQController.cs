using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class HQController : UnitController
{
    [SerializeField]
    protected bool isMain;
    [SerializeField]
    protected float alertDistance;
    [SerializeField]
    protected int alertUnitNum;
    [SerializeField]
    protected float alertInterval;
    protected float leftAleartInterval;

    protected override void Update()
    {
        base.Update();
        if (leftAleartInterval > 0) leftAleartInterval -= Time.deltaTime;
    }

    //被弾
    public override int Hit(int damage, Vector3 impact, Transform enemyTran)
    {
        int d = base.Hit(damage, impact, enemyTran);

        Aleart(enemyTran);

        return d;
    }

    //救援要請
    protected void Aleart(Transform enemyTran)
    {
        if (leftAleartInterval > 0 || enemyTran == null) return;

        int leftAleartNum = alertUnitNum;
        List<Transform> targets = BattleManager.Instance.GetUnitList(mySide);
        Dictionary<int, float> targetDistanceList = new Dictionary<int, float>();
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null) continue;
            float distance = Vector3.Distance(myTran.position, targets[i].position);
            if (distance < alertDistance)
            {
                targetDistanceList.Add(i, distance);
            }
        }
        //距離近い順にソート
        var sortList = targetDistanceList.OrderBy((x) => x.Value);
        int cnt = 0;
        foreach (var v in sortList)
        {
            targets[v.Key].GetComponent<UnitController>().SetTarget(enemyTran, 10.0f);
            if (++cnt >= alertUnitNum) break;
        }
        leftAleartInterval = alertInterval;
    }

    //本陣判定
    public bool IsMainHQ()
    {
        return isMain;
    }
}
