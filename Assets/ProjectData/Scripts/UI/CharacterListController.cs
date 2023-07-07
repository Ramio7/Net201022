using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class CharacterListController
{
    private List<CharacterContainer> _list = new(4);

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

    public void CreateCharacter()
    {

    }
}
