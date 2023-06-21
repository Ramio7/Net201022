using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;

public class PlayFabAccountManager : MonoBehaviour
{
    [SerializeField] private string _playFabTitleId;
    [SerializeField] private string _playFabUserName;
    [SerializeField] private string _playFabPassword;
    [SerializeField] private string _playFabEmail;
    [SerializeField] private string _playFabLoginUsername;
    [SerializeField] private string _playFabLoginPassword;
    private LoginWithPlayFabRequest _request;

    public string PlayFabUserName { get => _playFabUserName; set => _playFabUserName = value; }
    public string PlayFabPassWord { get => _playFabPassword; set => _playFabPassword = value; }
    public string EMail { get => _playFabEmail; set => _playFabEmail = value; }
    public string PlayFabLoginUsername { get => _playFabLoginUsername; set => _playFabLoginUsername = value; }
    public string PlayFabLoginPassword { get => _playFabLoginPassword; set => _playFabLoginPassword = value; }

    public event Action<string, Color> PlayFabMessage;

    public void CreatePlayFabAccount()
    {
        PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest
        {
            Username = _playFabUserName,
            Email = _playFabEmail,
            Password = _playFabPassword,
            RequireBothUsernameAndEmail = true
        }, result =>
        {
            Debug.Log($"Success: {_playFabUserName}");
        }, error =>
        {
            Debug.LogError($"Fail: {error.ErrorMessage}");
        });
    }

    public void ConnectPlayFab()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = _playFabTitleId;

        _request = new LoginWithPlayFabRequest
        {
            Username = _playFabLoginUsername,
            Password = _playFabLoginPassword,
        };

        PlayFabClientAPI.LoginWithPlayFab(
            _request,
            result =>
            {
                Debug.Log(result.PlayFabId);
                PlayFabMessage.Invoke($"{result.PlayFabId} connected to PlayFab", Color.green);
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
                PlayFabMessage.Invoke(error.GenerateErrorReport(), Color.red);
            });
    }
}
