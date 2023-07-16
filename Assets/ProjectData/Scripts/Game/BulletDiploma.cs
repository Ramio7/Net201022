using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(AudioSource))]
public class BulletDiploma : MonoBehaviour
{
    [SerializeField] private int _bulletStartingForce = 100;
    [SerializeField] private float _bulletDamage = 10.0f;
    public float BulletDamage { get => _bulletDamage; private set => _bulletDamage = value; }

    private void OnEnable()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.forward * _bulletStartingForce, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Bullet fased {collision.gameObject.name}");
        PhotonNetwork.Destroy(gameObject);
    }
}
