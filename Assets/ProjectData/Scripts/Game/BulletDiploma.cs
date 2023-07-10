using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletDiploma : MonoBehaviour
{
    private const int Bullet_Starting_Force = 100;

    public readonly float bulletDamage = 10.0f;

    private void OnEnable()
    {
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * Bullet_Starting_Force);
    }
}
