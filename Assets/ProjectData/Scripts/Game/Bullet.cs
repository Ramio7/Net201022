using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private const int Bullet_Starting_Force = 10;

    public readonly float bulletDamage = 1.0f;

    void OnEnable()
    {
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * Bullet_Starting_Force);
    }
}
