using UnityEngine;
using System.Collections;

public class LookForwardController : MonoBehaviour
{
    [SerializeField]
    private Transform targetTran;
    //private Rigidbody myRigidbody;
    private Vector3 prePos;

    private void Start()
    {
        if (targetTran == null) targetTran = transform;
        //myRigidbody = GetComponent<Rigidbody>();
    }

    private void Update ()
    {
        Vector3 diffPos = targetTran.position - prePos;
        targetTran.LookAt(targetTran.position + diffPos / 10);
        prePos = targetTran.position;
    }

}
