using Photon.Realtime;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public class RoomListController : IDisposable
{
    public List<RoomInfoContainer> _roomList { get; private set; } = new();
    private Transform _roomListContainerTransform;
    private RoomInfoContainer _roomInfoButtonPrefab;

    public RoomListController(Transform roomListContainerTransform, RoomInfoContainer roomInfoButtonPrefab)
    {
        _roomListContainerTransform = roomListContainerTransform;
        _roomInfoButtonPrefab = roomInfoButtonPrefab;
    }

    public void Dispose()
    {
        foreach (var roomInfoContainer in _roomList)
        {
            roomInfoContainer.ClearRoomInfo();
            Object.Destroy(roomInfoContainer.gameObject);
        }

        _roomInfoButtonPrefab = null;
        _roomListContainerTransform = null;
    }

    private void CreateRoomInfoContainer(RoomInfo roomInfo)
    {
        var newRoomButton = Object.Instantiate(_roomInfoButtonPrefab);
        newRoomButton.transform.SetParent(_roomListContainerTransform, false);
        newRoomButton.TryGetComponent<RoomInfoContainer>(out var roomInfoContainer);
        roomInfoContainer.SetRoomInfo(roomInfo);
        _roomList.Add(roomInfoContainer);
    }

    private void SetRoomContainerActive(RoomInfo roomInfo, int indexInRoomList)
    {
        _roomList[indexInRoomList].gameObject.SetActive(true);
        _roomList[indexInRoomList].SetRoomInfo(roomInfo);
    }

    private void SetAllRoomContainersUnactive()
    {
        foreach (var roomInfoContainer in _roomList)
        {
            roomInfoContainer.ClearRoomInfo();
            roomInfoContainer.gameObject.SetActive(false);
        }
    }

    public void UpdateRoomList(List<RoomInfo> roomInfos)
    {
        SetAllRoomContainersUnactive();

        foreach (var roomInfo in roomInfos)
        {
            for (var i = 0; i < _roomList.Count; i++)
            {

                if ((_roomList[i] == null && i == 0) || (_roomList[i].gameObject.activeSelf == true && i == _roomList.Count - 1)) continue;
                SetRoomContainerActive(roomInfo, i);
            }

            CreateRoomInfoContainer(roomInfo);
        }
    }
}
