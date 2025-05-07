using UnityEngine;

public enum BlockType
{
    STATIC,
    INTERACTABLE,
}

public class Block : MonoBehaviour
{
    private float distanceBetweenBlocks = 1 + 0.1f; // block size + gap size

    public BlockType blockType { get; private set; }
    public Vector3 coordinates { get; private set; }

    void Awake()
    {
        coordinates = new Vector3(
            Mathf.Round(transform.localPosition.x / distanceBetweenBlocks),
            Mathf.Round(transform.localPosition.y / distanceBetweenBlocks),
            Mathf.Round(transform.localPosition.z / distanceBetweenBlocks)
        );
    }

    public void Interact()
    {
        if (blockType == BlockType.STATIC)
        {
            Debug.Log("This block is static and cannot be interacted with.");
            return;
        }

        Debug.Log("Interacting with block: " + gameObject.name);
    }
}
