using Photon.Pun.Demo.PunBasics;
using PlayFab;
using System;
using UnityEngine;

public class PlayerHealthUpdater : MonoBehaviour
{
    [SerializeField] PlayerManager _playerManager;
    private string _playFabId;

    private void Awake()
    {
        PlayFabAccountManager.OnUserHpUpdate += SetUserStartingHp;
        _playerManager.OnPlayerHpValueChanged += UpdatePlayFabPlayerHp;
    }

    private void SetUserStartingHp(float hp, string playFabId)
    {
        _playerManager.Health = hp;
        _playFabId = playFabId;
    }

    private void UpdatePlayFabPlayerHp(float hp)
    {
        PlayFabClientAPI.UpdateUserData(new() 
        { 
            Data = new()
            {
                { "Health", hp.ToString() }
            }
        },
        result =>
        {
            PlayFabClientAPI.GetUserData(new()
            {
                PlayFabId = _playFabId
            },
            result =>
            {
                Debug.Log($"Current player PlayFab HP: {result.Data["Health"].Value}");
            },
            OnError());
        },
        OnError());
    }

    private Action<PlayFabError> OnError()
    {
        return error =>
        {
            Debug.Log(error.GenerateErrorReport());
        };
    }
}
