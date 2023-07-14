using UnityEngine;

public class CameraController
{
    private Camera _camera;
    private Transform _playerTransform;
    private Transform _cameraTransform;

    public CameraController(Transform playerTransform, Transform cameraStartingTransform)
    {
        _camera = Camera.main;
        _playerTransform = playerTransform;
        _cameraTransform = cameraStartingTransform;
        StartCameraFollowing();
    }

    private void StartCameraFollowing()
    {
        _camera.transform.SetParent(_playerTransform, false);
        _camera.transform.SetPositionAndRotation(_cameraTransform.position, _cameraTransform.rotation);
    }
}
