using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector3 coordinates { get; private set; }

    private Queue<MovementRequest> movementQueue = new Queue<MovementRequest>();
    private bool isMoving = false;

    void Awake()
    {
        UpdateCoordinates();
        Debug.Log($"Player coordinates: {coordinates}");
    }

    void Start()
    {
        EnqueueMove(Vector3.right, 2, 1f);
        EnqueueMove(Vector3.back, 2, 1f);
        EnqueueMove(Vector3.left, 2, 1f);
        EnqueueMove(Vector3.forward, 4, 1f);
        EnqueueMove(Vector3.left, 2, 1f);
    }

    private void UpdateCoordinates()
    {
        coordinates = new Vector3(
            Mathf.Round(transform.localPosition.x / Block.distanceBetweenBlocks2d),
            Mathf.Round(transform.localPosition.y / Block.distanceBetweenBlocksY),
            Mathf.Round(transform.localPosition.z / Block.distanceBetweenBlocks2d)
        );
    }

    public void EnqueueMove(Vector3 direction, int distance, float speed)
    {
        movementQueue.Enqueue(new MovementRequest(direction, distance, speed));
        if (!isMoving)
        {
            StartCoroutine(ProcessMovementQueue());
        }
    }

    private IEnumerator ProcessMovementQueue()
    {
        isMoving = true;

        while (movementQueue.Count > 0)
        {
            MovementRequest request = movementQueue.Dequeue();
            yield return StartCoroutine(Move(request.Direction, request.Distance, request.Speed));
        }

        isMoving = false;
    }

    private IEnumerator Move(Vector3 direction, int distance, float speed)
    {
        if (!IsValidDirection(direction))
        {
            yield break;
        }

        Vector3 targetPosition = CalculateTargetPosition(direction, distance);
        bool collided = false;

        int simulatedDistance = 1;
        while (simulatedDistance <= distance)
        {
            Block block = Block.FindBlockAtCoordinate(coordinates + direction * simulatedDistance);
            if (block != null && block.blockType == BlockType.STATIC)
            {
                targetPosition = CalculateTargetPosition(direction, simulatedDistance - 1);
                collided = true;

                break;
            }

            simulatedDistance++;
        }

        yield return StartCoroutine(MoveToPosition(targetPosition, speed * 1.5f, collided));
    }

    private bool IsValidDirection(Vector3 direction)
    {
        return direction == Vector3.right || direction == Vector3.left ||
               direction == Vector3.up || direction == Vector3.down ||
               direction == Vector3.forward || direction == Vector3.back;
    }

    private Vector3 CalculateTargetPosition(Vector3 direction, int distance)
    {
        Vector3 targetPosition = transform.position;

        if (direction == Vector3.up || direction == Vector3.down)
        {
            targetPosition += direction * distance * Block.distanceBetweenBlocksY;
        }
        else
        {
            targetPosition += direction * distance * Block.distanceBetweenBlocks2d;
        }

        Debug.Log($"Target coordinates: {targetPosition}");
        return targetPosition;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float speed, bool collided = false)
    {
        Vector3 startPosition = transform.position;
        float journeyTime = Vector3.Distance(startPosition, targetPosition) / speed;
        float elapsedTime = 0f;

        while (elapsedTime < journeyTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / journeyTime;
            progress = Mathf.SmoothStep(0, 1, progress);
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        transform.position = targetPosition;
        UpdateCoordinates();

        if (collided)
        {
            movementQueue.Clear();

            Vector3 direction = (targetPosition - startPosition).normalized;
            journeyTime = 0.25f;
            elapsedTime = 0f;
            Vector3 forwardPosition = targetPosition + direction * 0.25f;

            while (elapsedTime < journeyTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / journeyTime;
                progress = Mathf.SmoothStep(0, 1, progress);
                transform.position = Vector3.Lerp(targetPosition, forwardPosition, progress);
                yield return null;
            }
            transform.position = forwardPosition;

            journeyTime = 0.25f;
            elapsedTime = 0f;
            while (elapsedTime < journeyTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / journeyTime;
                progress = Mathf.SmoothStep(0, 1, progress);
                transform.position = Vector3.Lerp(forwardPosition, targetPosition, progress);
                yield return null;
            }
            transform.position = targetPosition;
            UpdateCoordinates();
        }
    }

    private class MovementRequest
    {
        public Vector3 Direction { get; }
        public int Distance { get; }
        public float Speed { get; }

        public MovementRequest(Vector3 direction, int distance, float speed)
        {
            Direction = direction;
            Distance = distance;
            Speed = speed;
        }
    }
}
