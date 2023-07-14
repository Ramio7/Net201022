using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelView : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> _spawnPoints;

    public List<SpawnPoint> SpawnPoints { get => _spawnPoints; private set => _spawnPoints = value; }

    public event Action<Vector3> OnSpawnPointGranted;

    public void GetSpawnPoint()
    {
        var freeSpawns = new List<SpawnPoint>();
        foreach (var spawnPoint in _spawnPoints)
        {
            if (!spawnPoint.SpawnPointClosed.GetValue()) freeSpawns.Add(spawnPoint);
        }
        var index = UnityEngine.Random.Range(0, freeSpawns.Count);
        OnSpawnPointGranted?.Invoke(_spawnPoints[index].transform.position);
    }
}
