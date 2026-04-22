using UnityEngine;
using DG.Tweening;

public class BoatRocking : MonoBehaviour
{
    private float _swayDuration;
    private float _swayAngle;
    private float _bobHeight;

    private void Start()
    {
        _swayDuration = Random.Range(1.5f, 2f);
        _swayAngle = Random.Range(4.5f, 5.5f);
        _bobHeight = 0.1f;

        // z
        transform.DORotate(new Vector3(0, 0, _swayAngle), _swayDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // x
        transform.DORotate(new Vector3(_swayAngle * 0.5f, 0, 0), _swayDuration * 1.2f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // y
        transform.DOMoveY(transform.position.y + _bobHeight, _swayDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDestroy() => DOTween.Kill(transform);
}
