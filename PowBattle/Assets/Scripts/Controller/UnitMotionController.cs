using UnityEngine;
using System.Collections;

public class UnitMotionController : MonoBehaviour
{
    [SerializeField]
    private GameObject body;
    private Animator _animator;
    private Animator animator
    {
        get { return (_animator) ? _animator : _animator = body.GetComponent<Animator>(); }
    }

    void Update()
    {
    }

    //### 走る ###
    public void Run(bool flg = true)
    {
        animator.SetBool(Common.CO.MOTION_FLG_RUN, flg);
    }

    //### ジャンプ ###
    public void Jump(bool flg = true)
    {
        animator.SetBool(Common.CO.MOTION_FLG_JUMP, flg);
    }

    //### 攻撃 ###
    public void Attack(int cnt)
    {
        animator.SetInteger(Common.CO.MOTION_FLG_ATTACK, cnt);
    }

    //### 死亡 ###
    public void Dead(bool flg = true)
    {
        animator.SetBool(Common.CO.MOTION_FLG_DEAD, flg);
    }

    //状態判定
    private bool IsCurrentMotion(string tag)
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.IsTag(tag);
    }
    public bool IsWait()
    {
        return IsCurrentMotion(Common.CO.MOTION_TAG_WAIT);
    }
    public bool IsRun()
    {
        return IsCurrentMotion(Common.CO.MOTION_TAG_RUN);
    }
    public bool IsJump()
    {
        return IsCurrentMotion(Common.CO.MOTION_TAG_JUMP);
    }
    public bool IsAttack()
    {
        return IsCurrentMotion(Common.CO.MOTION_TAG_ATTACK);
    }
    public bool IsDead()
    {
        return IsCurrentMotion(Common.CO.MOTION_TAG_DEAD);
    }

    //モーション終了判定
    private bool IsFinishedMotion(string tag)
    {
        bool isFinished = false;
        AnimatorStateInfo animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (animStateInfo.IsTag(tag))
        {
            if (animStateInfo.normalizedTime >= 1.0f) isFinished = true;
        }
        return isFinished;
    }
    public bool IsFinishedDead()
    {
        return IsFinishedMotion(Common.CO.MOTION_TAG_DEAD);
    }
}
