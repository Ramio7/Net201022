using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;
using System.Collections.Generic;

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

    public event Action<string, Color> OnCreateAccountMessageUpdate;
    public event Action<string, Color> OnLoginMessageUpdate;

    public static event Action<float, string> OnUserHpUpdate;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void CreatePlayFabAccount()
    {
        PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest
        {
            Username = _playFabUserName,
            Email = _playFabEmail,
            Password = _playFabPassword,
            RequireBothUsernameAndEmail = true,
        }, result =>
        {
            CreateUserXPData(result);
            Debug.Log($"Success: {_playFabUserName}");
            OnCreateAccountMessageUpdate.Invoke(result.ToString(), Color.green);
        }, error =>
        {
            Debug.LogError($"Fail: {error.ErrorMessage}");
            OnCreateAccountMessageUpdate.Invoke(error.ErrorMessage, Color.red);
        });
    }

    private void CreateUserXPData(RegisterPlayFabUserResult result)
    {
        PlayFabServerAPI.UpdateUserData(new()
        {
            AuthenticationContext = new()
            {
                PlayFabId = result.PlayFabId,
            },
            Data = new Dictionary<string, string>
            {
                { "Experience", "0"},
            },
            Permission = PlayFab.ServerModels.UserDataPermission.Public
        },
        result =>
        {
            Debug.Log(result.Request.ToString());
        }, OnError());
    }

    public void ConnectPlayFab()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = _playFabTitleId;

        if (PlayFabClientAPI.IsClientLoggedIn()) return;

        _request = new LoginWithPlayFabRequest
        {
            Username = _playFabLoginUsername,
            Password = _playFabLoginPassword,
        };

        PlayFabClientAPI.LoginWithPlayFab(
            _request,
            result =>
            {
                OnLoginMessageUpdate?.Invoke($"{result.PlayFabId} connected to PlayFab", Color.green);
            },
            error =>
            {
                OnError();
                OnLoginMessageUpdate?.Invoke(error.GenerateErrorReport(), Color.red);
            });
    }

    private Action<PlayFabError> OnError()
    {
        return error =>
        {
            Debug.Log(error.GenerateErrorReport());
        };
    }
}
