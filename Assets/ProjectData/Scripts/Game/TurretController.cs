using Photon.Pun;
using UnityEngine;

public class TurretController : MonoBehaviourPunCallbacks
{
    [SerializeField] private float _turretHealth;

    private bool _isFollowing = false;
    private PlayerController _playerControllerFollowing;


    private void Start()
    {
    }

    private void Update()
    {
        FollowPlayer(_playerControllerFollowing);
    }

    public override void OnDisable()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController player))
        {
            if (_isFollowing) return;

            _playerControllerFollowing = player;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController playerController) && (playerController.Player == _playerControllerFollowing.Player)) _isFollowing = false;
    }

    private void OnTriggerStay(Collider other)
    {
        
    }

    private void FollowPlayer(PlayerController player)
    {
        if (!_isFollowing) return;

        transform.rotation.SetFromToRotation(transform.rotation.eulerAngles, player.transform.position);
    }
}
