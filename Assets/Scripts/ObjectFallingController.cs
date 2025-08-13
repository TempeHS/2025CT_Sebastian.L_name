using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFallController : MonoBehaviour
{
    [Header("Spawn Timing")]
    [Tooltip("Seconds between each spawn")]
    public float wait = 1f;

    [Header("Hazard Settings")]
    public GameObject[] fallingObjects;

    [Header("Player Reference")]
    [Tooltip("Drag your Player GameObject here")]
    public Transform player;
    // If you’d rather auto-find by tag, uncomment:
    // private void Awake() { player = GameObject.FindWithTag("Player").transform; }

    [Header("Spawn Area Around Player")]
    [Tooltip("Horizontal range from player center")]
    public float horizontalRange = 10f;
    [Tooltip("Vertical offset above player")]
    public float verticalOffset = 10f;

    void Start()
    {
        if (player == null)
            Debug.LogError("Player Transform not assigned on ObjectFallController!");

        InvokeRepeating(nameof(Fall), wait, wait);
        int fall = gameObject.layer;                          // must be set to FallingObjects
        //int platform = LayerMask.NameToLayer("objects");
        //Physics2D.IgnoreLayerCollision(fall, platform, true);
    }

    void Fall()
    {

        int idx = Random.Range(0, fallingObjects.Length);
        Vector3 spawnPos = new Vector3(
            player.position.x + Random.Range(-horizontalRange, horizontalRange),
            player.position.y + verticalOffset,
            0f
        );
        Instantiate(fallingObjects[idx], spawnPos, Quaternion.identity);

    }
    
    
}
