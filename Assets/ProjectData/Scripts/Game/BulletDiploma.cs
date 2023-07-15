using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(AudioSource))]
public class BulletDiploma : MonoBehaviour
{
    [SerializeField] private int _bulletStartingForce = 100;

    public float bulletDamage = 10.0f;

    private void OnEnable()
    {
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * _bulletStartingForce);
    }

    private void OnCollisionEnter(Collision collision) => Destroy(gameObject);
}
