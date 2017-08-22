using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DamageEffectController : BaseMoveController
{
    protected Transform ownerTran;
    protected Transform targetTran;
    protected string ownerTag;
    protected DamageController dmgCtrl;
    private ObjectController _objCtrl;
    protected ObjectController objCtrl
    {
        get { return (_objCtrl) ? _objCtrl : _objCtrl = GetComponent<ObjectController>(); }
    }
    protected AudioManager audioMgr;
    protected ApproachAudioManager appAudioMgr;

    [SerializeField]
    protected int damage;
    [SerializeField]
    protected float impact;
    [SerializeField]
    protected bool isHitBreak;
    [SerializeField]
    protected bool isShootDown;

    protected const int MIN_DAMAGE_RATE = -90;

    protected override void Awake()
    {
        base.Awake();

        dmgCtrl = GetComponent<DamageController>();
        audioMgr = GetComponent<AudioManager>();
        appAudioMgr = audioMgr as ApproachAudioManager;
    }

    public void SetOwner(Transform t)
    {
        ownerTran = t;
        if (ownerTran != null)
        {
            ownerTag = ownerTran.tag;
            objCtrl.SetOwner(ownerTran);
        }
    }

    public void SetTarget(Transform t)
    {
        targetTran = t;
        if (appAudioMgr != null) appAudioMgr.SetTarget(targetTran);
    }

    public void SetDamageRate(int rate)
    {
        if (rate == 0) return;
        if (rate < MIN_DAMAGE_RATE) rate = MIN_DAMAGE_RATE;
        damage = damage * (100 + rate) / 100;
    }

    //衝突イベント
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(ownerTran + " >> "+ other.transform);
        Transform hitTran = other.transform;
        if (Common.Func.IsMySide(ownerTag, hitTran.tag)) return;

        bool isHit = false;

        //撃墜
        if (!isHit) isHit = ShootDown(hitTran);

        //ダメージ
        if (!isHit) isHit = Hit(hitTran);

        //自壊判
        if (isHit && IsHitBreak(hitTran.tag)) BreakProcess();
    }

    //ダメージ判定
    protected virtual bool Hit(Transform hitTran)
    {
        return dmgCtrl.Damage(damage, impact, myTran, hitTran, ownerTran);
    }

    //撃墜
    protected bool ShootDown(Transform hitTran)
    {
        if (hitTran.tag != Common.CO.TAG_DAMAGE_EFFECT || !isShootDown) return false;
        return hitTran.GetComponent<ObjectController>().ShootDown(ownerTran);
    }

    //衝突判定
    protected virtual bool IsHitBreak(string hitTag)
    {
        return isHitBreak;
    }

    //自壊
    protected virtual void BreakProcess()
    {
        objCtrl.DestroyObject();
    }

    //SE
    protected virtual void PlaySE(int no = 0)
    {
        if (audioMgr == null) return;
        audioMgr.Play(no);
    }
}
