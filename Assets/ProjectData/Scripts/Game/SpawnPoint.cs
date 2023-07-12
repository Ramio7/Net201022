using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpawnPoint : MonoBehaviour
{
    public ReactiveProperty<bool> SpawnPointClosed = new();

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player)) SpawnPointClosed?.SetValue(true);
        else SpawnPointClosed.SetValue(false);
    }
}
