using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObstacleController : UnitController
{
    [SerializeField]
    protected int side;
    [SerializeField]
    protected float camouflageRange;

    //初期処理
    protected override void Init()
    {
        return;
    }

    //発見判定
    public bool IsDiscovery(Transform target, int targetSide, float attackRange = 0)
    {
        if (camouflageRange <= 0) return false;
        if (side != Common.CO.SIDE_UNKNOWN && side == targetSide) return false;
        float distance = Vector3.Distance(myTran.position, target.position);
        if (distance > camouflageRange || distance > attackRange) return false;
        return true;
    }

    public float GetCamouflageRange()
    {
        return camouflageRange;
    }
}
