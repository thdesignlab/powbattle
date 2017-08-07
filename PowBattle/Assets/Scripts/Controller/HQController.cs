using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class HQController : UnitController
{
    [SerializeField]
    protected float aleartDistance;
    [SerializeField]
    protected int aleartNum;
    [SerializeField]
    protected float aleartInterval;
    protected float leftAleartInterval;

    protected override void Update()
    {
        base.Update();
        if (leftAleartInterval > 0) leftAleartInterval -= Time.deltaTime;
    }

    public override int Hit(int damage, Vector3 impact, Transform enemyTran)
    {
        int d = base.Hit(damage, impact, enemyTran);

        if (enemyTran != null) Aleart(enemyTran);

        return d;
    }

    protected void Aleart(Transform enemyTran)
    {
        if (leftAleartInterval > 0) return;

        int leftAleartNum = aleartNum;
        List<Transform> targets = BattleManager.Instance.GetUnitList(mySide);
        Dictionary<int, float> targetDistanceList = new Dictionary<int, float>();
        for (int i = 0; i < targets.Count; i++)
        {
            float distance = Vector3.Distance(myTran.position, targets[i].position);
            if (distance < aleartDistance)
            {
                targetDistanceList.Add(i, distance);
            }
        }
        //距離近い順にソート
        var sortList = targetDistanceList.OrderBy((x) => x.Value);
        int cnt = 0;
        foreach (var v in sortList)
        {
            targets[v.Key].GetComponent<UnitController>().SetForceTarget(enemyTran, 10.0f);
            if (++cnt >= aleartNum) break;
        }
        leftAleartInterval = aleartInterval;
    }
}
