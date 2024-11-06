using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform _tranformCamera;

    private void Start()
    {
        _tranformCamera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt( _tranformCamera );
    }
}
