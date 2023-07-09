using Photon.Realtime;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPresenter : MonoBehaviour
{
    [Header("Main menu")]
    [SerializeField] private Canvas _mainMenuCanvas;
    [SerializeField] private Button _createAccountMenuButton;
    [SerializeField] private Button _logInPlayFabButton;
    [SerializeField] private Button _exitButton;

    [Header("Create account menu")]
    [SerializeField] private Canvas _createAccountMenuCanvas;
    [SerializeField] private TMP_InputField _emailInput;
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private Button _createNewAccountButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_Text _createAccountMessage;

    [Header("PlayFab log in screen")]
    [SerializeField] private Canvas _playFabLogInSreenCanvas;
    [SerializeField] private TMP_InputField _usernameLoginInput;
    [SerializeField] private TMP_InputField _passwordLoginInput;
    [SerializeField] private Button _logInAccountButton;
    [SerializeField] private Button _backToMenuButton;
    [SerializeField] private TMP_Text _loginMessage;

    [Header("Character manager screen")]
    [SerializeField] private Canvas _characterManagerScreenCanvas;
    [SerializeField] private List<CharacterContainer> _characterContainerList;
    [SerializeField] private Button _exitToMainMenuButton;
    [SerializeField] private Canvas _createCharacterScreenCanvas;
    [SerializeField] private TMP_InputField _characterNameInput;
    [SerializeField] private Button _createCharacterButton;
    [SerializeField] private Button _cancelCharacterCreationButton;

    [Header("Photon login screen")]
    [SerializeField] private Canvas _photonLoginScreenCanvas;
    [SerializeField] private TMP_InputField _roomNameInput;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;
    [SerializeField] private Button _backToMainMenuButton;
    [SerializeField] private TMP_Text _photonLoginMessage;
    [SerializeField] private Transform _roomListTransform;

    [Header("Room properties screen")]
    [SerializeField] private Canvas _roomPropertiesCanvas;
    [SerializeField] private TMP_InputField _roomNameInputToCreate;
    [SerializeField] private Button _createRoomInPropertiesButton;
    [SerializeField] private Button _backToLobbyButton;
    [SerializeField] private Toggle _privateRoomToggle;

    [Header("Room screen")]
    [SerializeField] private Canvas _roomCanvas;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _backToLobbyRoomButton;

    [Header("Utility")]
    [SerializeField] private PlayFabAccountManager _playFabAccountManager;
    [SerializeField] private PhotonManager _photonManager;
    [SerializeField] private RoomInfoContainer _roomContainerPrefab;

    private readonly List<Button> _buttons = new();
    private readonly List<Canvas> _canvas = new();
    private readonly List<TMP_InputField> _inputFields = new();
    private readonly List<Toggle> _toggles = new();
    private RoomListController _roomListController;
    private CharacterListController _characterListController;

    #region Lifecycle Methods
    private void Awake()
    {
        StartRoomListController();
        RegisterCanvas();
        SubscribeEvents();
        SetMainMenuCanvasActive();

        _playFabAccountManager.OnCreateAccountMessageUpdate += UpdateCreateAccountMessage;
        _playFabAccountManager.OnLoginMessageUpdate += UpdateLoginMessage;
    }

    private void StartRoomListController()
    {
        _roomListController = new RoomListController(_roomListTransform, _roomContainerPrefab);
        _photonManager.OnRoomListUpdated += UpdateRoomInfoContainers;
    }

    private void RegisterCanvas()
    {
        _canvas.Add(_photonLoginScreenCanvas);
        _canvas.Add(_playFabLogInSreenCanvas);
        _canvas.Add(_createAccountMenuCanvas);
        _canvas.Add(_mainMenuCanvas);
        _canvas.Add(_roomPropertiesCanvas);
        _canvas.Add(_roomCanvas);
        _canvas.Add(_characterManagerScreenCanvas);
        _canvas.Add(_createCharacterScreenCanvas);
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        _roomListController?.Dispose();
    }

    private void SubscribeEvents()
    {
        SubcribeMainMenuEvents();
        SubscribeCreateAccountMenuEvents();
        SubscribePlayFabLoginMenuEvents();
        SubscribePhotonLoginMenuEvents();
        SubscribeCreateRoomEvents();
        SubscribeRoomEvents();
        SubcribeCharacterManagerEvents();
    }

    private void UnsubscribeEvents()
    {
        foreach (var button in _buttons) button.onClick?.RemoveAllListeners();
        foreach (var inputField in _inputFields) inputField.onValueChanged?.RemoveAllListeners();
        foreach (var toggle in _toggles) toggle.onValueChanged?.RemoveAllListeners();

        _photonManager.OnRoomListUpdated -= UpdateRoomInfoContainers;
        foreach (var roomInfoContainer in _roomListController.RoomList) roomInfoContainer.OnRoomInfoContainerClick -= JoinOutlinedRoom;
        foreach (var container in _characterContainerList) container.OnCharacterNameSend -= UpdatePhotonLoginUsername;
        _playFabAccountManager.OnCreateAccountMessageUpdate -= UpdateCreateAccountMessage;
        _playFabAccountManager.OnLoginMessageUpdate -= UpdateLoginMessage;
    }
    #endregion

    #region Event Management Methods
    private void SubcribeMainMenuEvents()
    {
        _createAccountMenuButton.onClick.AddListener(SetCreateAccountCanvasActive);
        _buttons.Add(_createAccountMenuButton);

        _logInPlayFabButton.onClick.AddListener(SetPlayFabLogInCanvasActive);
        _buttons.Add(_logInPlayFabButton);

        _exitButton.onClick.AddListener(Application.Quit);
        _buttons.Add(_exitButton);
    }

    private void SubscribeCreateAccountMenuEvents()
    {
        _usernameInput.onValueChanged.AddListener(UpdatePlayFabUsername);
        _inputFields.Add(_usernameInput);

        _passwordInput.onValueChanged.AddListener(UpdatePlayFabPassword);
        _inputFields.Add(_passwordInput);

        _emailInput.onValueChanged.AddListener(UpdatePlayFabEmail);
        _inputFields.Add(_emailInput);

        _createNewAccountButton.onClick.AddListener(_playFabAccountManager.CreatePlayFabAccount);
        _createNewAccountButton.onClick.AddListener(SetPlayFabLogInCanvasActive);
        _buttons.Add(_createNewAccountButton);

        _backButton.onClick.AddListener(SetMainMenuCanvasActive);
        _buttons.Add(_backButton);
    }

    private void SubscribePlayFabLoginMenuEvents()
    {
        _usernameLoginInput.onValueChanged.AddListener(UpdatePlayFabLoginUsername);
        _inputFields.Add(_usernameLoginInput);

        _passwordLoginInput.onValueChanged.AddListener(UpdatePlayFabLoginPassword);
        _inputFields.Add(_passwordLoginInput);

        _logInAccountButton.onClick.AddListener(_playFabAccountManager.ConnectPlayFab);
        _logInAccountButton.onClick.AddListener(SetCharacterManagerCanvasActive);
        _buttons.Add(_logInAccountButton);

        _backToMenuButton.onClick.AddListener(SetMainMenuCanvasActive);
        _buttons.Add(_backToMenuButton);
    }

    private void SubcribeCharacterManagerEvents()
    {
        _exitToMainMenuButton.onClick.AddListener(SetMainMenuCanvasActive);
        foreach (var container in _characterContainerList)
        {
            container.OnCharacterNameSend += UpdatePhotonLoginUsername;
            if (container.CharacterInContainer != null)
            {
                container.GetComponent<Button>().onClick.AddListener(container.GetCharacterInfo);
                container.GetComponent<Button>().onClick.AddListener(_photonManager.ConnectPhoton);
                container.GetComponent<Button>().onClick.AddListener(SetPhotonLogInCanvasActive);
            }
            else
            {
                container.GetComponent<Button>().onClick.AddListener(SetCreateCharacterCanvasActive);
            }
            _buttons.Add(container.GetComponent<Button>());
        }

        _characterNameInput.onValueChanged.AddListener(UpdatePhotonLoginUsername);
        _inputFields.Add(_characterNameInput);

        _createCharacterButton.onClick.AddListener(CreateCharacter);
        _createCharacterButton.onClick.AddListener(SetPhotonLogInCanvasActive);
        _createCharacterButton.onClick.AddListener(_photonManager.ConnectPhoton);
        _buttons.Add(_createCharacterButton);

        _cancelCharacterCreationButton.onClick.AddListener(SetCreateCharacterCanvasUnactive);
        _buttons.Add(_cancelCharacterCreationButton);
    }

    private void SubscribePhotonLoginMenuEvents()
    {
        _roomNameInput.onValueChanged.AddListener(UpdatePhotonRoomname);
        _inputFields.Add(_roomNameInput);

        _createRoomButton.onClick.AddListener(SetRoomPropertiesCanvasActive);
        _buttons.Add(_createRoomButton);

        _joinRoomButton.onClick.AddListener(JoinRoom);
        _buttons.Add(_joinRoomButton);

        _photonManager.OnClientStateChanged += UpdatePhotonClientStateOutput;

        _backToMainMenuButton.onClick.AddListener(SetMainMenuCanvasActive);
        _backToMainMenuButton.onClick.AddListener(_photonManager.DisconnectPhoton);
        _buttons.Add(_backToMainMenuButton);
    }

    private void SubscribeCreateRoomEvents()
    {
        _roomNameInputToCreate.onValueChanged.AddListener(UpdatePhotonRoomname);
        _inputFields.Add(_roomNameInputToCreate);

        _createRoomInPropertiesButton.onClick.AddListener(_photonManager.CreateRoom);
        _createRoomInPropertiesButton.onClick.AddListener(SetRoomCanvasActive);
        _buttons.Add(_createRoomInPropertiesButton);

        _backToLobbyButton.onClick.AddListener(SetRoomPropertiesCanvasUnactive);
        _buttons.Add(_backToLobbyButton);

        _privateRoomToggle.onValueChanged.AddListener(UpdatePrivateRoomToggle);
        _toggles.Add(_privateRoomToggle);
    }

    private void SubscribeRoomEvents()
    {
        _startGameButton.onClick.AddListener(_photonManager.StartTheGame);
        _buttons.Add(_startGameButton);

        _backToLobbyRoomButton.onClick.AddListener(SetPhotonLogInCanvasActive);
        _backToLobbyRoomButton.onClick.AddListener(_photonManager.LeaveCurrentRoom);
        _buttons.Add(_backToLobbyRoomButton);
    }

    private void UpdateRoomInfoContainers(List<RoomInfo> roomList)
    {
        _roomListController.UpdateRoomList(roomList);
        foreach (var roomInfoContainer in _roomListController.RoomList) roomInfoContainer.OnRoomInfoContainerClick += JoinOutlinedRoom;
    }
    #endregion

    #region Room Management Methods
    private void JoinRoom()
    {
        if (_roomNameInput.text != string.Empty) _photonManager.JoinRoom(_roomNameInput.text);
        else _photonManager.JoinRandomRoom();
    }
    private void JoinOutlinedRoom(RoomInfo roomInfo) => _photonManager.JoinRoom(roomInfo);
    #endregion

    #region Fields Update Methods
    private void UpdatePlayFabUsername(string username) => _playFabAccountManager.PlayFabUserName = username;
    private void UpdatePlayFabPassword(string password) => _playFabAccountManager.PlayFabPassWord = password;
    private void UpdatePlayFabEmail(string email) => _playFabAccountManager.EMail = email;
    private void UpdatePlayFabLoginUsername(string username) => _playFabAccountManager.PlayFabLoginUsername = username;
    private void UpdatePhotonLoginUsername(string username) => _photonManager.PhotonUsername = username;
    private void UpdatePlayFabLoginPassword(string password) => _playFabAccountManager.PlayFabLoginPassword = password;
    private void UpdatePhotonRoomname(string roomname)
    {
        _photonManager.Roomname = roomname;
    }
    private void UpdatePrivateRoomToggle(bool toggleIsOn) => _photonManager.RoomIsPrivate = _privateRoomToggle.isOn;
    private void UpdateCreateAccountMessage(string message, Color color)
    {
        _createAccountMessage.text = message;
        _createAccountMessage.color = color;
        Debug.Log(_createAccountMessage.ToString());
    }
    private void UpdateLoginMessage(string message, Color color)
    {
        _loginMessage.text = message;
        _loginMessage.color = color;
        Debug.Log(_loginMessage.ToString());
    }
    private void UpdatePhotonClientStateOutput(string state)
    {
        _photonLoginMessage.text = state;
        Debug.Log(state);
    }
    #endregion

    #region Canvas Switch Methods
    private void SetCanvasActive(Canvas canvasToActivate)
    {
        foreach (var canvas in _canvas) if (canvas.enabled == true) canvas.enabled = false;
        canvasToActivate.enabled = true;
    }
    private void SetCanvasActiveSelf(Canvas canvasToActivate)
    {
        canvasToActivate.enabled = true;
    }
    private void SetCanvasUnactiveSelf(Canvas canvasToUnactivate) => canvasToUnactivate.enabled = false;
    private void SetCreateAccountCanvasActive() => SetCanvasActive(_createAccountMenuCanvas);
    private void SetPlayFabLogInCanvasActive()
    {
        if (_createAccountMenuCanvas.enabled == true)
        {
            if (_createAccountMessage.color == Color.red) return;
        }
        SetCanvasActive(_playFabLogInSreenCanvas);
    }
    private void SetPhotonLogInCanvasActive()
    {
        SetCanvasActive(_photonLoginScreenCanvas);
    }
    private void SetMainMenuCanvasActive() => SetCanvasActive(_mainMenuCanvas);
    private void SetRoomCanvasActive() => SetCanvasActive(_roomCanvas);
    private void SetRoomPropertiesCanvasActive() => SetCanvasActiveSelf(_roomPropertiesCanvas);
    private void SetRoomPropertiesCanvasUnactive() => SetCanvasUnactiveSelf(_roomPropertiesCanvas);
    private async void SetCharacterManagerCanvasActive()
    {
        await Task.Run(() => WaitPlayFabLogin());
        _characterListController = new(_characterContainerList);
        var playerCharacters = _playFabAccountManager.GetCharacterList();
        _characterListController.FillCharacterContainers(playerCharacters);
        SetCanvasActive(_characterManagerScreenCanvas);
    }
    private void SetCreateCharacterCanvasActive() => SetCanvasActiveSelf(_createCharacterScreenCanvas);
    private void SetCreateCharacterCanvasUnactive() => SetCanvasUnactiveSelf(_createCharacterScreenCanvas);

    private Task WaitPlayFabLogin()
    {
        while (!PlayFabClientAPI.IsClientLoggedIn()) Task.Delay(100);
        return Task.FromResult(0);
    }
    #endregion

    private void CreateCharacter() => _characterListController.CreateCharacter(_photonManager.PhotonUsername);
}
