using Assets.Project.Scripts.NPC;
using BigProject.NPC.States;
using BigProject.Systems;
using UnityEngine;
using UnityEngine.Assertions;

public class NPCSimpleAnimator : MonoBehaviour
{
    [SerializeField]
    private string _initialStateName;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private float _timeOffset;

    private void Awake()
    {
        Assert.IsNotNull(_animator, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, name, "Animator"));
        SetAnimation(_initialStateName);
    }

    public void SetAnimation(string stateName)
    {
        _animator.Play(stateName, 0, _timeOffset);
    }
}
