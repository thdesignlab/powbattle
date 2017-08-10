using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DamageEffectController : BaseMoveController
{
    protected Transform ownerTran;
    protected string ownerTag;
    protected DamageController dmgCtrl;
    protected ObjectController objCtrl;

    [SerializeField]
    protected int damage;
    [SerializeField]
    protected float impact;
    [SerializeField]
    protected bool isHitBreak;
    [SerializeField]
    protected bool isShootDown;

    protected override void Awake()
    {
        base.Awake();

        dmgCtrl = GetComponent<DamageController>();
        objCtrl = GetComponent<ObjectController>();
    }

    public void SetOwner(Transform t)
    {
        ownerTran = t;
        if (ownerTran != null) ownerTag = ownerTran.tag;
    }

    //衝突イベント
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(ownerTran + " >> "+ other.transform);
        Transform hitTran = other.transform;
        //撃墜判定
        if (hitTran.tag == Common.CO.TAG_DAMAGE_EFFECT)
        {
            ShootDown(hitTran);
            return;
        }
        //ダメージ判定
        if (Common.Func.IsMySide(ownerTag, hitTran.tag)) return;
        Hit(hitTran);
    }

    //ダメージ判定
    protected virtual void Hit(Transform hitTran)
    {
        if (dmgCtrl.Damage(damage, impact, myTran, hitTran, ownerTran))
        {
            if (IsHitBreak(hitTran.tag)) BreakProcess();
        }
    }

    //撃墜判定
    protected virtual void ShootDown(Transform hitTran)
    {
        if (!isShootDown) return;
        hitTran.GetComponent<ObjectController>().DestroyObject();
    }

    //衝突判定
    protected virtual bool IsHitBreak(string hitTag)
    {
        return isHitBreak;
    }

    //自壊
    protected virtual void BreakProcess()
    {
        objCtrl.SetOwner(ownerTran);
        objCtrl.DestroyObject();
    }
}
