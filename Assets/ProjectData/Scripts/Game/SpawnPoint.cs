using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpawnPoint : MonoBehaviour
{
    public ReactiveProperty<bool> SpawnPointClosed = new(false);

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player)) SpawnPointClosed?.SetValue(true);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player)) SpawnPointClosed?.SetValue(false);
    }
}
