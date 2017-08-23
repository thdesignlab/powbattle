using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class LongRangeUnitController : ActiveUnitController
{
    [SerializeField]
    protected bool isNotNeedLockOn;
    protected bool isArtillery;
    protected bool isArtilleryUnit = false;

    //移動
    protected override void Move()
    {
        if (isArtillery) return;
        base.Move();
    }

    protected override void SearchOther()
    {
        if (isArtillery) return;

        //HQ
        SearchHQ(true);

        //破壊可能オブジェクト
        if (!IsDiscoveryTarget(targetTran, searchRange)) SearchObstacle();
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
        if (distance <= 5.0f) SetArtillery(false);

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

    protected override void SetLockOn(bool flg)
    {
        base.SetLockOn(flg);

        if (isArtillery || isArtilleryUnit)
        {
            agent.isStopped = flg;
            if (flg)
            {
                agent.destination = targetTran.position;
                isArtillery = true;
            }
        }
    }

    //固定砲台判定
    protected override void JudgeArtillery()
    {
        Ray ray = new Ray(myTran.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray,out hit, 5.0f, LayerMask.GetMask(Common.CO.LAYER_OBSTACLE)))
        {
            if (hit.transform.tag == Common.CO.TAG_ARTILLERY_OBSTACLE)
            {
                SetArtillery(true);
                isArtilleryUnit = true;
            }
        }
    }

    protected void SetArtillery(bool flg)
    {
        isArtillery = flg;
    }
}
