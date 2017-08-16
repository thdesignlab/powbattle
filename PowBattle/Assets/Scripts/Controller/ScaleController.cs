using UnityEngine;
using System.Collections;

public class ScaleController : MonoBehaviour
{
    protected Transform myTran;

    [SerializeField]
    private Vector3 startScale;
    [SerializeField]
    private Vector3 endScale;
    [SerializeField]
    private float scaleTime;
    [SerializeField]
    private float scaleLateTime;

    private float activeTime = 0;

    protected void Awake()
    {
        myTran = transform;
    }
	
	void Update ()
    {
        if (scaleTime <= 0 || activeTime > scaleTime + scaleLateTime) return;

        activeTime += Time.deltaTime;

        if (scaleLateTime > 0 && scaleLateTime >= activeTime) return;

        float rate = (activeTime - scaleLateTime) / scaleTime;

        ChangeScale(Vector3.Lerp(startScale, endScale, rate));
    }

    void OnEnable()
    {
        ChangeScale(startScale);
        activeTime = 0;
    }

    private void ChangeScale(Vector3 scale)
    {
        myTran.localScale = scale;
    }
}
