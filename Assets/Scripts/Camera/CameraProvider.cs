using UnityEngine;

public class CameraProvider : MonoBehaviour
{
    public Camera Get()
    {
        return GetComponent<Camera>();
    }
}
