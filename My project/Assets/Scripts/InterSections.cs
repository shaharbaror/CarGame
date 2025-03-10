using UnityEngine;

public class InterSections : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // get all the connected intersections
    [SerializeField]
    public InterSections[] sections;
    protected float x;
    protected float y;
    void Start()
    {
        x = transform.position.x;
        y = transform.position.y;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
