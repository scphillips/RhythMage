using UnityEngine;
using Zenject;

namespace Outplay.RhythMage
{
    [ExecuteInEditMode]
    public class CameraController : ILateTickable
    {
        [Inject]
        public ICameraBehavior Behavior { get; set; }
        public GameObject Camera { get; set; }

        [Inject]
        CameraController(GameObject camera)
        {
            Camera = camera;
        }

        public void LateTick()
        {
            if (Behavior != null)
            {
                Behavior.Resolve(Camera);
            }
        }
    }
}
