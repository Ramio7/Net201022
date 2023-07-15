using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(AudioSource))]
public class BulletDiploma : MonoBehaviourPunCallbacks
{
    [SerializeField] private int _bulletStartingForce = 100;

    public float bulletDamage = 10.0f;

    public override void OnEnable()
    {
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * _bulletStartingForce);
    }

    private void OnCollisionEnter(Collision collision) => PhotonNetwork.Destroy(gameObject);
}
