using System.Collections.Generic;
using UnityEngine;

public class LevelView : MonoBehaviour
{
    [SerializeField] private List<Transform> _spawnPoints;

    public List<Transform> SpawnPoints { get => _spawnPoints; private set => _spawnPoints = value; }

    public Transform GetSpawnPoint()
    {
        var spawnPointIndex = Random.Range(0, _spawnPoints.Count - 1);
        return _spawnPoints[spawnPointIndex];
    }
}
