using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class RoomListController : IDisposable
{
    public List<RoomInfoContainer> RoomList { get; private set; } = new();
    private Transform _roomListContainerTransform;
    private RoomInfoContainer _roomInfoButtonPrefab;

    public RoomListController(Transform roomListContainerTransform, RoomInfoContainer roomInfoButtonPrefab)
    {
        _roomListContainerTransform = roomListContainerTransform;
        _roomInfoButtonPrefab = roomInfoButtonPrefab;
    }

    public void Dispose()
    {
        foreach (var roomInfoContainer in RoomList)
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
        if (!newRoomButton.TryGetComponent<RoomInfoContainer>(out var roomInfoContainer))
        {
            Debug.LogError("No room info container on Room Button");
            return;
        }
        roomInfoContainer.SetRoomInfo(roomInfo);
        RoomList.Add(roomInfoContainer);
    }

    private void SetRoomContainerActive(RoomInfo roomInfo, int indexInRoomList)
    {
        RoomList[indexInRoomList].gameObject.SetActive(true);
        RoomList[indexInRoomList].SetRoomInfo(roomInfo);
    }

    private void SetAllRoomContainersUnactive()
    {
        foreach (var roomInfoContainer in RoomList)
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
            for (var i = 0; i < RoomList.Count; i++)
            {

                if ((RoomList[i] == null && i == 0) || (RoomList[i].gameObject.activeSelf == true && i == RoomList.Count - 1)) continue;
                SetRoomContainerActive(roomInfo, i);
            }

            CreateRoomInfoContainer(roomInfo);
        }
    }
}
