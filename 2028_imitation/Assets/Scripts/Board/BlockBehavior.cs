using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockBehavior : MonoBehaviour
{
    [SerializeField] private TextMeshPro _text;
    private Transform _targetPoint;
    private Animator _animator;

    private const float MOVE_TIEM = 0.08f;

    private void OnEnable()
    {
        _animator = GetComponent<Animator>();

    }

    public void Creation(int num)
    {
        _text.text = num.ToString();
        //_animator.Play("BlockCreation");
    }

    public void Extinction()
    {
        //_animator.Play("BlockExtinction");
    }

    public void Move(Transform targetPoint)
    {
        _targetPoint = targetPoint;
        StartCoroutine(MoveTargetPoint());
    }

    /// <summary>
    /// 실질적으로 블록을 움직이는 코루틴
    /// </summary>
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
