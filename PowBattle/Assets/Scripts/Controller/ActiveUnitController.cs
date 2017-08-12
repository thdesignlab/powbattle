using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ActiveUnitController : UnitController
{
    protected NavMeshAgent agent;

    [SerializeField]
    protected int hqDamage;
    protected float defaultSpeed;

    protected float coolTime = 0;

    protected override void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        base.Start();
    }

    protected override void Init()
    {
        //SearchHQ();
        base.Init();
        SaveDefault();
    }

    protected override void Update()
    {
        if (nowHP <= 0 || BattleManager.Instance.isBattleEnd)
        {
            agent.isStopped = true;
            return;
        }

        base.Update();

        if (coolTime > 0) coolTime -= Time.deltaTime;

        //敵の方を向く
        if (isLockOn) LookTarget(targetTran, agent.angularSpeed, new Vector3(1, 0, 1));

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

    //デフォルトステータス保管
    protected void SaveDefault()
    {
        defaultSpeed = agent.speed;
    }

    //ターゲットHQサーチ
    protected void SearchHQ(bool isSetTarget = false)
    {
        if (HQTran == null) HQTran = BattleManager.Instance.SelectTargetHQTran(enemySide, myTran);
        if (isSetTarget) SetTarget(HQTran);
    }

    //ターゲットサーチ
    protected override void Search()
    {
        //再索敵チェック
        if (leftForceTargetTime > 0) return;
        if (targetTran != null && targetTran.tag == targetTag && researchTime < researchLimit) return;

        //敵を探す
        List<Transform> targets = BattleManager.Instance.GetUnitList(enemySide);

        Transform tmpTarget = null;
        float tmpDistance = 0;
        foreach (Transform target in targets)
        {
            if (target == null) continue;
            //if (mySide == 0) Debug.Log(myTran.position + " >> " + target.position);

            float distance = Vector3.Distance(myTran.position, target.position);
            if (distance > searchRange) continue;
            if (!IsDiscoveryTarget(target, searchRange)) continue;

            if (distance <= attackRange)
            {
                //決定
                tmpTarget = target;
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
        if (targetTran == null || targetDistance > attackRange)
        {
            //HQ
            SearchHQ(true);

            //破壊可能オブジェクト
            if (targetDistance > attackRange) SearchObstacle();
        }
    }

    protected void SetLockOn(bool flg)
    {
        isLockOn = flg;
        if (flg)
        {
            agent.stoppingDistance = attackRange * 0.8f;
        }
        else
        {
            agent.stoppingDistance = 1.5f;
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
        if (obstacleCtrl.IsDiscovery(myTran, mySide))
        {
            SetTarget(obstacleCtrl.transform);
        }
    }

    //有効なエージェントかチェック
    protected bool IsEffectiveAgent()
    {
        if (agent == null) return false;
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid) return false;
        return true;
    }

    //移動
    protected void Move()
    {
        if (coolTime > 0 || targetTran == null) return;

        agent.isStopped = false;
        agent.destination = targetTran.position;
    }

    //攻撃判定
    protected override bool JudgeAttack()
    {
        SetLockOn(IsDiscoveryTarget(targetTran, attackRange));
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
        if (targetTran == null
            || targetTran.tag == targetHQTag
            || targetTran.tag == Common.CO.TAG_BREAK_OBSTACLE
        ) {
            SetTarget(t);
        }
        else
        {
            if (targetDistance > Vector3.Distance(myTran.position, t.position))
            {
                SetTarget(t);
            }
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
        if (hqDamage <= 0) return;
        //if (BattleManager.Instance.hqCtrl[mySide] == null) return;
        //if (HQCtrl == null) return;

        //HQにダメージを与える
        BattleManager.Instance.DeadDamage(mySide, hqDamage, myTran);
        //float rate = 1.0f;
        //if (!BattleManager.Instance.IsSuperioritySituation(mySide)) rate *= 0.75f;
        //HQCtrl.Hit((int)(hqDamage * rate), null);
    }
    
    //HQまでの距離取得
    protected float GetHQDistance()
    {
        if (HQTran == null) return 99999;
        return Vector3.Distance(myTran.position, HQTran.position);

    }

    //ステータス取得
    protected float GetSpeed()
    {
        int rate = GetStatusEffect(Common.CO.STATUS_SPEED);
        if (rate < MIN_SPEED_EFFECT) rate = MIN_SPEED_EFFECT;
        return  agent.speed * (100 + rate) / 100.0f;
    }
}
