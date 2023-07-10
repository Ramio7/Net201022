using Photon.Pun;
using System;
using UnityEngine;

[RequireComponent(typeof(CameraController))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public float Health = 100f;

    public static GameObject LocalPlayerInstance;

    public event Action<float> OnPlayerHpValueChanged;

    bool IsFiring;

    public void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        CameraController _cameraWork = gameObject.GetComponent<CameraController>();

        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("Missing CameraController Component on player Prefab.", this);
        }
    }


    public override void OnDisable()
    {
        base.OnDisable();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (other.TryGetComponent<BulletDiploma>(out var bullet))
        {
            Health -= bullet.bulletDamage;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.IsFiring);
            stream.SendNext(this.Health);
            UpdatePlayerHp(Health);
        }
        else
        {
            this.IsFiring = (bool)stream.ReceiveNext();
            this.Health = (float)stream.ReceiveNext();
        }
    }

    private void UpdatePlayerHp(float health) => OnPlayerHpValueChanged?.Invoke(health);
}
