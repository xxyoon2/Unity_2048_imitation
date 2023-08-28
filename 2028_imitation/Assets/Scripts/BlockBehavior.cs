using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehavior : MonoBehaviour
{
    private Transform _targetPoint;
    public void Move(Transform targetPoint)
    {
        _targetPoint = targetPoint;
        StartCoroutine(MoveTargetPoint());
    }

    IEnumerator MoveTargetPoint()
    {
        while (true)
        {
            Debug.Log("움직이다.");
            float distance = Vector2.Distance(this.transform.position, _targetPoint.position);
            if (distance <= 0.01f)
            {
                this.transform.position = _targetPoint.position;
                break;
            }

            this.transform.position = Vector2.MoveTowards(this.transform.position, _targetPoint.position, 0.5f);
            yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }
}
