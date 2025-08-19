using UnityEngine;

public class TestScriptCube : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 eulers = new Vector3();
        eulers += Vector3.left;
        eulers += Vector3.forward;
        eulers *= Time.deltaTime * 10.0f;

        transform.Rotate(eulers);
    }
}
