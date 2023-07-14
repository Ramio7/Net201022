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
            if (!spawnPoint.SpawnPointClosed.GetValue()) freeSpawns.Add(spawnPoint);
        }
        var index = UnityEngine.Random.Range(0, freeSpawns.Count);
        _spawnPoints[index].SpawnPointClosed.SetValue(true);
        OnSpawnPointGranted?.Invoke(_spawnPoints[index].transform.position);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_spawnPoints);
        }
        else
        {
            _spawnPoints = (List<SpawnPoint>)stream.ReceiveNext();
        }
    }
}
