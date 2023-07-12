using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

public class LevelView : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> _spawnPoints;
    private int _freeSpawnPointIndex;

    public List<SpawnPoint> SpawnPoints { get => _spawnPoints; private set => _spawnPoints = value; }

    public event Action<Vector3> OnSpawnPointGranted;

    public async void GetSpawnPoint()
    {
        await Task.Run(() => WaitForCleanSpawnPoint());
        OnSpawnPointGranted?.Invoke(_spawnPoints[_freeSpawnPointIndex].transform.position);
    }

    private Task WaitForCleanSpawnPoint()
    {
        var randomizer = new Random();
        var spawnPointIndex = randomizer.Next(0, _spawnPoints.Count - 1);
        while (_spawnPoints[spawnPointIndex].SpawnPointClosed.GetValue())
        {
            spawnPointIndex = randomizer.Next(0, _spawnPoints.Count - 1);
            Task.Delay(1);
        }
        _freeSpawnPointIndex = spawnPointIndex;
        return Task.CompletedTask;
    }
}
