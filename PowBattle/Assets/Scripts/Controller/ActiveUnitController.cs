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
        SearchHQ();
        base.Init();
        SaveDefault();
    }

    protected override void Update()
    {
        base.Update();

        if (coolTime > 0) coolTime -= Time.deltaTime;

        //敵の方を向く
        if (isLockOn) LookTarget(targetTran, agent.angularSpeed, new Vector3(1, 0, 1));

        //移動モーション
        MoveMotion();
    }

    //行動
    protected override void Action()
    {
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
    protected void SearchHQ()
    {
        HQTran = BattleManager.Instance.hqInfo[enemySide];
    }

    //ターゲットサーチ
    protected override void Search()
    {
        //再索敵チェック
        if (leftForceTargetTime > 0) return;
        if (targetTran != null && targetTran.tag == targetTag && researchTime < researchLimit) return;

        //敵を探す
        List<Transform> targets = BattleManager.Instance.GetUnitList(enemySide);
        if (targets.Count == 0) return;

        Transform tmpTarget = null;
        float tmpDistance = 0;
        NavMeshHit hit;
        foreach (Transform target in targets)
        {
            if (target == null) continue;
            float distance = Vector3.Distance(myTran.position, target.position);
            if (distance > searchRange) continue;
            if (agent.Raycast(target.position, out hit))
            {
                if (distance <= attackRange)
                {
                    //決定
                    tmpTarget = target;
                    break;
                }
                else if (distance <= searchRange)
                {
                    //仮
                    if (tmpDistance == 0 || tmpDistance >= distance)
                    {
                        tmpTarget = target;
                        tmpDistance = distance;
                    }
                }
            }
        }
        if (tmpTarget != null) SetTarget(tmpTarget);

        //敵以外をターゲット
        if (targetTran == null)
        {
            //HQ
            SetTarget(HQTran);

            //破壊可能オブジェクト
            if (targetDistance > attackRange) SearchObstacle();
        }
    }

    //オブジェクトサーチ
    protected int obstacleIndex = 0;
    protected void SearchObstacle()
    {
        ObstacleController obstacleCtrl = BattleManager.Instance.obstacleCtrls[obstacleIndex];
        if (obstacleCtrl == null) return;
        if (obstacleCtrl.IsDiscovery(myTran, mySide))
        {
            SetTarget(obstacleCtrl.transform);
        }
        obstacleIndex = (obstacleIndex + 1) % BattleManager.Instance.obstacleCtrls.Count;
    }


    //移動
    protected void Move()
    {
        if (coolTime > 0 || targetTran == null || agent == null) return;
        if (agent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            agent.isStopped = false;
            agent.destination = targetTran.position;
        }
    }

    //攻撃判定
    protected override bool JudgeAttack()
    {
        if (agent == null) return false;

        bool atk = base.JudgeAttack();
        if (isLockOn)
        {
            agent.stoppingDistance = attackRange * 0.8f;
        }
        else
        {
            agent.stoppingDistance = 1.0f;
        }
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
        if (BattleManager.Instance.hqCtrl[mySide] == null) return;

        //HQにダメージを与える
        float rate = 1.0f;
        if (!BattleManager.Instance.JugdeBattleSituation(mySide)) rate *= 0.75f;
        BattleManager.Instance.hqCtrl[mySide].Hit((int)(hqDamage * rate), null);
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
