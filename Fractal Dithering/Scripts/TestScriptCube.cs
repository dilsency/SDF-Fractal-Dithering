using UnityEngine;

public class TestScriptCube : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    [SerializeField]
    bool shouldRotateLeft = true;
    [SerializeField]
    bool shouldRotateForward = true;
    [SerializeField]
    bool shouldRotateUp = true;

    // Update is called once per frame
    void Update()
    {
        Vector3 eulers = new Vector3();
        if (shouldRotateLeft)
        {
            eulers += Vector3.left;
        }
        if (shouldRotateForward)
        {
            eulers += Vector3.forward;
        }
        if (shouldRotateUp)
        {
            eulers += Vector3.up;
        }
        eulers *= Time.deltaTime * 10.0f;

        transform.Rotate(eulers);
    }
}
