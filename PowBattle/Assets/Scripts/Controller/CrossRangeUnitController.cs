using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CrossRangeUnitController : ActiveUnitController
{
    //ターゲットサーチ
    protected override void Search()
    {
        //再索敵チェック

        //敵を探す
        List<Transform> targets = BattleManager.Instance.GetUnitList(enemySide);

        Transform tmpTarget = null;
        float tmpDistance = 0;
        bool isWithinRange = false;
        foreach (Transform target in targets)
        {
            if (target == null) continue;

            //索敵範囲判定
            float distance = Vector3.Distance(myTran.position, target.position);
            if (distance > searchRange) continue;

            //目視判定
            if (!IsDiscoveryTarget(target, searchRange)) continue;

            //到達可否判定
            if (!IsEnabledPath(target)) continue;
            
            //射程内判定
            if (weaponCtrl.IsWithinRange(target, distance))
            {
                //決定
                tmpTarget = target;
                isWithinRange = true;
                break;
            }
            else
            {
                //仮置き
                if (tmpDistance == 0 || tmpDistance > distance)
                {
                    tmpTarget = target;
                    tmpDistance = distance;
                }
            }
        }
        if (tmpTarget != null) SetTarget(tmpTarget);

        //敵以外をターゲット
        if (targetTran == null || !isWithinRange) SearchOther();
    }

    //再索敵判定
    protected override bool IsResearch()
    {
        if (leftForceTargetTime > 0) return false;

        if (targetTran != null && targetTran.tag == targetTag)
        {
            if (IsEnabledPath(targetTran))
            {
                //再ターゲット
                SetTarget(targetTran);
                return false;
            }
        }
        return true;
    }

    //ターゲットへ到達可否判定
    protected override bool IsEnabledPath(Transform target)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(target.position, path);
        return (path.status == NavMeshPathStatus.PathComplete);
    }


    //攻撃判定
    protected override bool JudgeAttack()
    {
        //武器射程取得
        float attackRangeMax = weaponCtrl.GetMaxRange();

        SetLockOn(IsDiscoveryTarget(targetTran, attackRangeMax));
        if (!isLockOn) return false;

        bool atk = base.JudgeAttack();
        if (atk) 
        {
            coolTime = weaponCtrl.GetMoveDelay();
            agent.isStopped = true;
        }
        return atk;
    }


    //ターゲット切り替え判定
    protected override void JugdeChangeTarget(Transform t)
    {
        if (t == null) return;
        if (leftForceTargetTime > 0) return;

        if (targetTran == null)
        {
            SetTarget(t);
        }
        else if (targetTran.tag == targetHQTag || targetTran.tag == Common.CO.TAG_BREAK_OBSTACLE)
        {
            if (IsEnabledPath(t)) SetTarget(t);
        }
        else
        {
            if (targetDistance > Vector3.Distance(myTran.position, t.position)) SetTarget(t);
        }
    }


    //目視チェック
    protected override bool IsDiscoveryTarget(Transform target, float range = 0)
    {
        if (target == null) return false;
        if (range <= 0) range = searchRange;
        Vector3 basePos = myTran.position + myTran.forward + myTran.up;
        float attackRange = weaponCtrl.GetMaxRange();

        bool ret = false;
        if (target == targetTran && targetDistance <= attackRange)
        {
            ret = true;
        }
        else if (Vector3.Distance(basePos, target.position) <= attackRange)
        {
            ret = true;
        }
        else
        {
            RaycastHit hit;
            Ray ray = new Ray(basePos, target.position - basePos);
            //Debug.DrawRay(basePos, target.position - basePos);
            if (Physics.SphereCast(ray, 0.2f, out hit, range, targetLayer))
            {
                if (hit.transform == target) ret = true;
            }
        }
        return ret;
    }
}
