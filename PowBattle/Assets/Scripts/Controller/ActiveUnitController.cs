using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ActiveUnitController : UnitController
{
    private NavMeshAgent _agent;
    protected NavMeshAgent agent
    {
        get { return (_agent) ? _agent : _agent = GetComponent<NavMeshAgent>(); }
    }

    protected float coolTime = 0;
    protected float leftRotateTime = 0;
    protected const float LOCK_ON_ROTATE_INTERVAL = 1.5f;

    protected override void Awake()
    {
        base.Awake();
        agent.enabled = isActive;
    }

    protected override void Update()
    {
        if (!isActive) return;

        if (nowHP <= 0 || BattleManager.Instance.isBattleEnd)
        {
            agent.isStopped = true;
            return;
        }

        base.Update();

        if (coolTime > 0) coolTime -= Time.deltaTime;

        //敵の方を向く
        if (leftRotateTime > 0) leftRotateTime -= Time.deltaTime;
        if (isLockOn && leftRotateTime <= 0)
        {
            if (LookTarget(targetTran, agent.angularSpeed, new Vector3(1, 0, 1))) leftRotateTime = LOCK_ON_ROTATE_INTERVAL;
        }

        //移動モーション
        MoveMotion();

        if (HQTran == null) SearchHQ();
    }

    //行動
    protected override void Action()
    {
        if (!IsEffectiveAgent()) return;

        base.Action();

        //移動
        Move();
    }

    //移動モーション
    protected void MoveMotion()
    {
        if (motionCtrl == null) return;
        motionCtrl.Run(agent.velocity != Vector3.zero);
    }

    //ターゲットHQサーチ
    protected void SearchHQ(bool isSetTarget = false)
    {
        if (HQTran == null) HQTran = BattleManager.Instance.SelectTargetHQTran(enemySide, myTran);
        if (isSetTarget) SetTarget(HQTran, 1.0f);
    }

    //ターゲットサーチ
    protected override void Search()
    {
        //再索敵チェック
        if (!IsResearch()) return;

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
            
            //優先ターゲット判定
            if (weaponCtrl.IsPriorityTarget(target, distance))
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
        if (targetTran == null || !isWithinRange)
        {
            //HQ
            SearchHQ(true);

            //破壊可能オブジェクト
            if (!IsDiscoveryTarget(targetTran, searchRange)) SearchObstacle();
        }
    }

    //再索敵判定
    protected override bool IsResearch()
    {
        if (leftForceTargetTime > 0) return false;
        if (targetTran != null && targetTran.tag == targetTag)
        {
            if (IsDiscoveryTarget(targetTran, searchRange) && weaponCtrl.IsWithinRange(targetTran))
            {
                //再ターゲット
                SetTarget(targetTran);
                return false;
            }
        }
        return true;
    }

    //ターゲットへ到達可否判定
    protected virtual bool IsEnabledPath(Transform target)
    {
        return true;
    }

    protected virtual void SetLockOn(bool flg)
    {
        isLockOn = flg;
        if (flg)
        {
            //武器射程取得
            float attackRangeMax = weaponCtrl.GetMaxRange(targetTran);
            agent.stoppingDistance = attackRangeMax * 0.8f;
        }
        else
        {
            agent.stoppingDistance = 1.0f;
        }
    }

    //オブジェクトサーチ
    protected int obstacleIndex = 0;
    protected void SearchObstacle()
    {
        if (BattleManager.Instance.obstacleCtrls.Count == 0) return;
        obstacleIndex = (obstacleIndex + 1) % BattleManager.Instance.obstacleCtrls.Count;
        ObstacleController obstacleCtrl = BattleManager.Instance.obstacleCtrls[obstacleIndex];
        if (obstacleCtrl == null) return;
        if (obstacleCtrl.IsDiscovery(myTran, mySide, searchRange))
        {
            SetTarget(obstacleCtrl.transform);
        }
    }

    //有効なエージェントかチェック
    protected bool IsEffectiveAgent()
    {
        if (agent == null) return false;
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            agent.enabled = false;
            SetTarget(null);
            agent.enabled = true;
            return false;
        }
        return true;
    }

    //移動
    protected virtual void Move()
    {
        if (coolTime > 0 || targetTran == null) return;

        agent.isStopped = false;
        //if (agent.remainingDistance > 0 && agent.remainingDistance <= agent.stoppingDistance) return; 
        agent.destination = targetTran.position;
    }

    //攻撃判定
    protected override bool JudgeAttack()
    {
        //武器射程取得
        float attackRangeMax = weaponCtrl.GetMaxRange(targetTran);

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

    //ダメージ処理
    public override int Damage(int damage, Vector3 impact)
    {
        int d = base.Damage(damage, impact);

        //衝撃判定
        if (impact != Vector3.zero) Skip(impact);

        return d;
    }

    //ターゲット切り替え判定
    protected override void JugdeChangeTarget(Transform t)
    {
        if (t == null) return;
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
            if (targetDistance > Vector3.Distance(myTran.position, t.position)) SetTarget(t);
        }
    }

    //死亡
    protected override void Dead()
    {
        DeadDamage();
        BattleManager.Instance.RemoveUnit(mySide, myTran);
        base.Dead();
    }

    //死亡ダメージ
    protected void DeadDamage()
    {
        //HQにダメージを与える
        BattleManager.Instance.DeadDamage(mySide, myTran);
    }
    
    //HQまでの距離取得
    protected float GetHQDistance()
    {
        if (HQTran == null) return 9999;
        return Vector3.Distance(myTran.position, HQTran.position);

    }

    //ステータス取得
    protected float GetSpeed()
    {
        return  agent.speed;
    }
}
