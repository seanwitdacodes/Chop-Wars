using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnFall : MonoBehaviour
{
    public float destroyY = -5f; // Y position where it gets destroyed

    void Update()
    {
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}
