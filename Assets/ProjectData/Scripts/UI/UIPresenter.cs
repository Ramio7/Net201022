using Photon.Pun;
using Photon.Realtime;
using PlayFab;
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

    [Header("Photon login screen")]
    [SerializeField] private Canvas _photonLoginScreenCanvas;
    [SerializeField] private TMP_InputField _photonUsernameInput;
    [SerializeField] private TMP_InputField _roomnameInput;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;
    [SerializeField] private Button _leaveRoomButton;
    [SerializeField] private Button _joinRandomRoomButton;
    [SerializeField] private Button _backToPlayFabButton;
    [SerializeField] private Button _connectPhotonButton;
    [SerializeField] private TMP_Text _photonLoginMessage;
    [SerializeField] private ScrollRect _roomList;

    [Header("Utility")]
    [SerializeField] private PlayFabAccountManager _playFabAccountManager;
    [SerializeField] private PhotonManager _photonManager;
    [SerializeField] private PhotonLobbyManager _lobbyManager;
    [SerializeField] private RoomInfoButton _roomLineButtonPrefab;

    private List<Button> _buttons = new();
    private List<Canvas> _canvas = new();
    private List<TMP_InputField> _inputFields = new();
    private RoomInfo _selectedRoomInfo;

    private void Start()
    {
        SubscribeEvents();
        RegisterCanvas();
        SetMainMenuCanvasActive();
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
        _usernameLoginInput.onValueChanged.AddListener(UpdatePlayFabLoginUsername);
        _inputFields.Add(_usernameLoginInput);

        _passwordLoginInput.onValueChanged.AddListener(UpdatePlayFabLoginPassword);
        _inputFields.Add(_passwordLoginInput);

        _logInAccountButton.onClick.AddListener(_playFabAccountManager.ConnectPlayFab);
        _logInAccountButton.onClick.AddListener(SetPhotonLogInCanvasActive);
        _buttons.Add(_logInAccountButton);

        _backToMenuButton.onClick.AddListener(SetMainMenuCanvasActive);
        _buttons.Add(_backToMenuButton);

        _playFabAccountManager.LoginMessage += UpdateLoginMessage;

        ActivateAllButtons();
    }

    private void SubscribePhotonLoginMenuEvents()
    {
        _photonUsernameInput.onValueChanged.AddListener(UpdatePhotonUsername);
        _inputFields.Add(_photonUsernameInput);

        _roomnameInput.onValueChanged.AddListener(UpdatePhotonRoomname);
        _inputFields.Add(_roomnameInput);

        _createRoomButton.onClick.AddListener(_lobbyManager.CreateRoom);
        _buttons.Add(_createRoomButton);

        _joinRoomButton.onClick.AddListener(JoinOutlinedRoom);
        _buttons.Add(_joinRoomButton);

        _backToPlayFabButton.onClick.AddListener(SetMainMenuCanvasActive);
        _backToPlayFabButton.onClick.AddListener(_photonManager.DisconnectPhoton);
        _buttons.Add(_backToPlayFabButton);

        _connectPhotonButton.onClick.AddListener(_photonManager.ConnectPhoton);
        _connectPhotonButton.onClick.AddListener(ActivateRoomManagementButtons);
        _buttons.Add(_connectPhotonButton);

        _joinRandomRoomButton.onClick.AddListener(_photonManager.JoinRandomRoom);
        _buttons.Add(_joinRandomRoomButton);

        _leaveRoomButton.onClick.AddListener(_photonManager.LeaveCurrentRoom);
        _buttons.Add(_leaveRoomButton);

        ActivateAllButtons();
    }

    private void JoinOutlinedRoom() => _lobbyManager.JoinRoom(new()
    {
        RoomName = _selectedRoomInfo.Name,
        RoomOptions = new()
        {
            MaxPlayers = _selectedRoomInfo.MaxPlayers,
            IsOpen = _selectedRoomInfo.IsOpen,
            IsVisible = _selectedRoomInfo.IsVisible,
        }
    });

    private void ActivateAllButtons()
    {
        foreach (var button in _buttons) if (!button.IsInteractable()) button.interactable = true;
    }

    private void UpdatePlayFabUsername(string username) => _playFabAccountManager.PlayFabUserName = username;
    private void UpdatePlayFabPassword(string password) => _playFabAccountManager.PlayFabPassWord = password;
    private void UpdatePlayFabEmail(string email) => _playFabAccountManager.EMail = email;
    private void UpdatePlayFabLoginUsername(string username) => _playFabAccountManager.PlayFabLoginUsername = username;
    private void UpdatePlayFabLoginPassword(string password) => _playFabAccountManager.PlayFabLoginPassword = password;
    private void UpdatePhotonUsername(string username)
    {
        _lobbyManager.PhotonUsername = username;
        _photonManager.PhotonUsername = username;
    }

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
        await Task.Delay(1000);
        if (!PlayFabClientAPI.IsClientLoggedIn()) return;
        await Task.Delay(3000);
        SetCanvasActive(_photonLoginScreenCanvas);
    }

    private void SetMainMenuCanvasActive() => SetCanvasActive(_mainMenuCanvas);

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

    private void ActivateButton(Button button) => button.gameObject.SetActive(true);
    private void ActivateCreateRoomButton() => ActivateButton(_createRoomButton);
    private void ActivateJoinRoomButton() => ActivateButton(_joinRoomButton);
    private void ActivateJoinRandomRoomButton() => ActivateButton(_joinRandomRoomButton);
    private void ActivateLeaveRoomButton() => ActivateButton(_leaveRoomButton);
    private async void ActivateRoomManagementButtons()
    {
        await WaitPlayerToConnect();
        ActivateCreateRoomButton();
        ActivateJoinRoomButton();
        ActivateJoinRandomRoomButton();
        ActivateLeaveRoomButton();
    }

    private Task WaitPlayerToConnect()
    {
        while (!PhotonNetwork.IsConnected) Task.Delay(10);
        return Task.CompletedTask;
    }
}
