using System.Collections;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

public class Player : MonoBehaviour
{
    public Vector3 coordinates { get; private set; }

    void Awake()
    {
        UpdateCoordinates();

        Debug.Log($"Player coordinates: {coordinates}");
    }

    void Start()
    {
        Move(Vector3.forward, 2, 0.5f); // Example move command
    }

    private void UpdateCoordinates()
    {
        coordinates = new Vector3(
            Mathf.Round(transform.localPosition.x / Block.distanceBetweenBlocks2d),
            Mathf.Round(transform.localPosition.y / Block.distanceBetweenBlocksY),
            Mathf.Round(transform.localPosition.z / Block.distanceBetweenBlocks2d)
        );
    }

    public void Move(Vector3 direction, int distance, float speed)
    {
        if (!IsValidDirection(direction))
        {
            return;
        }

        Vector3 targetPosition = CalculateTargetPosition(direction, distance);
        bool colided = false;

        int simulatedDistance = 1;
        while (simulatedDistance <= distance)
        {
            Block block = Block.FindBlockAtCoordinate(coordinates + direction * simulatedDistance);
            Debug.Log($"Block at {coordinates + direction * simulatedDistance}, block exists: {block != null}");
            Debug.Log($"Block type: {block?.blockType}");
            if (block != null && block.blockType == BlockType.STATIC)
            {
                Debug.Log($"Blocked by static block at {coordinates + direction * simulatedDistance}");
                targetPosition = CalculateTargetPosition(direction, simulatedDistance - 1);
                colided = true;
            }

            simulatedDistance++;
        }

        StartCoroutine(MoveToPosition(targetPosition, speed, colided));
    }

    private bool IsValidDirection(Vector3 direction)
    {
        return direction == Vector3.right || direction == Vector3.left ||
               direction == Vector3.up || direction == Vector3.down ||
               direction == Vector3.forward || direction == Vector3.back;
    }

    private Vector3 CalculateTargetPosition(Vector3 direction, int distance)
    {
        Vector3 targetPosition = coordinates;

        if (direction == Vector3.up || direction == Vector3.down)
        {
            targetPosition += direction * distance * Block.distanceBetweenBlocksY;
        }
        else
        {
            targetPosition += direction * distance * Block.distanceBetweenBlocks2d;
        }

        return targetPosition;
    }


    private IEnumerator MoveToPosition(Vector3 targetPosition, float speed, bool collided = false)
    {
        Vector3 startPosition = transform.position;
        float journeyTime = Vector3.Distance(startPosition, targetPosition) / speed;
        float elapsedTime = 0f;

        while (elapsedTime < journeyTime)
        {
            elapsedTime += Time.fixedDeltaTime;
            float fractionOfJourney = elapsedTime / journeyTime;

            float easedFraction = fractionOfJourney * fractionOfJourney * (3f - 2f * fractionOfJourney);

            transform.position = Vector3.Lerp(startPosition, targetPosition, easedFraction);
            yield return null;
        }

        transform.position = targetPosition;
        UpdateCoordinates();

        if (collided)
        {
            Vector3 bounceForwardPosition = targetPosition + ((targetPosition - startPosition).normalized * Block.distanceBetweenBlocks2d * 0.25f);
            Vector3 bounceBackPosition = targetPosition;
            elapsedTime = 0f;

            // Bounce forward
            while (elapsedTime < journeyTime)
            {
                elapsedTime += Time.fixedDeltaTime;
                float fractionOfJourney = elapsedTime / journeyTime;

                float easedFraction = fractionOfJourney * fractionOfJourney * (3f - 2f * fractionOfJourney);

                transform.position = Vector3.Lerp(targetPosition, bounceForwardPosition, easedFraction);
                yield return null;
            }

            elapsedTime = 0f;

            // Bounce back
            while (elapsedTime < journeyTime)
            {
                elapsedTime += Time.fixedDeltaTime;
                float fractionOfJourney = elapsedTime / journeyTime;

                float easedFraction = fractionOfJourney * fractionOfJourney * (3f - 2f * fractionOfJourney);

                transform.position = Vector3.Lerp(bounceForwardPosition, bounceBackPosition, easedFraction);
                yield return null;
            }
        }
    }
}
