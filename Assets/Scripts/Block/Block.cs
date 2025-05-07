using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    STATIC,
    INTERACTABLE,
}

public class Block : MonoBehaviour
{
    public static readonly float distanceBetweenBlocks = 1 + 0.1f; // block size + gap size

    [SerializeField]
    public BlockType blockType;
    public Vector3 coordinates { get; private set; }

    void Awake()
    {
        coordinates = new Vector3(
            Mathf.Round(transform.localPosition.x / distanceBetweenBlocks),
            Mathf.Round(transform.localPosition.y / distanceBetweenBlocks),
            Mathf.Round(transform.localPosition.z / distanceBetweenBlocks)
        );
    }

    public List<Block> GetAdjacentBlocks()
    {
        List<Block> adjacentBlocks = new List<Block>();

        Vector3[] directions = new Vector3[]
        {
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back
        };

        foreach (var direction in directions)
        {
            Vector3 adjacentCoordinate = coordinates + direction;
            Block adjacentBlock = FindBlockAtCoordinate(adjacentCoordinate);
            if (adjacentBlock != null)
            {
                adjacentBlocks.Add(adjacentBlock);
            }
        }

        return adjacentBlocks;
    }

    public static Block FindBlockAtCoordinate(Vector3 coordinate)
    {
        Collider[] colliders = Physics.OverlapSphere(coordinate, 0.1f);
        foreach (var collider in colliders)
        {
            Block block = collider.GetComponent<Block>();
            if (block != null && block.coordinates == coordinate)
            {
                return block;
            }
        }

        return null;
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
