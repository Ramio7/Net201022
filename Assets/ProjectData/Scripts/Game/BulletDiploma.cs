using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(AudioSource))]
public class BulletDiploma : MonoBehaviourPunCallbacks
{
    [SerializeField] private int _bulletStartingForce = 100;
    [SerializeField] private float _bulletDamage = 10.0f;

    private float _selfDestructionTime;
    private float _bulletLifeTime = 2.0f;

    public float BulletDamage { get => _bulletDamage; private set => _bulletDamage = value; }

    public override void OnEnable()
    {
        _selfDestructionTime = Time.time + _bulletLifeTime;
        GetComponent<Rigidbody>().AddForce(Vector3.forward * _bulletStartingForce);
    }

    private void Update()
    {
        if (_selfDestructionTime <= Time.time)
        {
            photonView.TransferOwnership(PhotonNetwork.CurrentRoom.MasterClientId);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Bullet faced {collision.gameObject.name}");
        photonView.TransferOwnership(PhotonNetwork.CurrentRoom.MasterClientId);
        PhotonNetwork.Destroy(gameObject);
    }
}
