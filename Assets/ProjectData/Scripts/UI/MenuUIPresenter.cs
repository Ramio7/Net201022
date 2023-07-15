using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIPresenter : MonoBehaviour
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

    [Header("Photon login screen")]
    [SerializeField] private Canvas _photonLoginScreenCanvas;
    [SerializeField] private TMP_InputField _roomNameInput;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;
    [SerializeField] private Button _backToMainMenuButton;
    [SerializeField] private Button _refreshRoomlistButton;
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
    [SerializeField] private TMP_Text _playerList;

    [Header("Utility")]
    [SerializeField] private PlayFabAccountManager _playFabAccountManager;
    [SerializeField] private PhotonManager _photonManager;
    [SerializeField] private RoomInfoContainer _roomContainerPrefab;
    [SerializeField] private GameObject _authenticationPrefab;

    private readonly List<Button> _buttons = new();
    private readonly List<Canvas> _canvas = new();
    private readonly List<TMP_InputField> _inputFields = new();
    private readonly List<Toggle> _toggles = new();
    private RoomListController _roomListController;
    private PlayerInRoomListController _playerInRoomListController;

    private void Awake()
    {
        _photonManager = FindFirstObjectByType<PhotonManager>();
        _playFabAccountManager = FindFirstObjectByType<PlayFabAccountManager>();
        GameObject auth;
        if (_playFabAccountManager == null || _photonManager == null)
        {
            auth = Instantiate(_authenticationPrefab, null);
            _photonManager = auth.GetComponent<PhotonManager>();
            _playFabAccountManager = auth.GetComponent<PlayFabAccountManager>();
        }

        SubscribeEvents();
        RegisterCanvas();

        if (PhotonNetwork.IsConnected) SetPhotonLogInCanvasActive();
        else SetMainMenuCanvasActive();

        _playFabAccountManager.OnCreateAccountMessageUpdate += UpdateCreateAccountMessage;
        _playFabAccountManager.OnLoginMessageUpdate += UpdateLoginMessage;
    }

    private void StartRoomListController() => _roomListController = new RoomListController(_roomListTransform, _roomContainerPrefab);

    private void RegisterCanvas()
    {
        _canvas.Add(_photonLoginScreenCanvas);
        _canvas.Add(_playFabLogInSreenCanvas);
        _canvas.Add(_createAccountMenuCanvas);
        _canvas.Add(_mainMenuCanvas);
        _canvas.Add(_roomPropertiesCanvas);
        _canvas.Add(_roomCanvas);
    }

    private void OnDisable()
    {
        if (_photonManager != null && _playFabAccountManager != null) UnsubscribeEvents();
        _roomListController?.Dispose();
        _playerInRoomListController?.Dispose();
    }

    private void SubscribeEvents()
    {
        SubcribeMainMenuEvents();
        SubscribeCreateAccountMenuEvents();
        SubscribePlayFabLoginMenuEvents();
        SubscribePhotonLoginMenuEvents();
        SubscribeCreateRoomEvents();
        SubscribeRoomEvents();
    }

    private void UnsubscribeEvents()
    {
        foreach (var button in _buttons) button.onClick?.RemoveAllListeners();
        foreach (var inputField in _inputFields) inputField.onValueChanged?.RemoveAllListeners();
        foreach (var toggle in _toggles) toggle.onValueChanged?.RemoveAllListeners();

        foreach (var roomInfoContainer in _roomListController.RoomList) roomInfoContainer.OnRoomInfoContainerClick -= JoinOutlinedRoom;
        _playFabAccountManager.OnCreateAccountMessageUpdate -= UpdateCreateAccountMessage;
        _playFabAccountManager.OnLoginMessageUpdate -= UpdateLoginMessage;
    }

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
        _usernameLoginInput.onValueChanged.AddListener(UpdateLoginUsername);
        _inputFields.Add(_usernameLoginInput);

        _passwordLoginInput.onValueChanged.AddListener(UpdatePlayFabLoginPassword);
        _inputFields.Add(_passwordLoginInput);

        _logInAccountButton.onClick.AddListener(_playFabAccountManager.ConnectPlayFab);
        _logInAccountButton.onClick.AddListener(SetPhotonLogInCanvasActive);
        _buttons.Add(_logInAccountButton);

        _backToMenuButton.onClick.AddListener(SetMainMenuCanvasActive);
        _buttons.Add(_backToMenuButton);
    }

    private void SubscribePhotonLoginMenuEvents()
    {
        _roomNameInput.onValueChanged.AddListener(UpdatePhotonRoomname);
        _inputFields.Add(_roomNameInput);

        _createRoomButton.onClick.AddListener(SetRoomPropertiesCanvasActive);
        _buttons.Add(_createRoomButton);

        _joinRoomButton.onClick.AddListener(JoinRoom);
        _joinRoomButton.onClick.AddListener(SetRoomCanvasActive);
        _buttons.Add(_joinRoomButton);

        _photonManager.OnClientStateChanged += UpdatePhotonClientStateOutput;

        _backToMainMenuButton.onClick.AddListener(SetMainMenuCanvasActive);
        _backToMainMenuButton.onClick.AddListener(_photonManager.DisconnectPhoton);
        _buttons.Add(_backToMainMenuButton);

        _refreshRoomlistButton.onClick.AddListener(_photonManager.RoomListUpdate);
        _buttons.Add(_refreshRoomlistButton);
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

    private void JoinRoom()
    {
        if (_roomNameInput.text != string.Empty) _photonManager.JoinRoom(_roomNameInput.text);
        else _photonManager.JoinRandomRoom();
        SetRoomCanvasActive();
    }

    private void JoinOutlinedRoom(RoomInfo roomInfo)
    {
        _photonManager.JoinRoom(roomInfo);
        SetRoomCanvasActive();
    }

    private void UpdatePlayFabUsername(string username) => _playFabAccountManager.PlayFabUserName = username;
    private void UpdatePlayFabPassword(string password) => _playFabAccountManager.PlayFabPassWord = password;
    private void UpdatePlayFabEmail(string email) => _playFabAccountManager.EMail = email;
    private void UpdateLoginUsername(string username)
    {
        _playFabAccountManager.PlayFabLoginUsername = username;
        _photonManager.PhotonUsername = username;
    }
    private void UpdatePlayFabLoginPassword(string password) => _playFabAccountManager.PlayFabLoginPassword = password;

    private void UpdatePhotonRoomname(string roomname)
    {
        _photonManager.Roomname = roomname;
    }

    private void UpdatePrivateRoomToggle(bool toggleIsOn) => _photonManager.RoomIsPrivate = _privateRoomToggle.isOn;

    private void SetCanvasActive(Canvas canvasToActivate)
    {
        foreach (var canvas in _canvas) if (canvas.enabled == true) canvas.enabled = false;
        canvasToActivate.enabled = true;
    }

    private void SetCanvasActiveSelf(Canvas canvasToActivate) => canvasToActivate.enabled = true;
    private void SetCanvasUnactiveSelf(Canvas canvasToUnactivate) => canvasToUnactivate.enabled = false;
    public void SetCreateAccountCanvasActive() => SetCanvasActive(_createAccountMenuCanvas);
    private void SetPlayFabLogInCanvasActive()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            SetPhotonLogInCanvasActive();
            return;
        }

        if (_createAccountMenuCanvas.enabled == true)
        {
            if (_createAccountMessage.color == Color.red) return;
        }
        SetCanvasActive(_playFabLogInSreenCanvas);
    }
    private async void SetPhotonLogInCanvasActive()
    {
        await Task.Run(() => WaitPlayFabLogin());
        _photonManager.ConnectPhoton();
        await Task.Run(() => WaitPhotonLogin());
        StartRoomListController();
        SetCanvasActive(_photonLoginScreenCanvas);
    }
    private void SetMainMenuCanvasActive() => SetCanvasActive(_mainMenuCanvas);
    private async void SetRoomCanvasActive()
    {
        await Task.Run(() => WaitRoomJoin());
        _playerInRoomListController = new(_playerList);
        SetCanvasActive(_roomCanvas);
        if (!PhotonNetwork.IsMasterClient) _startGameButton.interactable = false;
    }

    private void SetRoomPropertiesCanvasActive() => SetCanvasActiveSelf(_roomPropertiesCanvas);
    private void SetRoomPropertiesCanvasUnactive() => SetCanvasUnactiveSelf(_roomPropertiesCanvas);

    private Task WaitPlayFabLogin()
    {
        while (!PlayFabClientAPI.IsClientLoggedIn()) Task.Delay(100);
        return Task.FromResult(0);
    }

    private Task WaitPhotonLogin()
    {
        while (PhotonNetwork.NetworkClientState.ToString() != "ConnectedToMasterServer") Task.Delay(100);
        return Task.FromResult(0);
    }

    private Task WaitRoomJoin()
    {
        while (!PhotonNetwork.InRoom) Task.Delay(100);
        return Task.FromResult(0);
    }

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
}
