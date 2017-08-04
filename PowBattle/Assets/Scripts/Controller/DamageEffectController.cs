using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DamageEffectController : BaseMoveController
{
    protected Transform ownerTran;
    protected DamageController dmgCtrl;
    protected ObjectController objCtrl;

    [SerializeField]
    protected int damage;

    protected override void Awake()
    {
        base.Awake();

        dmgCtrl = GetComponent<DamageController>();
        objCtrl = GetComponent<ObjectController>();
    }

    public void SetOwner(Transform t)
    {
        ownerTran = t;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(ownerTran + " >> "+ other.transform);
        if (ownerTran == null) return;
        Transform hitTran = other.transform;
        if (hitTran.tag == ownerTran.tag) return;
        Hit(hitTran);
    }

    protected virtual void Hit(Transform hitTran)
    {
        if (dmgCtrl.Damage(myTran, hitTran, damage, ownerTran))
        {
            if (IsHitBreak(hitTran.tag)) objCtrl.DestoryObject();
        }
    }
    protected virtual bool IsHitBreak(string hitTag)
    {
        return true;
    }
}
