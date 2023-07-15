using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class MatchController : IDisposable
{
    private ReactiveProperty<int> _matchTime = new(-1);
    private List<PlayerGameStatistics> _gameStats = new();

    public event Action OnTimeExpired;

    public const int Kill_VC_Reward = 5;
    public const int Assist_VC_Reward = 1;
    public const int Kill_XP_Reward = 20;
    public const int Assist_XP_Reward = 10;

    public static MatchController Instance { get; private set; }

    public MatchController(int matchTime)
    {
        Instance = this;
        _matchTime.SetValue(matchTime);
        StartMatchTimer();
    }

    public void Dispose()
    {

    }

    private async void StartMatchTimer()
    {
        await Task.Run(() => Countdown());
        OnTimeExpired?.Invoke();
    }

    private async Task Countdown()
    {
        while (_matchTime.GetValue() > 0)
        {
            await Task.Run(() => TimerTick());
            var newTime = _matchTime.GetValue() - 1;
            _matchTime.SetValue(newTime);
        }
        GrantRewardsForPlayers();
    }

    private Task TimerTick() => Task.Delay(1000);

    private void GetPlayerEndGameStatistics(PlayerGameStatistics playerStats) => _gameStats.Add(playerStats);

    private void GetEndGameStatistics(List<PlayerGameStatistics> gameStatistics)
    {
        foreach (var playerStats in gameStatistics) _gameStats.Add(playerStats);
    }

    private void GrantRewardsForPlayers()
    {
        foreach (var playerStats in _gameStats)
        {
            var stats = playerStats.PlayerStatistics;
            PlayFabClientAPI.GetAccountInfo(new()
            {
                Username = stats.Name.GetValue(),
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
    }

    private void GrantVirtualCurrencyToPlayer(GetAccountInfoResult result, PlayerGameStatistics.GameStatistics stats)
    {
        PlayFabServerAPI.AddUserVirtualCurrency(new()
        {
            PlayFabId = result.AccountInfo.PlayFabId,
            VirtualCurrency = "Credits",
            Amount = stats.Kills.GetValue() * Kill_VC_Reward + stats.Assists.GetValue() * Assist_VC_Reward,
        },
        result =>
        {
            Debug.Log($"Current {stats.Name} currency: {result.Balance}");
        },
        error =>
        {
            error.GenerateErrorReport();
        });
    }

    private void GrantExperienceToPlayer(GetAccountInfoResult result, PlayerGameStatistics.GameStatistics stats)
    {
        PlayFabServerAPI.GetTitleData(new()
        {
            AuthenticationContext = result.Request.AuthenticationContext,
            Keys = { "Experience" },
        },
        result =>
        {
            result.Data.TryGetValue("Experience", out string exp);
            if (!int.TryParse(exp, out var currentPlayerXP)) Debug.LogError("Failed parsing player experience");
            SetNewXPValue(result, stats, currentPlayerXP);
        },
        error =>
        {
            error.GenerateErrorReport();
        });
    }

    private void SetNewXPValue(PlayFab.ServerModels.GetTitleDataResult result, PlayerGameStatistics.GameStatistics stats, int currentPlayerXP)
    {
        PlayFabClientAPI.UpdateUserData(new()
        {
            AuthenticationContext = result.Request.AuthenticationContext,
            Data = new Dictionary<string, string>
                    {
                        { "Experience", (currentPlayerXP + stats.Kills.GetValue() * Kill_XP_Reward + stats.Assists.GetValue() * Assist_XP_Reward).ToString() }
                    },
            Permission = UserDataPermission.Public,
        },
        result =>
        {
        
        },
        error =>
        {
            error.GenerateErrorReport();
        });
    }
}
