using Photon.Realtime;
using PlayFab;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPresenter : MonoBehaviour // —делать возможность создани€ приватной комнаты
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
    [SerializeField] private Button _backToPlayFabButton;
    [SerializeField] private TMP_Text _photonLoginMessage;
    [SerializeField] private Transform _roomListTransform;

    [Header("Utility")]
    [SerializeField] private PlayFabAccountManager _playFabAccountManager;
    [SerializeField] private PhotonManager _photonManager;
    [SerializeField] private PhotonLobbyManager _lobbyManager;
    [SerializeField] private RoomInfoContainer _roomContainerPrefab;

    private List<Button> _buttons = new();
    private List<Canvas> _canvas = new();
    private List<TMP_InputField> _inputFields = new();
    private RoomListController _roomListController;

    private void Awake()
    {
        StartRoomListController();
        SubscribeEvents();
        RegisterCanvas();
        SetMainMenuCanvasActive();
    }

    private void StartRoomListController()
    {
        _roomListController = new RoomListController(_roomListTransform, _roomContainerPrefab);
        _lobbyManager.OnRoomListUpdated += _roomListController.UpdateRoomList;
        foreach (var roomInfoContainer in _roomListController._roomList) roomInfoContainer.OnRoomInfoContainerClick += JoinOutlinedRoom;
    }

    private void RegisterCanvas()
    {
        _canvas.Add(_photonLoginScreenCanvas);
        _canvas.Add(_playFabLogInSreenCanvas);
        _canvas.Add(_createAccountMenuCanvas);
        _canvas.Add(_mainMenuCanvas);
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
    }

    private void UnsubscribeEvents()
    {
        foreach (var button in _buttons) button.onClick?.RemoveAllListeners();
        foreach (var inputField in _inputFields) inputField.onValueChanged?.RemoveAllListeners();
        _lobbyManager.OnRoomListUpdated -= _roomListController.UpdateRoomList;
        
    }

    private void SubcribeMainMenuEvents()
    {
        _createAccountMenuButton.onClick.AddListener(SetCreateAccountCanvasActive);
        _buttons.Add(_createAccountMenuButton);

        _logInPlayFabButton.onClick.AddListener(SetPlayFabLogInCanvasActive);
        _buttons.Add(_logInPlayFabButton);

        _exitButton.onClick.AddListener(Application.Quit);
        _buttons.Add(_exitButton);

        ActivateAllButtons();
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

        _playFabAccountManager.CreateAccountMessage += UpdateCreateAccountMessage;

        ActivateAllButtons();
    }

    private void SubscribePlayFabLoginMenuEvents()
    {
        _usernameLoginInput.onValueChanged.AddListener(UpdateLoginUsername);
        _inputFields.Add(_usernameLoginInput);

        _passwordLoginInput.onValueChanged.AddListener(UpdatePlayFabLoginPassword);
        _inputFields.Add(_passwordLoginInput);

        _logInAccountButton.onClick.AddListener(_playFabAccountManager.ConnectPlayFab);
        _logInAccountButton.onClick.AddListener(SetPhotonLogInCanvasActive);
        _logInAccountButton.onClick.AddListener(_lobbyManager.ConnectPhoton);
        _buttons.Add(_logInAccountButton);

        _backToMenuButton.onClick.AddListener(SetMainMenuCanvasActive);
        _buttons.Add(_backToMenuButton);

        _playFabAccountManager.LoginMessage += UpdateLoginMessage;

        ActivateAllButtons();
    }

    private void SubscribePhotonLoginMenuEvents()
    {
        _roomNameInput.onValueChanged.AddListener(UpdatePhotonRoomname);
        _inputFields.Add(_roomNameInput);

        _createRoomButton.onClick.AddListener(_lobbyManager.CreateRoom);
        _buttons.Add(_createRoomButton);

        _joinRoomButton.onClick.AddListener(JoinRoom);
        _buttons.Add(_joinRoomButton);

        _backToPlayFabButton.onClick.AddListener(SetMainMenuCanvasActive);
        _backToPlayFabButton.onClick.AddListener(_lobbyManager.DisconnectPhoton);
        _buttons.Add(_backToPlayFabButton);

        ActivateAllButtons();
    }

    private void JoinRoom()
    {
        if (_roomNameInput.text != string.Empty) _lobbyManager.JoinRoom(new EnterRoomParams()
        {
            RoomName = _roomNameInput.text,
        });
        else _lobbyManager.JoinRandomRoom();
    }

    private void JoinOutlinedRoom(RoomInfo enterRoomParams) => _lobbyManager.JoinRoom(new()
    {
        RoomName = enterRoomParams.Name,
        RoomOptions = new()
        {
            MaxPlayers = enterRoomParams.MaxPlayers,
            IsOpen = enterRoomParams.IsOpen,
            IsVisible = enterRoomParams.IsVisible,
        }
    });

    private void ActivateAllButtons()
    {
        foreach (var button in _buttons) if (!button.IsInteractable()) button.interactable = true;
    }

    private void UpdatePlayFabUsername(string username) => _playFabAccountManager.PlayFabUserName = username;
    private void UpdatePlayFabPassword(string password) => _playFabAccountManager.PlayFabPassWord = password;
    private void UpdatePlayFabEmail(string email) => _playFabAccountManager.EMail = email;
    private void UpdateLoginUsername(string username)
    {
        _playFabAccountManager.PlayFabLoginUsername = username;
        _lobbyManager.PhotonUsername = username;
    }
    private void UpdatePlayFabLoginPassword(string password) => _playFabAccountManager.PlayFabLoginPassword = password;

    private void UpdatePhotonRoomname(string roomname)
    {
        _lobbyManager.Roomname = roomname;
        _photonManager.Roomname = roomname;
    }

    private void SetCanvasActive(Canvas canvasToActivate)
    {
        foreach (var canvas in _canvas) if (canvas.enabled == true) canvas.enabled = false;
        canvasToActivate.enabled = true;
    }

    private void SetCreateAccountCanvasActive() => SetCanvasActive(_createAccountMenuCanvas);

    private async void SetPlayFabLogInCanvasActive()
    {
        await Task.Delay(1000);
        if (_createAccountMessage.color == Color.red) return;
        if (_createAccountMenuCanvas.enabled == true) await Task.Delay(3000);
        SetCanvasActive(_playFabLogInSreenCanvas);
    }

    private async void SetPhotonLogInCanvasActive()
    {
        await Task.Run(() => WaitPlayFabLogin());
        SetCanvasActive(_photonLoginScreenCanvas);
    }

    private void SetMainMenuCanvasActive() => SetCanvasActive(_mainMenuCanvas);

    private Task WaitPlayFabLogin()
    {
        while (!PlayFabClientAPI.IsClientLoggedIn()) Task.Delay(100);
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
}
