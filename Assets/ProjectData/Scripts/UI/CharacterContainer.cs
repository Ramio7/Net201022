using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

public class CharacterContainer : MonoBehaviour
{
    [SerializeField] private TMP_Text _characterName;
    [SerializeField] private TMP_Text _characterLevel;
    [SerializeField] private TMP_Text _characterCurrency;
    [SerializeField] private TMP_Text _createNewCharacter;
    private CharacterResult _characterInContainer;

    public void SetCharacterInfo(CharacterResult characterData)
    {
        PlayFabClientAPI.GetAllUsersCharacters(new ListUsersCharactersRequest(),
            result =>
            {
                var characterList = result.Characters;
                bool characterInfo(CharacterResult result) { return result.CharacterId == characterData.CharacterId; }
                if (characterList.Contains(characterData)) 
                {
                    _characterInContainer = characterList.Find(characterInfo);
                    SwitchCharacterIsSet(true);
                }
                else Debug.LogError("No such character find in account");
            },
            PlayFabAccountManager.OnError());
    }

    public CharacterResult GetCharacterInfo()
    {
        return _characterInContainer;
    }

    private void SwitchCharacterIsSet(bool isSet)
    {
        _createNewCharacter.gameObject.SetActive(!isSet);
        _characterLevel.gameObject.SetActive(isSet);
        _characterCurrency.gameObject.SetActive(isSet);
        _characterName.gameObject.SetActive(isSet);
    }
}
