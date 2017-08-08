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
    }

    //行動
    protected override void Action()
    {
        base.Action();

        if (mySide == 0)
        {
            //Debug.Log("---target >>" + targetTran);
            //Debug.Log("---defence >>" + GetDefence());
            //foreach (int rate in statusEffectCoroutine[Common.CO.STATUS_DEFENCE].Keys)
            //{
            //    Debug.Log("***rate >>" + rate);
            //}
        }


        //移動
        Move();
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
        base.Search();

        if (leftForceTargetTime > 0) return;

        //敵HQまでの距離
        float hqDistance = GetHQDistance();

        //敵とHQ近いほうをターゲット
        if (targetTran == null || hqDistance < targetDistance)
        {
            SetTarget(HQTran);
            //targetTran = HQTran;
            //targetDistance = hqDistance;
            //if (mySide == 0) Debug.Log("###target HQ >>"+ targetTran);
        }

        //破壊可能オブジェクト
        if (targetDistance > attackRange)
        {
            //★オブジェターゲット判定
            if (Random.Range(0, 100) >= 95)
            {
                Transform objTran = Common.Func.RandomList<Transform>(BattleManager.Instance.breakableObstacles);
                if (objTran != null)
                {
                    float d = Vector3.Distance(myTran.position, objTran.position);
                    if (d < targetDistance)
                    {
                        SetTarget(objTran);
                        //targetTran = objTran;
                        //targetDistance = d;
                        //if (mySide == 0) Debug.Log("###target OBSTACLE >> "+ targetTran);
                    }
                }
            }
        }
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
