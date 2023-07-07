using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RoomInfoContainer : MonoBehaviour
{
    [SerializeField] private TMP_Text _roomInfoText;

    public RoomInfo ButtonRoomInfo { get; private set; }

    public Button Button { get; private set; }

    public event Action<RoomInfo> OnRoomInfoContainerClick;

    private void Awake()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(GetRoomInfo);
    }

    private void OnDestroy() => Button.onClick.RemoveListener(GetRoomInfo);

    public void GetRoomInfo() => OnRoomInfoContainerClick?.Invoke(ButtonRoomInfo);

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
