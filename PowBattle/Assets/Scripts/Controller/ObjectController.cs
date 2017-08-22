using UnityEngine;
using System.Collections;

public class ObjectController : MonoBehaviour {

    [SerializeField]
    private GameObject effectSpawn;
    [SerializeField]
    private float activeLimitTime = 0;
    [SerializeField]
    private float activeLimitDistance = 0;
    [SerializeField]
    private bool isNotAutoBreak = false;
    [SerializeField]
    private bool isNotShootDown = false;

    private Transform myTran;
    private Transform ownerTran;
    private Transform targetTran;

    private float activeTime = 0;
    private float activeDistance = 0;

    protected bool isDestroyProc = false;

    void Start()
    {
        myTran = transform;
        if (activeLimitTime == 0 && !isNotAutoBreak) activeLimitTime = 15;
        if (activeLimitTime > 0) StartCoroutine(CountDown());
        if (activeLimitDistance > 0) StartCoroutine(CheckDistance());
    }

    private void Update()
    {
        if (myTran.position.y < -1) DestroyObject(0.5f);
    }

    IEnumerator CountDown()
    {
        for (;;)
        {
            activeTime += Time.deltaTime;
            if (activeTime >= activeLimitTime) break;
            yield return null;
        }
        DestroyObject();
    }

    IEnumerator CheckDistance()
    {
        Vector3 prePos = myTran.position;
        for (;;)
        {
            activeDistance += Mathf.Abs(Vector3.Distance(myTran.position, prePos));
            if (activeDistance >= activeLimitDistance)
            {
                DestroyObject();
                break;
            }
            prePos = myTran.position;
            yield return null;
        }
    }

    public bool ShootDown(Transform opponentTran)
    {
        if (isNotShootDown) return false;

        if (opponentTran != null)
        {
            int mySide = Common.Func.GetMySide(tag);
            int opponentSide = Common.Func.GetMySide(opponentTran.tag);
            if (mySide == opponentSide) return false;
        }
        DestroyObject();
        return true;
    }

    public void DestroyObject(float delay = 0)
    {
        if (isDestroyProc) return;
        isDestroyProc = true;
        if (delay > 0)
        {
            StartCoroutine(DelayDestroy(delay));
        }
        else
        {
            DestroyProccess();
        }
    }

    IEnumerator DelayDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroyProccess();
    }

    private void DestroyProccess()
    {
        if (effectSpawn != null)
        {
            GameObject effectObj = Instantiate(effectSpawn, myTran.position, effectSpawn.transform.rotation);
            DamageEffectController dmgCtrl = effectObj.GetComponent<DamageEffectController>();
            if (dmgCtrl != null) dmgCtrl.SetOwner(ownerTran);
        }
        Destroy(gameObject);
    }

    public void ObjectSetting(Transform owner, Transform target)
    {
        SetOwner(owner);
        SetTarget(targetTran);
    }
    public void SetOwner(Transform owner)
    {
        ownerTran = owner;
    }

    public void SetTarget(Transform target)
    {
        targetTran = target;
    }

    public void Reset()
    {
        activeTime = 0;
        activeDistance = 0;
    }
}
