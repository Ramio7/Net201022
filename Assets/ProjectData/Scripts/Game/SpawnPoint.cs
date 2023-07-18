using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpawnPoint : MonoBehaviour
{
    private ReactiveProperty<bool> _spawnPointOccupied = new(false);
    public bool IsOccupied { get => _spawnPointOccupied.Value; set => _spawnPointOccupied.Value = value; }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController player)) IsOccupied = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController player)) IsOccupied = false;
    }
}
