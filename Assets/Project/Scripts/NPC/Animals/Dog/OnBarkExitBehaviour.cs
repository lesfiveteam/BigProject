using UnityEngine;

public class OnBarkExitBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isStanding", false);
        animator.ResetTrigger("doAction");
    }
}