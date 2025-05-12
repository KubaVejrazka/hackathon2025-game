using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    public Vector3 coordinates { get; private set; }

    private Queue<PlayerAction> actionQueue = new Queue<PlayerAction>();
    private bool actionInProgress = false;

    void Awake()
    {
        UpdateCoordinates();
    }

    void Start()
    {
        //EnqueueAction(new MovementAction(4, 1));
        //EnqueueAction(new InteractionAction());
        //EnqueueAction(new RotationAction("right"));
        //EnqueueAction(new MovementAction(2, 1));
        //EnqueueAction(new InteractionAction());
    }

    private void UpdateCoordinates()
    {
        coordinates = new Vector3(
            Mathf.Round(transform.localPosition.x / Block.distanceBetweenBlocks2d),
            Mathf.Round(transform.localPosition.y / Block.distanceBetweenBlocksY),
            Mathf.Round(transform.localPosition.z / Block.distanceBetweenBlocks2d)
        );
    }

    public void EnqueueAction(PlayerAction action)
    {
        actionQueue.Enqueue(action);
        if (!actionInProgress)
        {
            StartCoroutine(ProcessActionQueue());
        }
    }

    private IEnumerator ProcessActionQueue()
    {
        actionInProgress = true;

        while (actionQueue.Count > 0)
        {
            PlayerAction action = actionQueue.Dequeue();
            yield return action.Execute(this);
        }

        actionInProgress = false;
    }

    public IEnumerator Move(Vector3 direction, int distance, float speed)
    {
        if (!IsValidDirection(direction))
        {
            yield break;
        }

        Vector3 targetPosition = CalculateTargetPosition(direction, distance);
        bool interrupted = false;
        bool won = false;

        int simulatedDistance = 0;
        while (simulatedDistance <= distance)
        {
            Vector3 currentPosition = coordinates + direction * simulatedDistance;

            // Check if there is a block under the player
            Block blockBelow = Block.FindBlockAtCoordinate(currentPosition + Vector3.down);
            if (blockBelow == null)
            {
                Debug.Log($"No block below at position: {currentPosition}");
                targetPosition = CalculateTargetPosition(direction, simulatedDistance - 1);
                interrupted = true;

                break;
            }

            // Check for collision in the direction
            Block block = Block.FindBlockAtCoordinate(currentPosition);
            //if (block != null && block.blockType == BlockType.STATIC)
            if (block != null)
            {
                Debug.Log($"Collision detected at distance: {simulatedDistance}");
                targetPosition = CalculateTargetPosition(direction, simulatedDistance - 1);
                interrupted = true;

                break;
            }

            simulatedDistance++;
        }

        yield return StartCoroutine(MoveToPosition(targetPosition, direction, speed * 1.5f, interrupted, won));
    }

    public IEnumerator Rotate(Vector3 rotation, float speed)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + rotation);
        float elapsedTime = 0f;
        float journeyTime = Quaternion.Angle(startRotation, targetRotation) / (speed * 100);

        while (elapsedTime < journeyTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / journeyTime;
            progress = Mathf.SmoothStep(0, 1, progress); // Ease-in and ease-out
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    public IEnumerator Interact()
    {
        Block blockInFront = Block.FindBlockAtCoordinate(coordinates + transform.forward);

        if (blockInFront == null || blockInFront.blockType == BlockType.STATIC)
        {
            yield return StartCoroutine(Rotate(new Vector3(0, -15, 0), 1));
            yield return StartCoroutine(Rotate(new Vector3(0, 30, 0), 1));
            yield return StartCoroutine(Rotate(new Vector3(0, -15, 0), 1));
        }

        else if (blockInFront.blockType == BlockType.INTERACTABLE)
        {
            Debug.Log($"Performing interaction on block at {coordinates + transform.forward}");

            yield return StartCoroutine(Rotate(new Vector3(0, 120, 0), 3));
            yield return StartCoroutine(Rotate(new Vector3(0, 120, 0), 3));
            yield return StartCoroutine(Rotate(new Vector3(0, 120, 0), 3));
        }

        else if (blockInFront.blockType == BlockType.FINISH)
        {
            Debug.Log($"Performing interaction on finish at {coordinates + transform.forward}");

            float journeyTime = 0.5f;
            float elapsedTime = 0f;
            Vector3 targetPosition = transform.position;
            Vector3 jumpPosition = targetPosition + new Vector3(0, 0.5f, 0);
            while (elapsedTime < journeyTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / journeyTime;
                progress = Mathf.SmoothStep(0, 1, progress);
                transform.position = Vector3.Lerp(targetPosition, jumpPosition, progress);
                yield return null;
            }
            transform.position = jumpPosition;

            yield return StartCoroutine(Rotate(new Vector3(0, 120, 0), 3));
            yield return StartCoroutine(Rotate(new Vector3(0, 120, 0), 3));
            yield return StartCoroutine(Rotate(new Vector3(0, 120, 0), 3));

            journeyTime = 0.5f;
            elapsedTime = 0f;
            while (elapsedTime < journeyTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / journeyTime;
                progress = Mathf.SmoothStep(0, 1, progress);
                transform.position = Vector3.Lerp(jumpPosition, targetPosition, progress);
                yield return null;
            }
            transform.position = targetPosition;

            Debug.Log("win");
            WebGLMessageHandler.SendToJavaScript(new WebGLMessageHandler.OutBrowserMessage
            {
                action = "levelPass",
                args = null
            });
        }

        yield return null;

        //yield return new WaitForSeconds(1f); // Simulate interaction delay
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

        return targetPosition;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, Vector3 direction, float speed, bool interrupted = false, bool won = false)
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

        if (interrupted)
        {
            //actionQueue.Clear();

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

        else if (won)
        {
            
        }
    }
}

public abstract class PlayerAction
{
    public abstract IEnumerator Execute(Player player);
}

public class MovementAction : PlayerAction
{
    private int distance;
    private float speed;

    public MovementAction(int distance, float speed)
    {
        this.distance = distance;
        this.speed = speed;
    }

    public override IEnumerator Execute(Player player)
    {
        // set direction to players current orientation
        Vector3 direction = player.transform.forward;
        yield return player.Move(direction, distance, speed);
    }
}

public class RotationAction : PlayerAction
{
    private Vector3 rotation;

    public RotationAction(string direction)
    {
        if (direction.ToLower() == "right")
        {
            this.rotation = new Vector3(0, 90, 0);
        }
        else if (direction.ToLower() == "left")
        {
            this.rotation = new Vector3(0, -90, 0);
        }
        else
        {
            Debug.LogError($"Invalid rotation direction: {direction}");
        }
    }

    public override IEnumerator Execute(Player player)
    {
        yield return player.Rotate(rotation, 2);
    }
}

public class InteractionAction : PlayerAction
{
    public override IEnumerator Execute(Player player)
    {
        yield return player.Interact();
    }
}
