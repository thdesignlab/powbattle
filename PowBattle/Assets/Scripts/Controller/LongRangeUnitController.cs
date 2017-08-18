using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class LongRangeUnitController : ActiveUnitController
{
    [SerializeField]
    protected bool isArtillery;
    [SerializeField]
    protected bool isNotNeedLockOn;


    //移動
    protected override void Move()
    {
        if (isArtillery) return;
        base.Move();
    }

    //目視チェック
    protected override bool IsDiscoveryTarget(Transform target, float range = 0)
    {
        if (isNotNeedLockOn) return true;
        return base.IsDiscoveryTarget(target, range);
    }

    //ターゲット切り替え判定
    protected override void JugdeChangeTarget(Transform t)
    {
        if (t == null) return;

        float distance = Vector3.Distance(myTran.position, t.position);
        if (distance <= 5.0f) isArtillery = false;

        if (targetTran == null)
        {
            SetTarget(t);
        }
        else if (targetTran.tag == targetHQTag || targetTran.tag == Common.CO.TAG_BREAK_OBSTACLE)
        {
            if (IsDiscoveryTarget(t)) SetTarget(t);
        }
        else
        {
            if (targetDistance > distance) SetTarget(t);
        }
    }
}
