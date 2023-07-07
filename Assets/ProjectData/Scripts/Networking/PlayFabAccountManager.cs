using PlayFab;
using PlayFab.ClientModels;
using PlayFab.ServerModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using CharacterResult = PlayFab.ClientModels.CharacterResult;

public class PlayFabAccountManager : MonoBehaviour
{
    [SerializeField] private string _playFabTitleId;
    [SerializeField] private string _playFabUserName;
    [SerializeField] private string _playFabPassword;
    [SerializeField] private string _playFabEmail;
    [SerializeField] private string _playFabLoginUsername;
    [SerializeField] private string _playFabLoginPassword;
    private LoginWithPlayFabRequest _request;
    private string _playFabId;

    public const int Max_User_Characters = 4;
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
            RequireBothUsernameAndEmail = true
        }, result =>
        {
            _playFabId = result.PlayFabId;
            Debug.Log($"Success: {_playFabUserName}");
            GrantStartingItemsToUser(result);
            OnCreateAccountMessageUpdate.Invoke(result.ToString(), Color.green);
        }, error =>
        {
            Debug.LogError($"Fail: {error.ErrorMessage}");
            OnCreateAccountMessageUpdate.Invoke(error.ErrorMessage, Color.red);
        });
    }

    private void GrantStartingItemsToUser(RegisterPlayFabUserResult result)
    {
        PlayFabServerAPI.GrantItemsToUser(new()
        {
            PlayFabId = result.PlayFabId,
        },
                    result =>
                    {
                        for (int i = 0; i < Max_User_Characters; i++) result.ItemGrantResults.Add(new GrantedItemInstance() { ItemId = "create_character_token" });
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
                OnLoginSuccess(result.PlayFabId);
                OnLoginMessageUpdate.Invoke($"{result.PlayFabId} connected to PlayFab", Color.green);
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
                OnLoginMessageUpdate.Invoke(error.GenerateErrorReport(), Color.red);
            });
    }

    private void OnLoginSuccess(string playFabId)
    {
        _playFabId = playFabId;
        PlayFabClientAPI.UpdateUserData(new()
        {
            Data = new()
            {
                { "Health", 100.ToString() }
            }
        },
            result =>
            {
                PlayFabClientAPI.GetUserData(new()
                {
                    PlayFabId = playFabId,
                },
                result =>
                {
                    if (float.TryParse(result.Data["Health"].Value, out var userHP)) OnUserHpUpdate?.Invoke(userHP, playFabId);
                    else return;
                    Debug.LogWarning($"User startin HP: {userHP}");
                },
                OnError());
            },
            OnError());
    }

    public static Action<PlayFabError> OnError()
    {
        return error =>
        {
            Debug.Log(error.GenerateErrorReport());
        };
    }

    public List<CharacterResult> GetCharacterList()
    {
        List<CharacterResult> characterList = new();
        PlayFabClientAPI.GetAllUsersCharacters(new()
        {
            PlayFabId = _playFabId,
        },
        result =>
        {
            characterList.AddRange(result.Characters);
        }, OnError());

        return characterList;
    }
}
