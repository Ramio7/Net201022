using Photon.Pun;
using UnityEngine;

public class CameraController : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        if (photonView.IsMine)
        {
            var camera = Camera.main;
            camera.transform.SetParent(transform, false);
            camera.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
    }
}
