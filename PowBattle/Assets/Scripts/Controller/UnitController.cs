using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class UnitController : BaseMoveController
{
    protected int mySide;
    protected int enemySide;
    protected NavMeshAgent agent;
    protected string targetTag;
    protected string targetHQTag;
    protected int targetLayer;

    protected Transform HQTran;
    protected Transform targetTran;
    protected float targetDistance;

    [SerializeField]
    protected Slider hpGage;
    [SerializeField]
    protected GameObject weapon;
    [SerializeField]
    protected int maxHP;
    protected int nowHP;
    [SerializeField]
    protected int defence;
    [SerializeField]
    protected int moveLimit;
    [SerializeField]
    protected int hqDamage;

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
        mySide = Common.Func.GetMySide(myTran.tag);
        enemySide = BattleManager.Instance.GetEnemySide(mySide);
        if (mySide != Common.CO.SIDE_UNKNOWN)
        {
            targetTag = Common.CO.tagUnitArray[enemySide];
            targetHQTag = Common.CO.tagHQArray[enemySide];
            targetLayer = Common.Func.GetSightLayerMask(enemySide);
            agent = GetComponent<NavMeshAgent>();
            SearchHQ();
        }
        else
        {
            enemySide = Common.CO.SIDE_UNKNOWN;
        }

        EquipWeapon();
    }

    protected virtual void Update()
    {
        if (nowHP <= 0) return;
        UpdateHpGage();

        leftMoveDelay -= Time.deltaTime;
        if (leftMoveDelay <= 0 && agent.isStopped) agent.isStopped = false;

        isAttackRange = false;
        isLockOn = false;
        targetDistance = 0;

        if (targetTran == null)
        {
            Search();
            return;
        }

        //敵との距離
        targetDistance = Vector3.Distance(myTran.position, targetTran.position);
        if (targetDistance <= attackRange)
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

    protected void UpdateHpGage()
    {
        if (hpGage != null)
        {
            hpGage.value = GetHpRate();
        }
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
    protected void SearchHQ()
    {
        HQTran = BattleManager.Instance.hqInfo[enemySide];
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
        if (targets.Length == 0)
        {
            targetTran = HQTran;
            return false;
        }

        //敵HQまでの距離
        float hqDistance = GetHQDistance();

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
        //敵とHQ近いほうをターゲット
        if (hqDistance < distance)
        {
            targetTran = HQTran;
            distance = hqDistance;
        }
        else
        {
            targetTran = targets[index].transform;
        }

        //破壊可能オブジェクト
        if (distance > attackRange)
        {
            if (Random.Range(0, 100) >= 95)
            {
                foreach (Transform obj in BattleManager.Instance.breakableObstacles)
                {
                    if (obj == null) continue;
                    float d = Vector3.Distance(myTran.position, obj.position);
                    if (d < distance)
                    {
                        targetTran = obj;
                        break;
                    }

                }
            }
        }
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
            string hitTag = hit.transform.tag;
            //if (hitTag == targetTag || hitTag == targetHQTag) isTargetSight = true;
            if (hitTag == targetTran.tag) isTargetSight = true;
            //if (!isTargetSight) Debug.Log(name + " >> "+ hitTag +" / "+ targetTran.tag);
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
        //ターゲット切り替え判定
        JugdeChangeTarget(enemyTran);

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

    //ターゲット切り替え判定
    protected virtual void JugdeChangeTarget(Transform t)
    {
        if (targetTran == null || targetTran == HQTran || targetTran.tag == Common.CO.TAG_BREAK_OBSTACLE)
        {
            targetTran = t;
        }
        else
        {
            float enemyDistance = Vector3.Distance(myTran.position, targetTran.position);
            if (targetDistance > enemyDistance) targetTran = t;
        }
    }

    //死亡
    protected void Dead()
    {
        UpdateHpGage();
        DeadDamage();
        ObjectController objCon = GetComponent<ObjectController>();
        objCon.DestoryObject();
    }

    //死亡ダメージ
    protected void DeadDamage()
    {
        if (hqDamage <= 0) return;
        if (BattleManager.Instance.hqCtrl[mySide] == null) return;

        //HQにダメージを与える
        float rate = 1.0f;
        if (!BattleManager.Instance.JugdeBattleSituation(mySide)) rate /= 2;
        BattleManager.Instance.hqCtrl[mySide].Hit((int)(hqDamage * rate), null);
    }

    //HP割合取得
    public int GetHpRate()
    {
        return Common.Func.GetPer(nowHP, maxHP);
    }

    //HQまでの距離取得
    protected float GetHQDistance()
    {
        if (HQTran == null) return 99999;
        return Vector3.Distance(myTran.position, HQTran.position);

    }

    //HPゲージ色設定
    public void SetHpGageColor(Color color)
    {
        if (hpGage == null) return;
        Transform fill = hpGage.transform.Find("Fill Area/Fill");
        if (fill == null) return;
        fill.GetComponent<Image>().color = color;
    }
}
