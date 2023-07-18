using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelView : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private List<SpawnPoint> _spawnPoints;

    public event Action<Vector3> OnSpawnPointGranted;

    public void GetSpawnPoint()
    {
        var freeSpawns = new List<SpawnPoint>();
        foreach (var spawnPoint in _spawnPoints)
        {
            if (!spawnPoint.IsOccupied) freeSpawns.Add(spawnPoint);
        }
        var index = UnityEngine.Random.Range(0, freeSpawns.Count);
        _spawnPoints[index].IsOccupied = true;
        OnSpawnPointGranted?.Invoke(_spawnPoints[index].transform.position);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            foreach (var spawnPoint in _spawnPoints)
            {
                stream.SendNext(spawnPoint.IsOccupied);
            }
        }
        else
        {
            foreach (var spawnPoint in _spawnPoints)
            {
                spawnPoint.IsOccupied = (bool)stream.ReceiveNext();
            }
        }
    }
}
