using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class UnitController : BaseMoveController
{
    protected int mySide;
    protected int enemySide;
    protected string targetTag;
    protected string targetHQTag;
    protected int targetLayer;

    protected Transform HQTran;
    protected Transform targetTran;
    protected float targetDistance;
    protected bool isAttackRange;
    protected bool isLockOn;

    [SerializeField]
    protected Slider hpGage;
    [SerializeField]
    protected GameObject weapon;
    [SerializeField]
    protected int maxHP;
    protected int nowHP;
    [SerializeField]
    protected float researchLimit;
    protected float researchTime;
    protected float leftForceTargetTime;

    protected WeaponController weaponCtrl;
    protected float attackRange;

    protected const int MAX_DEFENCE = 90;

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
        }
        else
        {
            enemySide = Common.CO.SIDE_UNKNOWN;
        }
        isAttackRange = false;
        isLockOn = false;
        targetDistance = 0;
        researchTime = 0;
        leftForceTargetTime = 0;

        Init();
    }

    //初期処理
    protected virtual void Init()
    {
        OpenShield(3.0f);
        EquipWeapon();
        StartCoroutine(ActionRoutine());
    }

    protected virtual void Update()
    {
        if (nowHP <= 0) return;
        UpdateHpGage();
        researchTime += Time.deltaTime;
        if (leftForceTargetTime > 0) leftForceTargetTime -= Time.deltaTime;
    }

    //行動ルーチン
    IEnumerator ActionRoutine()
    {
        if (weapon == null) yield break;

        float wait = 0.5f;
        for (;;)
        {
            Action();
            yield return new WaitForSeconds(wait);
        }
    }

    //行動
    protected virtual void Action()
    {
        //索敵
        Search();

        //攻撃判定
        JudgeAttack();
    }

    //ターゲットまでの距離取得
    protected float GetTargetDistance()
    {
        if (targetTran == null) return 99999;
        return Vector3.Distance(myTran.position, targetTran.position);
    }

    //HPゲージ更新
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
        researchLimit = weaponCtrl.GetReload() * 1.5f;
    }

    //ターゲットサーチ
    protected virtual void Search()
    {
        //再索敵チェック
        if (targetTran != null && researchTime < researchLimit) return;

        //敵を探す
        List<Transform> targets = BattleManager.Instance.GetUnitList(enemySide);

        if (targets.Count == 0) return;

        //一番近い敵を決定
        int index = 0;
        float distance = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null) continue;
            float tmpDistance = Vector3.Distance(myTran.position, targets[i].position);
            if (i == 0 || tmpDistance < distance)
            {
                distance = tmpDistance;
                index = i;
            }
        }
        targetTran = targets[index];
    }

    //攻撃判定
    protected virtual bool JudgeAttack()
    {
        //目視チェック
        if (!LockOn()) return false;
        bool atk = Attack();
        if (atk) researchTime = 0;
        return atk;
    }

    //攻撃
    protected bool Attack()
    {
        return weaponCtrl.Attack(targetTran);
    }

    //目視チェック
    protected bool LockOn()
    {
        //敵との距離
        targetDistance = GetTargetDistance();
        isAttackRange = (targetDistance <= attackRange);

        bool isTargetSight = false;
        if (isAttackRange)
        {
            Ray ray = new Ray(myTran.position, targetTran.position - myTran.position);
            RaycastHit hit;
            if (Physics.SphereCast(ray, 0.3f, out hit, attackRange, targetLayer))
            {
                string hitTag = hit.transform.tag;
                if (hitTag == targetTran.tag) isTargetSight = true;
            }
        }
        isLockOn = isTargetSight;
        return isLockOn;
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

        //シールドチェック
        if (shieldTime > 0) return damage;

        //ダメージ
        int d = Damage(damage, impact);

        return d;
    }

    //ダメージ処理
    public virtual int Damage(int damage, Vector3 impact)
    {
        int d = CalcDamage(damage);

        //ダメージ
        nowHP -= d;
        if (nowHP <= 0)
        {
            Dead();
        }

        return d;
    }

    //ダメージ計算
    protected virtual int CalcDamage(int damage)
    {
        return damage;
    }

    //ターゲット切り替え判定
    protected virtual void JugdeChangeTarget(Transform t)
    {
        if (t == null || leftForceTargetTime > 0) return;
        if (targetTran == null
            || targetTran == HQTran 
            || targetTran.tag == Common.CO.TAG_BREAK_OBSTACLE
        ) {
            targetTran = t;
        }
        else
        {
            float enemyDistance = Vector3.Distance(myTran.position, t.position);
            if (targetDistance > enemyDistance) targetTran = t;
        }
    }

    //強制ターゲット
    public void SetForceTarget(Transform tran, float time)
    {
        targetTran = tran;
        leftForceTargetTime = time;
    }

    //死亡
    protected virtual void Dead()
    {
        UpdateHpGage();
        GetComponent<ObjectController>().DestoryObject();
    }

    //HP割合取得
    public int GetHpRate()
    {
        return Common.Func.GetPer(nowHP, maxHP);
    }

    //HPゲージ色設定
    public void SetHpGageColor(Color color)
    {
        if (hpGage == null) return;
        Transform fill = hpGage.transform.Find("Fill Area/Fill");
        if (fill == null) return;
        fill.GetComponent<Image>().color = color;
    }


    //シールド展開
    protected float shieldTime = 0;
    Coroutine shieldCoroutine;
    protected void OpenShield(float time)
    {
        if (shieldCoroutine != null)
        {
            shieldTime += time;
        }
        else
        {
            shieldTime = time;
            shieldCoroutine = StartCoroutine(ShieldCoroutine());
        }
    }
    IEnumerator ShieldCoroutine()
    {
        float wait = 0.5f;
        for (;;)
        {
            shieldTime -= wait;
            if (shieldTime <= 0) yield break;
            yield return new WaitForSeconds(wait);
        }
    }

}
