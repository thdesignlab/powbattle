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
    public bool IsDiscovery(Transform target, int targetSide)
    {
        if (camouflageRange <= 0) return false;
        if (side != Common.CO.SIDE_UNKNOWN && side == targetSide) return false;
        if (Vector3.Distance(myTran.position, target.position) > camouflageRange) return false;
        return true;
    }

    public float GetCamouflageRange()
    {
        return camouflageRange;
    }
}
