using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector3 coordinates { get; private set; }

    void Awake()
    {
        coordinates = new Vector3(
            Mathf.Round(transform.localPosition.x / Block.distanceBetweenBlocks),
            Mathf.Round(transform.localPosition.y / Block.distanceBetweenBlocks),
            Mathf.Round(transform.localPosition.z / Block.distanceBetweenBlocks)
        );

        Debug.Log($"Player coordinates: {coordinates}");
    }

    void Start()
    {
        Move(Vector3.forward, 1, 0.5f); // Example move command
    }

    public void Move(Vector3 direction, int distance, float speed)
    {
        // Ensure the direction vector has only one axis set to 1
        if (direction == Vector3.right || direction == Vector3.left ||
            direction == Vector3.up || direction == Vector3.down ||
            direction == Vector3.forward || direction == Vector3.back)
        {
            Vector3 targetPosition = coordinates + direction * distance * Block.distanceBetweenBlocks;
            StartCoroutine(MoveToPosition(targetPosition, speed));
        }
        else
        {
            Debug.LogError("Invalid direction vector. Only one axis can be set to 1.");
        }
    }


    private IEnumerator MoveToPosition(Vector3 targetPosition, float speed)
    {
        Vector3 startPosition = transform.position;
        float journeyTime = Vector3.Distance(startPosition, targetPosition) / speed;
        float elapsedTime = 0f;

        while (elapsedTime < journeyTime)
        {
            elapsedTime += Time.fixedDeltaTime;
            float fractionOfJourney = elapsedTime / journeyTime;

            // Apply a Bezier-like easing function for speed (fade in and fade out)
            float easedFraction = fractionOfJourney * fractionOfJourney * (3f - 2f * fractionOfJourney);

            transform.position = Vector3.Lerp(startPosition, targetPosition, easedFraction);
            yield return null;
        }

        transform.position = targetPosition;
    }
}
