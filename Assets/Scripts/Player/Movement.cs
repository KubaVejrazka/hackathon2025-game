using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    float moveDistance = 2f;
    float moveSpeed = 5f;

    Vector3 globalTargetPosition;
    bool isMoving = false;

    void Start()
    {
        globalTargetPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (!isMoving && Vector3.Distance(transform.position, globalTargetPosition) < 0.01f)
        {
            int randomDirection = Random.Range(0, 3);
            switch (randomDirection)
            {
                case 0:
                    MoveForward();
                    break;
                case 1:
                    MoveLeft();
                    break;
                case 2:
                    MoveRight();
                    break;
            }
        }
    }

    public void MoveLeft()
    {
        Vector3 targetPosition = transform.position + Vector3.left * moveDistance;
        StartCoroutine(MoveToPosition(targetPosition));
    }

    public void MoveRight()
    {
        Vector3 targetPosition = transform.position + Vector3.right * moveDistance;
        StartCoroutine(MoveToPosition(targetPosition));
    }

    public void MoveForward()
    {
        Vector3 targetPosition = transform.position + Vector3.forward * moveDistance;
        StartCoroutine(MoveToPosition(targetPosition));
    }

    IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true;
        globalTargetPosition = targetPosition;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}
