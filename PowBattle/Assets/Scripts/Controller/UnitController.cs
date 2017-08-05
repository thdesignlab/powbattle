using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : BaseMoveController
{
    protected bool isMine;
    protected NavMeshAgent agent;
    protected string targetTag;
    protected string targetHQTag;
    protected int targetLayer;

    protected Transform HQTran;
    protected Transform targetTran;

    [SerializeField]
    protected GameObject weapon;
    [SerializeField]
    protected int maxHP;
    protected int nowHP;
    [SerializeField]
    protected int speed;
    [SerializeField]
    protected int defence;
    [SerializeField]
    protected int moveLimit = 5;

    protected WeaponController weaponCtrl;
    protected float attackRange;
    protected float leftMoveDelay = 0;

    protected bool isAttackRange;
    protected bool isLockOn;
    protected float moveTime = 0;

    protected Dictionary<Transform, float> hateDic = new Dictionary<Transform, float>();

    protected virtual void Start()
    {
        nowHP = maxHP;
        isMine = (tag == Common.CO.TAG_UNIT) ? true : false;
        targetTag = (isMine) ? Common.CO.TAG_ENEMY : Common.CO.TAG_UNIT;
        targetHQTag = (isMine) ? Common.CO.TAG_ENEMY_HQ : Common.CO.TAG_HQ;
        string layer = ((isMine) ? Common.CO.LAYER_ENEMY : Common.CO.LAYER_UNIT);
        targetLayer = LayerMask.GetMask(new string[] { layer, Common.CO.LAYER_OBSTACLE });

        agent = GetComponent<NavMeshAgent>();

        EquipWeapon();
        SearchHQ();
        //targetTran = HQTran;
        //if (HQTran == null) Search();
        Search();
    }

    protected virtual void Update()
    {
        if (nowHP <= 0) return;

        leftMoveDelay -= Time.deltaTime;
        if (leftMoveDelay <= 0 && agent.isStopped) agent.isStopped = false;

        //索敵
        if (!Search())
        {
            targetTran = HQTran;
        }
        if (targetTran == null) return;

        //敵との距離
        isAttackRange = false;
        isLockOn = false;
        if (Vector3.Distance(myTran.position, targetTran.position) <= attackRange)
        {
            isAttackRange = true;
            LockOn();

            if (isLockOn)
            {
                //敵の方を向く
                LookTarget(targetTran, agent.angularSpeed, new Vector3(1, 0, 1));

                //攻撃
                if (Attack()) {
                    moveTime = 0;
                    return;
                }
            }
        }

        //移動
        Move();
        moveTime += Time.deltaTime;
    }

    //武器装備
    protected void EquipWeapon()
    {
        if (weapon == null) return;

        //装備箇所検索
        Transform joint = null;
        foreach (Transform child in myTran)
        {
            if (child.tag == Common.CO.TAG_WEAPON_JOINT)
            {
                joint = child;
                break;
            }
        }
        //武器生成＆装備
        GameObject w = Instantiate(weapon, joint.position, joint.rotation);
        w.transform.SetParent(joint, true);
        weaponCtrl = w.GetComponent<WeaponController>();
        weaponCtrl.SetOwner(myTran);
        attackRange = weaponCtrl.GetRange();
    }

    //ターゲットHQサーチ
    protected virtual void SearchHQ()
    {
        GameObject target = GameObject.FindGameObjectWithTag(targetHQTag);
        if (target == null) return;
        HQTran = target.transform;
    }

    //ターゲットサーチ
    protected virtual bool Search()
    {
        if (targetTran != null)
        {
            if (moveTime <= moveLimit) return true;
        }

        //敵を探す
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        if (targets.Length == 0) return false;

        //一番近い敵を決定
        int index = 0;
        float distance = 0;
        for (int i = 0; i < targets.Length; i++)
        {
            float tmpDistance = Vector3.Distance(myTran.position, targets[i].transform.position);
            if (i == 0 || tmpDistance < distance)
            {
                distance = tmpDistance;
                index = i;
            }
        }
        targetTran = targets[index].transform;
        return true;
    }

    //移動
    protected void Move()
    {
        if (leftMoveDelay > 0) return;
        agent.destination = targetTran.position;
    }

    //攻撃
    protected bool Attack()
    {
        //目視チェック
        if (!isLockOn) return false;

        //攻撃
        if (weaponCtrl.Attack(targetTran))
        {
            leftMoveDelay = weaponCtrl.GetMoveDelay();
            agent.isStopped = true;
            return true;
        }
        return false;
    }

    //目視チェック
    protected void LockOn()
    {
        bool isTargetSight = false;
        Ray ray = new Ray(myTran.position, targetTran.position - myTran.position);
        RaycastHit hit;
        if (Physics.SphereCast(ray, 0.3f, out hit, attackRange, targetLayer))
        //if (Physics.Raycast(ray, out hit, attackRange, targetLayer))
        {
            //Debug.Log(name +" >> "+hit.transform.name);
            if (hit.transform.tag == targetTag || hit.transform.tag == targetHQTag) isTargetSight = true;
        } 
        agent.stoppingDistance = (isTargetSight) ? attackRange * 0.8f : 1;        
        isLockOn = isTargetSight;
    }

    //被ダメージ
    public virtual int Hit(int damage, Transform enemyTran)
    {
        return Hit(damage, Vector3.zero, enemyTran);
    }
    public virtual int Hit(int damage, Vector3 impact, Transform enemyTran)
    {
        if (!isLockOn || targetTran == HQTran)
        {
            targetTran = enemyTran;
        }

        //ダメージ
        nowHP -= damage;
        if (nowHP <= 0)
        {
            Dead();
        }
        else if (impact != Vector3.zero)
        {
            Skip(impact);
        }

        return damage;
    }

    //死亡
    protected void Dead()
    {
        ObjectController objCon = GetComponent<ObjectController>();
        objCon.DestoryObject();
    }

    //HP割合取得
    public int GetHpRate()
    {
        return Common.Func.GetPer(nowHP, maxHP);
    }
}
