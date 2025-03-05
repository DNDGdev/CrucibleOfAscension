using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float time = 4f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyObject(time));
    }

    IEnumerator DestroyObject(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
