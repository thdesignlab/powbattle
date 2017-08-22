using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WeaponController : MonoBehaviour
{
    protected Transform myTran;
    protected Transform ownerTran;
    protected Transform targetTran;
    protected UnitMotionController motionCtrl;
    protected AudioManager audioMgr;
    protected float maxRange;
    protected float minRange;
    protected List<Transform> muzzleList = new List<Transform>();

    [SerializeField]
    protected GameObject bullet;
    [SerializeField]
    protected int rapidCount;
    [SerializeField]
    protected float rapidInterval;
    [SerializeField]
    protected float range;
    [SerializeField]
    protected float reload;
    [SerializeField]
    protected float motionWait;
    [SerializeField]
    protected float moveDelay;
    [SerializeField, Range(0, 2), TooltipAttribute("0:default, 1:near, 2:far")]
    protected int weaponType;

    protected float leftReload = 0;
    protected int attackRate = 0;

    protected virtual void Awake()
    {
        myTran = transform;
        audioMgr = myTran.GetComponentInChildren<AudioManager>();
        SetMuzzle();
    }

    protected virtual void Update()
    {
        if (leftReload > 0)
        {
            leftReload -= Time.deltaTime;
        }
    }

    public bool Attack(Transform target, int rate = 0)
    {
        if (!isEnabledAttack()) return false;
        SetTarget(target);
        attackRate = rate;
        AttackProcess();
        Reload();
        return true;
    }

    protected virtual void AttackProcess()
    {
        StartCoroutine(RapidShoot());
    }

    protected virtual IEnumerator RapidShoot()
    {
        if (muzzleList.Count == 0) yield break;

        for (int i = 1; i <= rapidCount; i++)
        {
            LockOn();
            AttackMotion(i);
            yield return new WaitForSeconds(motionWait);

            PlaySE(i - 1);
            for (int j = 0; j < muzzleList.Count; j++)
            {
                Shoot(j);
            }
            yield return new WaitForSeconds(rapidInterval);
        }
        AttackMotion(0);
    }

    protected virtual void LockOn()
    {
        return;
    }

    protected virtual GameObject Shoot(int muzzleNo = 0)
    {
        GameObject obj = Instantiate(bullet, muzzleList[muzzleNo].position, muzzleList[muzzleNo].rotation);
        SetDamageEffect(obj);
        return obj;
    }

    protected virtual void SetDamageEffect(GameObject shootObj)
    {
        DamageEffectController effectCtrl = shootObj.GetComponent<DamageEffectController>();
        effectCtrl.SetOwner(ownerTran);
        effectCtrl.SetTarget(targetTran);
        effectCtrl.SetDamageRate(attackRate);
    }

    protected void SetMuzzle()
    {
        foreach (Transform child in myTran)
        {
            if (child.tag == Common.CO.TAG_MUZZLE)
            {
                muzzleList.Add(child);
            }
        }
        if (muzzleList.Count == 0)
        {
            muzzleList.Add(myTran);
        }
    }

    protected virtual void AttackMotion(int count)
    {
        if (motionCtrl == null) return;
        motionCtrl.Attack(count);
    }
    
    public virtual bool isEnabledAttack()
    {
        if (leftReload > 0) return false;
        return true;
    }

    protected virtual void Reload()
    {
        leftReload = reload;
    }

    protected virtual void SetTarget(Transform t)
    {
        targetTran = t;
    }

    public virtual void SetOwner(Transform t)
    {
        ownerTran = t;
    }

    public void SetMotionCtrl(UnitMotionController ctrl)
    {
        motionCtrl = ctrl;
    }

    public float GetReload()
    {
        return reload;
    }

    public virtual float GetMinRange(Transform target = null)
    {
        return 0;
    }

    public virtual float GetMaxRange(Transform target = null)
    {
        return range;
    }

    public float GetMoveDelay()
    {
        return moveDelay;
    }

    //射程内判定
    public bool IsWithinRange(Transform target, float distance = -1)
    {
        if (distance < 0) distance = Vector3.Distance(myTran.position, target.position);
        return (GetMinRange(target) <= distance && distance <= GetMaxRange(target));
    }

    //優先ターゲット判定
    public bool IsPriorityTarget(Transform target, float distance = -1)
    {
        if (weaponType == Common.CO.WEAPON_TYPE_DEFAULT) return IsWithinRange(target, distance);

        if (distance < 0) distance = Vector3.Distance(myTran.position, target.position);
        float minRange = GetMinRange(target);
        float maxRange = GetMaxRange(target);
        bool isPriority = false;
        switch (weaponType)
        {
            case Common.CO.WEAPON_TYPE_NEAR:
                isPriority = (distance <= (maxRange + minRange) / 3 + minRange);
                break;

            case Common.CO.WEAPON_TYPE_FAR:
                isPriority = (distance >= (maxRange + minRange) * 2 / 3 + minRange);
                break;
        }
        return isPriority;
    }

    protected void PlaySE(int no = 0)
    {
        if (audioMgr == null) return;
        audioMgr.Play(no, true);
    }

    public int GetWeaponType()
    {
        return weaponType;
    }
}
