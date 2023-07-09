using PlayFab;
using PlayFab.ClientModels;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CharacterContainer : MonoBehaviour
{
    [SerializeField] private TMP_Text _characterName;
    [SerializeField] private TMP_Text _characterLevel;
    [SerializeField] private TMP_Text _characterCurrency;
    [SerializeField] private TMP_Text _createNewCharacter;

    public CharacterResult CharacterInContainer { get; private set; }

    public event Action<CharacterResult> OnContainerButtonClicked;
    public event Action<string> OnCharacterNameSend;

    public void SetCharacterInfo(CharacterResult characterData)
    {
        PlayFabClientAPI.GetAllUsersCharacters(new ListUsersCharactersRequest(),
            result =>
            {
                var characterList = result.Characters;
                bool characterInfo(CharacterResult result) { return result.CharacterId == characterData.CharacterId; }
                if (characterList.Contains(characterData)) 
                {
                    CharacterInContainer = characterList.Find(characterInfo);
                    SwitchCharacterIsSet(true);
                }
                else Debug.LogError("No such character find in account");
            },
            PlayFabAccountManager.OnError());
    }

    public void GetCharacterInfo()
    {
        OnContainerButtonClicked.Invoke(CharacterInContainer);
        OnCharacterNameSend.Invoke(CharacterInContainer.CharacterName);
    }

    private void SwitchCharacterIsSet(bool isSet)
    {
        _createNewCharacter.gameObject.SetActive(!isSet);
        _characterLevel.gameObject.SetActive(isSet);
        _characterCurrency.gameObject.SetActive(isSet);
        _characterName.gameObject.SetActive(isSet);
    }
}
