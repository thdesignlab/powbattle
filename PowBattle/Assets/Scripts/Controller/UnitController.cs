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
    protected bool isLockOn;

    protected Slider hpGage;
    [SerializeField]
    protected GameObject weapon;
    protected GameObject _bufEffect;
    protected GameObject bufEffect
    {
        get {　return (_bufEffect != null) ? _bufEffect : _bufEffect = Common.Resource.GetEffectResource("BufEffect"); }
    }
    protected GameObject _debufEffect;
    protected GameObject debufEffect
    {
        get { return (_debufEffect != null) ? _debufEffect : _debufEffect = Common.Resource.GetEffectResource("DebufEffect"); }
    }
    [SerializeField]
    protected int maxHP;
    protected int nowHP;
    protected int attack;
    protected int defence;
    [SerializeField]
    protected float searchRange;
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
    protected Transform weaponTran;
    protected WeaponController weaponCtrl;
    protected LaserPointerController laserPointerCtrl;

    protected const float SPAWN_WAIT_TIME = 1.0f;
    protected const int MAX_DEFENCE = 90;
    protected const int MIN_SPEED_EFFECT = -100;

    protected const string UNIT_PARTS_GROUND = "Ground";
    protected const string UNIT_PARTS_WEAPON_JOINT = "WeaponJoint";
    protected const string UNIT_PARTS_TARGET_SIGHT = "TargetSight";
    protected const string UNIT_PARTS_STATUS_POINT = "StatusPoint";
    protected const string UNIT_PARTS_STATUS_CANVAS = "StatusCanvas";

    protected bool isActive = false;

    protected override void Awake()
    {
        base.Awake();

        isActive = Common.Func.IsBattleScene();
        JudgeArtillery();
    }

    protected virtual void Start()
    {
        if (isActive)
        {
            nowHP = maxHP;
            isLockOn = false;
            targetDistance = 0;
            leftForceTargetTime = 0;
        }
        else
        {
            SetHpGage(isActive);
        }
        Init();
    }

    //初期処理
    protected virtual void Init()
    {
        if (isActive)
        {
            EquipWeapon();
            GetLaserPointer();
            StartCoroutine(ActionRoutine());
        }
        else
        {
            if (myRigidbody != null) myRigidbody.isKinematic = true;
        }
    }

    public void SetSide(int side)
    {
        mySide = side;
        enemySide = BattleManager.Instance.GetEnemySide(mySide);
        if (mySide != Common.CO.SIDE_UNKNOWN)
        {
            targetTag = Common.CO.tagUnitArray[enemySide];
            targetHQTag = Common.CO.tagHQArray[enemySide];
            targetLayer = Common.Func.GetSightLayerMask(enemySide);
            tag = Common.CO.tagUnitArray[side];
            Common.Func.SetLayer(gameObject, Common.CO.layerUnitArray[side], false);
            SetHpGage();
        }
        else
        {
            SetHpGage(false);
        }
    }

    protected virtual void Update()
    {
        if (!isActive) return;

        if (nowHP <= 0 || BattleManager.Instance.isBattleEnd) return;
        UpdateHpGage();
        if (leftForceTargetTime > 0) leftForceTargetTime -= Time.deltaTime;
    }

    //行動ルーチン
    IEnumerator ActionRoutine()
    {
        OpenShield(SPAWN_WAIT_TIME);
        yield return new WaitForSeconds(SPAWN_WAIT_TIME);

        if (weapon == null) yield break;

        float wait = 1.0f;
        for (;;)
        {
            if (nowHP <= 0 || BattleManager.Instance.isBattleEnd) yield break;
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

    //レーザーポインター取得
    protected void GetLaserPointer()
    {
        Transform targetSight = transform.Find(UNIT_PARTS_TARGET_SIGHT);
        if (targetSight == null) return;
        laserPointerCtrl = targetSight.GetComponent<LaserPointerController>();

        if (laserPointerCtrl != null)
        {
            laserPointerCtrl.SetLayerMask(LayerMask.GetMask(new string[] { Common.CO.layerUnitArray[enemySide] }));
            Color color = (mySide == Common.CO.SIDE_MINE) ? Color.cyan : Color.red;
            laserPointerCtrl.SetLaserColor(color);
        }
    }

    //武器装備
    protected void EquipWeapon()
    {
        if (weapon == null) return;

        //装備箇所検索
        Transform joint = transform.Find(UNIT_PARTS_WEAPON_JOINT);
        if (joint == null) joint = myTran;

        //武器生成＆装備
        GameObject w = Instantiate(weapon, joint.position, joint.rotation, joint);
        weaponCtrl = w.GetComponent<WeaponController>();
        weaponCtrl.SetOwner(myTran);
        if (searchRange <= 0) searchRange = weaponCtrl.GetMaxRange() * 1.5f;
        weaponCtrl.SetMotionCtrl(motionCtrl);
    }

    //ターゲットサーチ
    protected virtual void Search()
    {
        //再索敵チェック
        if (!IsResearch()) return;

        //武器射程取得
        if (targetTran != null)
        {
            if (!weaponCtrl.IsWithinRange(targetTran))
            {
                SetTarget(null);
                isLockOn = false;
            }
        }
        else
        {
            //敵を探す
            List<Transform> targets = BattleManager.Instance.GetUnitList(enemySide);

            if (targets.Count == 0) return;

            //射程内の敵をターゲット
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null) continue;
                if (!weaponCtrl.IsWithinRange(targets[i])) continue;
                if (!IsDiscoveryTarget(targets[i], searchRange)) continue;
                SetTarget(targets[i], 5.0f);
                break;
            }
            isLockOn = (targetTran != null);
        }
    }

    //再索敵判定
    protected virtual bool IsResearch()
    {
        if (leftForceTargetTime > 0) return false;
        return true;
    }

    //攻撃判定
    protected virtual bool JudgeAttack()
    {
        if (!isLockOn) return false;
        if (targetTran == null) return false;
        return Attack();
    }

    //攻撃
    protected bool Attack()
    {
        return weaponCtrl.Attack(targetTran, GetAttack());
    }

    //目視チェック
    protected virtual bool IsDiscoveryTarget(Transform target, float range = 0)
    {
        if (target == null) return false;
        if (range <= 0) range = searchRange;
        Vector3 basePos = myTran.position + myTran.forward + myTran.up;

        bool ret = false;
        if (target == targetTran && targetDistance <= 1.5f)
        {
            ret = true;
        }
        else if (Vector3.Distance(basePos, target.position) <= 1.5f)
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
        return;
    }

    //ターゲット設定
    public void SetTarget(Transform tran, float forceTime = 3.0f)
    {
        targetTran = tran;
        targetDistance = (targetTran != null) ? Vector3.Distance(myTran.position, targetTran.position) : 99999;
        leftForceTargetTime = (targetTran == null) ? 0 : forceTime;
        SetTargetSight();
    }

    //ターゲット視覚化
    protected void SetTargetSight()
    {
        if (!BattleManager.Instance.isVisibleTarget) return;
        if (laserPointerCtrl == null) return;

        if (targetTran != null)
        {
            laserPointerCtrl.SetOn(targetTran);
        }
        else
        {
            laserPointerCtrl.SetOff();
        }
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

    //最大HP取得
    public int GetMaxHp()
    {
        return maxHP;
    }

    //HP割合取得
    public int GetHpRate()
    {
        return Common.Func.GetPer(nowHP, maxHP);
    }

    //HPゲージ設定
    public void SetHpGage(bool flg = true)
    {
        Transform statusPoint = myTran.Find(UNIT_PARTS_STATUS_POINT);
        if (statusPoint == null) return;
        Transform statusCanvas = Instantiate(Common.Resource.GetUIResource(UNIT_PARTS_STATUS_CANVAS), statusPoint).transform;
        if (flg && mySide == Common.CO.SIDE_MINE)
        {
            hpGage = statusCanvas.Find("HP").GetComponent<Slider>();
            statusCanvas.Find("EnemyHP").gameObject.SetActive(false);
        }
        else
        { 
            statusCanvas.gameObject.SetActive(false);
        }
        //string myGageName = (mySide == Common.CO.SIDE_MINE) ? "HP" : "EnemyHP";
        //string enemyGageName = (mySide == Common.CO.SIDE_MINE) ? "EnemyHP" : "HP";
        //hpGage = statusCanvas.Find(myGageName).GetComponent<Slider>();
        //statusCanvas.Find(enemyGageName).gameObject.SetActive(false);
    }

    public virtual void SetExtraBuff()
    {
        AttackEffect(50, 15);
        DefenceEffect(50, 15);
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
        GameObject effectObj = Instantiate(effect, myTran.position + Vector3.up * 2.0f, myTran.rotation, myTran);
        float wait = 0.5f;
        for (;;)
        {
            if (!statusEffectCoroutine[type].ContainsKey(rate)) break;
            yield return new WaitForSeconds(wait);
        }
        Destroy(effectObj);
    }

    //足元取得
    public Transform GetGround()
    {
        Transform ground = myTran.Find(UNIT_PARTS_GROUND);
        if (ground == null) ground = myTran;
        return ground;
    }

    //固定砲台判定
    protected virtual void JudgeArtillery()
    {
        return;
    }

    public Transform GetTarget()
    {
        return targetTran;
    }
}
