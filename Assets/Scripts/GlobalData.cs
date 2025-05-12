using UnityEngine;

public class GlobalData : MonoBehaviour
{
    public static GlobalData instance { get; private set; }

    public float distanceBetweenBlocks2d = 1 + 0.1f; // block size + gap size
    public float distanceBetweenBlocksY = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
