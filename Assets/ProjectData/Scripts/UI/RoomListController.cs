using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class RoomListController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform _roomListContainerTransform;
    [SerializeField] private RoomInfoContainer _roomInfoButtonPrefab;

    private List<RoomInfoContainer> _roomList = new();

    private void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
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

    private void CreateRoomInfoContainer(RoomInfo roomInfo)
    {
        var newRoomButton = Instantiate(_roomInfoButtonPrefab, _roomListContainerTransform);
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
}
