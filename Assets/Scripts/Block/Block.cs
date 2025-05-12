using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    STATIC,
    INTERACTABLE,
}

public class Block : MonoBehaviour
{
    [SerializeField]
    public BlockType blockType;
    public Vector3 coordinates { get; private set; }

    void Start()
    {
        coordinates = new Vector3(
            Mathf.Round(transform.localPosition.x / GlobalData.instance.distanceBetweenBlocks2d),
            Mathf.Round(transform.localPosition.y / GlobalData.instance.distanceBetweenBlocksY),
            Mathf.Round(transform.localPosition.z / GlobalData.instance.distanceBetweenBlocks2d)
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
        List<Block> blocks = new List<Block>(FindObjectsByType<Block>(FindObjectsSortMode.None));
        foreach (Block block in blocks)
        {
            if (block.coordinates == coordinate)
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
