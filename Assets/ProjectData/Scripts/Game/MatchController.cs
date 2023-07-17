using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField, Tooltip("Match time in seconds")] private float _matchTimeTotal;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _gameResults;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _backToMenuButton;
    [SerializeField] private Canvas _matchResultCanvas;
    private ReactiveProperty<float> _matchTime = new(1);
    private List<PlayerGameStatistics> _gameStats = new();
    private float _endTime;

    public event Action OnTimeExpired;
    public event Action<bool> OnMatchEnd;
    public event Action OnMatchStart;

    public const int Kill_VC_Reward = 5;
    public const int Assist_VC_Reward = 1;
    public const int Kill_XP_Reward = 20;
    public const int Assist_XP_Reward = 10;

    public static MatchController Instance { get; private set; }
    public List<PlayerGameStatistics> GameStats { get => _gameStats; private set => _gameStats = value; }

    private void Awake()
    {
        Instance = this;
        _matchTime.Value = _matchTimeTotal;
        _matchTime.OnValueChanged += SetMatchTimerValue;
        _endTime = Time.time + _matchTimeTotal;
        OnMatchEnd += ShowMatchResults;
        _restartButton.onClick.AddListener(RestartMatch);
        _backToMenuButton.onClick.AddListener(PhotonManager.Instance.DisconnectPhoton);
    }

    private void FixedUpdate()
    {
        if (_matchTime.Value > 0) _matchTime.Value = _endTime - Time.time;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_matchTime.Value);
        }
        else
        {
            _matchTime.Value = (float)stream.ReceiveNext();
        }
    }

    private void RestartMatch()
    {
        OnMatchStart?.Invoke();
        _matchResultCanvas.enabled = false;
        SetMatchTimerValue(_matchTimeTotal);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ShowMatchResults(bool isEnded)
    {
        if (!isEnded) return;

        _matchResultCanvas.enabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetMatchTimerValue(float timeLeft)
    {
        if (_matchTime.Value <= 0)
        {
            OnTimeExpired?.Invoke();
            GrantRewardsForPlayers();
            return;
        }
        var timeInMinutes = timeLeft / 60;
        var minutesLeft = (int)timeInMinutes;
        int secondsLeft = (int)((timeInMinutes - minutesLeft) * 60);
        var secondsLeftFormated = string.Format("{0:d2}", secondsLeft);
        _timerText.text = string.Format($"{minutesLeft} : {secondsLeftFormated}");
    }

    private void GrantRewardsForPlayers()
    {
        foreach (var playerStats in _gameStats)
        {
            var stats = playerStats.PlayerStatistics;
            PlayFabClientAPI.GetAccountInfo(new()
            {
                Username = stats.Name.Value,
            },
            result =>
            {
                GrantVirtualCurrencyToPlayer(result, stats);
                GrantExperienceToPlayer(result, stats);
            },
            error =>
            {
                error.GenerateErrorReport();
            });
        }
        OnMatchEnd?.Invoke(true);
    }

    private void GrantVirtualCurrencyToPlayer(GetAccountInfoResult accountInfo, PlayerGameStatistics.GameStatistics stats)
    {
        PlayFabClientAPI.AddUserVirtualCurrency(new()
        {
            AuthenticationContext = accountInfo.Request.AuthenticationContext,
            VirtualCurrency = "CR",
            Amount = stats.Kills.Value * Kill_VC_Reward + stats.Assists.Value * Assist_VC_Reward,
        },
        result =>
        {
            _gameResults.text += $"\n{accountInfo.AccountInfo.Username} got {result.Balance} {result.VirtualCurrency} in total";
            Debug.Log($"Current {stats.Name} currency: {result.Balance}");
        },
        error =>
        {
            error.GenerateErrorReport();
        });
    }

    private void GrantExperienceToPlayer(GetAccountInfoResult accountInfo, PlayerGameStatistics.GameStatistics stats)
    {
        PlayFabClientAPI.GetUserData(new()
        {
            AuthenticationContext = accountInfo.Request.AuthenticationContext,
            PlayFabId = accountInfo.AccountInfo.PlayFabId,
            Keys = { "Experience" }
        },
        userData =>
        {
            PlayFabClientAPI.UpdateUserData(new()
            {
                AuthenticationContext = userData.Request.AuthenticationContext,
                Data = new()
            {
                { "Experience", (userData.Data["Experience"].Value + stats.Kills.Value * Kill_XP_Reward + stats.Assists.Value * Assist_XP_Reward).ToString() }
            }
            },
            result =>
            {
                _gameResults.text += $" and {userData.Data["Experience"].Value} exp";
            },
            error =>
            {
                error.GenerateErrorReport();
            });
        },
        error => { });

    }
}
