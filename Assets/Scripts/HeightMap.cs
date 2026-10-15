using UnityEngine;

public class HeightMap : MonoBehaviour
{
    private float[] heights;

    public void Initialize(int vertexCount)
    {
        heights = new float[vertexCount];
    }

    public float GetHeight(int index)
    {
        return heights[index];
    }

    public void AddHeight(int index, float amount)
    {
        heights[index] += amount;
    }

    public void ResetHeights()
    {
        System.Array.Clear(heights, 0, heights.Length);
    }
}
