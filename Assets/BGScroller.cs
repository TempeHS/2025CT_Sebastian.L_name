using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGScroller : MonoBehaviour
{
    [Header("References")]
    public Transform player;               // Drag your player here
    public Transform[] backgrounds;        // Two background segments

    [Header("Settings")]
    public float backgroundWidth = 20f;    // Width of each background segment

    private int leftIndex = 0;
    private int rightIndex = 1;

    void Update()
    {
        float playerX = player.position.x;

        // If player moves past the right background
        if (playerX > backgrounds[rightIndex].position.x)
        {
            RepositionLeft();
        }
        // If player moves past the left background
        else if (playerX < backgrounds[leftIndex].position.x)
        {
            RepositionRight();
        }
    }

    void RepositionLeft()
    {
        // Move left segment to the right of the right segment
        backgrounds[leftIndex].position = backgrounds[rightIndex].position + Vector3.right * backgroundWidth;

        // Swap indices
        int temp = leftIndex;
        leftIndex = rightIndex;
        rightIndex = temp;
    }

    void RepositionRight()
    {
        // Move right segment to the left of the left segment
        backgrounds[rightIndex].position = backgrounds[leftIndex].position - Vector3.right * backgroundWidth;

        // Swap indices
        int temp = leftIndex;
        leftIndex = rightIndex;
        rightIndex = temp;
    }
}
