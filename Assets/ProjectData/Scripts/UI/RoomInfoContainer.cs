using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;

public class RoomInfoContainer : MonoBehaviour
{
    [SerializeField] private TMP_Text _roomInfoText;

    public RoomInfo ButtonRoomInfo { get; private set; }

    public event Action<RoomInfo> OnRoomInfoContainerClick;

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        ButtonRoomInfo = roomInfo;
        _roomInfoText.text = roomInfo.ToStringFull();
    }

    public void ClearRoomInfo()
    {
        ButtonRoomInfo = null;
        _roomInfoText.text = string.Empty;
    }
}
