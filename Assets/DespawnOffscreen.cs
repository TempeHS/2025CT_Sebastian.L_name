using UnityEngine;

public class DespawnOffscreen : MonoBehaviour
{
    private float bottomLimit;

    void Start()
    {
        // Set a Y-position threshold below the camera view
        bottomLimit = Camera.main.transform.position.y - 20f;
    }

    void Update()
    {
        if (transform.position.y < bottomLimit)
        {
            Destroy(gameObject);
        }
    }
}
