using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class CharacterListController
{
    private List<CharacterContainer> _list = new(4);
    private const float Character_Starting_Health = 100;
    private const int Character_Starting_Level = 1;
    private const int Character_Starting_Currency = 500;

    public CharacterListController(List<CharacterContainer> characterContainers)
    {
        _list.AddRange(characterContainers);
    }

    public void FillCharacterContainers(List<CharacterResult> characterList)
    {
        if (characterList.Count <= PlayFabAccountManager.Max_User_Characters - 1)
            for (int i = 0; i < characterList.Count; i++) _list[i].SetCharacterInfo(characterList[i]);
        else Debug.LogError("To much characters on user account");
    }

    public void CreateCharacter(string characterName)
    {
        PlayFabClientAPI.GrantCharacterToUser(new()
        {
            CharacterName = characterName,
            ItemId = "create_character_token",
            CatalogVersion = "Release"
        },
        result =>
        {
            result.CustomData = new Dictionary<string, string>() 
            {
                { "Health", Character_Starting_Health.ToString() },
                { "Level", Character_Starting_Level.ToString() },
                { "Currency", Character_Starting_Currency.ToString() }
            };
        }, PlayFabAccountManager.OnError());
    }

    public void DeleteCharacter(string playFabId, string characterId)
    {
        PlayFabServerAPI.DeleteCharacterFromUser(new()
        {
            PlayFabId = playFabId,
            CharacterId = characterId,
            SaveCharacterInventory = false
        },
        result =>
        {
            PlayFabServerAPI.GrantItemsToUser(new()
            {
                PlayFabId = playFabId,
                CatalogVersion = "Release"
            },
            result =>
            {
                result.ItemGrantResults.Add(new()
                {
                    ItemId = "create_character_token",
                });
            }, PlayFabAccountManager.OnError());
        }, PlayFabAccountManager.OnError());
    }

    public void UpdateCharacterInfo(CharacterContainer characterContainer)
    {
        PlayFabClientAPI.UpdateCharacterData(new()
        {
            CharacterId = characterContainer.CharacterInContainer.CharacterId,
        },
        result =>
        {
            bool characterInfo(CharacterContainer result) { return result.CharacterInContainer.CharacterId == characterContainer.CharacterInContainer.CharacterId; }
            if (_list.Contains(characterContainer))
            {
                var character = _list.Find(characterInfo);
                PlayFabClientAPI.GetAllUsersCharacters(new()
                {
                    PlayFabId = result.Request.AuthenticationContext.PlayFabId,
                },
                result =>
                {
                    bool characterResultPredicate(CharacterResult result) { return result.CharacterId == character.CharacterInContainer.CharacterId; }
                    var characterResult = result.Characters.Find(characterResultPredicate);
                    character.SetCharacterInfo(characterResult);
                }, PlayFabAccountManager.OnError());
            }
            else Debug.LogError("No such character find in account");
        }, PlayFabAccountManager.OnError());
    }
}
