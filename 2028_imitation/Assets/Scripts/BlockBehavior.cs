using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehavior : MonoBehaviour
{
    private Transform _targetPoint;
    private Animator _animator;

    private const float MOVE_TIEM = 0.1f;

    public void Move(Transform targetPoint)
    {
        _targetPoint = targetPoint;
        StartCoroutine(MoveTargetPoint());
    }

    IEnumerator MoveTargetPoint()
    {
        float percent = 0;
        float current = 0;
        Vector2 startPoint = transform.position;
        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / MOVE_TIEM;

            transform.position = Vector3.Lerp(startPoint, _targetPoint.position, percent);
            //Debug.Log("움직이다.");
            float distance = Vector2.Distance(transform.position, _targetPoint.position);
            if (distance <= 0.01f)
            {
                transform.position = _targetPoint.position;
                break;
            }
            yield return null;
        }

        yield break;
    }
}
