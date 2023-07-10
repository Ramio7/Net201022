using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float distance = 7.0f;

    [SerializeField] private float height = 3.0f;

    [SerializeField] private Vector3 centerOffset = Vector3.zero;

    [SerializeField] private bool followOnStart = false;

    [SerializeField] private float smoothSpeed = 0.125f;

    Transform cameraTransform;

    bool isFollowing;

    Vector3 cameraOffset = Vector3.zero;

    void Start()
    {
        if (followOnStart)
        {
            OnStartFollowing();
        }
    }


    void LateUpdate()
    {
        if (cameraTransform == null && isFollowing)
        {
            OnStartFollowing();
        }

        if (isFollowing)
        {
            Follow();
        }
    }

    public void OnStartFollowing()
    {
        cameraTransform = Camera.main.transform;
        isFollowing = true;
        Cut();
    }

    void Follow()
    {
        cameraOffset.z = -distance;
        cameraOffset.y = height;

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, this.transform.position + this.transform.TransformVector(cameraOffset), smoothSpeed * Time.deltaTime);

        cameraTransform.LookAt(this.transform.position + centerOffset);

    }

    void Cut()
    {
        cameraOffset.z = -distance;
        cameraOffset.y = height;

        cameraTransform.position = this.transform.position + this.transform.TransformVector(cameraOffset);

        cameraTransform.LookAt(this.transform.position + centerOffset);
    }
}
