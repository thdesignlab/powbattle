using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class ObstacleController : UnitController
{
    [SerializeField]
    protected int side;
    [SerializeField]
    protected float camouflageRange;
    [SerializeField]
    protected UnityEvent breakAction;

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
        return side;
    }

    protected override int GetMySide()
    {
        return GetSide();
    }

    public int GetSide()
    {
        return side;
    }

    protected override void Dead()
    {
        if (breakAction != null) breakAction.Invoke();
        base.Dead();
    }
}
