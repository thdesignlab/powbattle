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

    //[SerializeField]
    protected Slider hpGage;
    [SerializeField]
    protected GameObject weapon;
    [SerializeField]
    protected GameObject bufEffect;
    [SerializeField]
    protected GameObject debufEffect;
    [SerializeField]
    protected int maxHP;
    protected int nowHP;
    [SerializeField]
    protected int attack;
    [SerializeField]
    protected int defence;
    [SerializeField]
    protected float searchRange;
    [SerializeField]
    protected float researchLimit;
    protected float researchTime;
    protected float leftForceTargetTime;

    protected ObjectController _objCtrl;
    protected ObjectController objCtrl
    {
        get { return (_objCtrl) ? _objCtrl : _objCtrl = GetComponent<ObjectController>(); }
    }
    protected UnitMotionController _motionCtrl;
    protected UnitMotionController motionCtrl
    {
        get { return (_motionCtrl) ? _motionCtrl : _motionCtrl = GetComponent<UnitMotionController>(); }
    }
    protected WeaponController weaponCtrl;
    protected float attackRange;
    protected Transform _targetSight;
    protected Transform targetSight
    {
        get { return (_targetSight) ? _targetSight : _targetSight = transform.Find("TargetSight"); }
    }

    protected const int MAX_DEFENCE = 90;
    protected const int MIN_SPEED_EFFECT = -100;

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
        SetHpGage();
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
        if (searchRange <= 0) searchRange = weaponCtrl.GetReload() * 1.5f;
        weaponCtrl.SetMotionCtrl(motionCtrl);
    }

    //ターゲットサーチ
    protected virtual void Search()
    {
        //再索敵チェック
        if (leftForceTargetTime > 0) return;
        if (targetTran != null && targetTran.tag == targetTag && researchTime < researchLimit) return;

        //敵を探す
        List<Transform> targets = BattleManager.Instance.GetUnitList(enemySide);

        if (targets.Count == 0) return;

        //射程内の敵をターゲット
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null) continue;
            if (Vector3.Distance(myTran.position, targets[i].position) > attackRange) continue;
            SetTarget(targets[i]);
            break;
        }
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
        //targetDistance = GetTargetDistance();
        isAttackRange = (targetDistance <= attackRange);

        bool isTargetSight = false;
        if (isAttackRange && targetTran != null)
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
        int def = GetDefence();
        if (def == 0) return damage;
        if (def > MAX_DEFENCE) def = MAX_DEFENCE;
        int d = (int)(damage * (100 - def) / 100.0f);
        return d;
    }

    //ターゲット切り替え判定
    protected virtual void JugdeChangeTarget(Transform t)
    {
        if (t == null) return;
        if (targetTran == null
            || targetTran == HQTran 
            || targetTran.tag == Common.CO.TAG_BREAK_OBSTACLE
        ) {
            SetTarget(t);
        }
        else
        {
            float enemyDistance = Vector3.Distance(myTran.position, t.position);
            if (targetDistance > enemyDistance)
            {
                SetTarget(t);
            }
        }
    }

    //ターゲット設定
    public void SetTarget(Transform tran, float forceTime = 0.0f)
    {
        targetTran = tran;
        targetDistance = (targetTran != null) ? Vector3.Distance(myTran.position, targetTran.position) : 99999;
        leftForceTargetTime = forceTime;
    }

    //死亡
    protected virtual void Dead()
    {
        UpdateHpGage();
        if (motionCtrl != null)
        {
            StartCoroutine(WaitDestroy());
        }
        else
        {
            objCtrl.DestroyObject();
        }
    }
    IEnumerator WaitDestroy()
    {
        motionCtrl.Dead();

        for (int i=0; i<100; i++)
        {
            yield return null;
            if (motionCtrl.IsFinishedDead()) break;
        }
        objCtrl.DestroyObject();
    }

    //HP割合取得
    public int GetHpRate()
    {
        return Common.Func.GetPer(nowHP, maxHP);
    }

    //HPゲージ設定
    public void SetHpGage()
    {
        Transform statusCanvas = myTran.Find("StatusCanvas");
        if (statusCanvas == null) return;
        string myGageName = (mySide == Common.CO.SIDE_MINE) ? "HP" : "EnemyHP";
        string enemyGageName = (mySide == Common.CO.SIDE_MINE) ? "EnemyHP" : "HP";
        hpGage = statusCanvas.Find(myGageName).GetComponent<Slider>();
        statusCanvas.Find(enemyGageName).gameObject.SetActive(false);
        //if (hpGage == null) return;
        //Transform fill = hpGage.transform.Find("Fill Area/Fill");
        //if (fill == null) return;
        //fill.GetComponent<Image>().color = color;
    }

    //###シールド展開###
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

    //###ステータス効果###
    protected Dictionary<int, Dictionary<int, Coroutine>> statusEffectCoroutine = new Dictionary<int, Dictionary<int, Coroutine>>()
    {
        { Common.CO.STATUS_ATTACK, new Dictionary<int, Coroutine>() },
        { Common.CO.STATUS_DEFENCE, new Dictionary<int, Coroutine>() },
        { Common.CO.STATUS_SPEED, new Dictionary<int, Coroutine>() },
    };

    //ステータス効果付与
    public void AttackEffect(int rate, float time)
    {
        StatusEffect(Common.CO.STATUS_ATTACK, rate, time);
    }
    public void DefenceEffect(int rate, float time)
    {
        StatusEffect(Common.CO.STATUS_DEFENCE, rate, time);
    }
    public void SpeedEffect(int rate, float time)
    {
        StatusEffect(Common.CO.STATUS_SPEED, rate, time);
    }
    public void StatusEffect(int type, int rate, float time)
    {
        if (rate == 0 || time <= 0 || !statusEffectCoroutine.ContainsKey(type)) return;

        Dictionary<int, Coroutine> statusEffectCoroutineClone = new Dictionary<int, Coroutine>(statusEffectCoroutine[type]);
        bool isSet = true;
        foreach (int nowRate in statusEffectCoroutineClone.Keys)
        {
            if (statusEffectCoroutine[type][nowRate] == null)
            {
                statusEffectCoroutine[type].Remove(nowRate);
                continue;
            }

            if ((nowRate > 0 && rate > 0)
                || (nowRate < 0 && rate < 0))
            {
                if (Mathf.Abs(rate) >= Mathf.Abs(nowRate))
                {
                    //既存効果リセット
                    ResetStatusEffect(type, nowRate);
                }
                else
                {
                    //無効
                    isSet = false;
                }
                break;
            }
        }

        //効果付与
        if (isSet) SetStatusEffect(type, rate, time);
    }

    //ステータス効果セット
    protected void SetStatusEffect(int type, int rate, float time)
    {
        Coroutine cor = StartCoroutine(EffectCoroutine(type, rate, time));
        statusEffectCoroutine[type].Add(rate, cor);
        StartCoroutine(StatusEffect(type, rate, (rate > 0) ? bufEffect : debufEffect));
    }

    //ステータス効果リセット
    protected void ResetStatusEffect(int type, int rate)
    {
        if (!statusEffectCoroutine[type].ContainsKey(rate)) return;
        StopCoroutine(statusEffectCoroutine[type][rate]);
        statusEffectCoroutine[type].Remove(rate);
    }
    IEnumerator EffectCoroutine(int type, int rate, float time)
    {
        float wait = 0.5f;
        for (;;)
        {
            yield return new WaitForSeconds(wait);
            time -= wait;
            if (time <= 0) break;
        }
        ResetStatusEffect(type, rate);
    }

    //ステータス効果取得
    protected int GetStatusEffect(int type)
    {
        int rate = 0;
        if (!statusEffectCoroutine.ContainsKey(type)) return rate;
        foreach (int r in statusEffectCoroutine[type].Keys)
        {
            rate += r;
        }
        return rate;
    }

    //ステータス取得
    protected int GetAttack()
    {
        return GetStatusEffect(Common.CO.STATUS_ATTACK) + attack;
    }
    protected int GetDefence()
    {
        return GetStatusEffect(Common.CO.STATUS_DEFENCE) + defence;
    }

    //ステータスエフェクト
    IEnumerator StatusEffect(int type, int rate, GameObject effect)
    {
        if (effect == null) yield break;
        GameObject effectObj = Instantiate(effect, myTran.position + Vector3.up * 1.5f, myTran.rotation);
        effectObj.transform.SetParent(myTran, true);
        float wait = 0.5f;
        for (;;)
        {
            if (!statusEffectCoroutine[type].ContainsKey(rate)) break;
            yield return new WaitForSeconds(wait);
        }
        Destroy(effectObj);
    }
}
