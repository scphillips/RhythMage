using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Zenject.Inject]
    readonly CameraProvider cameraProvider;

    void Update()
    {
        transform.forward = cameraProvider.transform.forward;
    }
}
