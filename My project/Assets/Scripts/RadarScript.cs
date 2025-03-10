using System.Collections.Generic;
using UnityEngine;

public class RadarScript : MonoBehaviour
{
    private List<GameObject> collidingObjects = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {
        if (!collidingObjects.Contains(collision.gameObject))
        {
            collidingObjects.Add(collision.gameObject);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collidingObjects.Contains(collision.gameObject))
        {
            collidingObjects.Remove(collision.gameObject);
        }
    }

    public GameObject[] GetCollisions()
    {
        return collidingObjects.ToArray();
    }


}
