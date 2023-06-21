using System.Collections.Generic;
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

    [Header("PlayFab log in screen")]
    [SerializeField] private Canvas _playFabLogInSreenCanvas;
    [SerializeField] private TMP_InputField _usernameLoginInput;
    [SerializeField] private TMP_InputField _passwordLoginInput;
    [SerializeField] private Button _logInAccountButton;
    [SerializeField] private Button _backToMenuButton;

    [Header("Photon login screen")]
    [SerializeField] private Canvas _photonLoginScreenCanvas;
    [SerializeField] private TMP_InputField _photonUsernameInput;
    [SerializeField] private TMP_InputField _roomnameInput;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;
    [SerializeField] private Button _backToPlayFabButton;
    [SerializeField] private Button _connectPhotonButton;

    [Header("Utility")]
    [SerializeField] private PlayFabAccountManager _playFabAccountManager;
    [SerializeField] private PhotonManager _photonManager;

    private List<Button> _buttons = new();
    private List<Canvas> _canvas = new();
    private List<TMP_InputField> _inputFields = new();

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

        _logInPlayFabButton.onClick.AddListener(SetPhotonLogInCanvasActive);
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

        _logInAccountButton.onClick.AddListener(SetPhotonLogInCanvasActive);
        _buttons.Add(_logInAccountButton);

        _backToMenuButton.onClick.AddListener(SetMainMenuCanvasActive);
        _buttons.Add(_backToMenuButton);
    }

    private void SubscribePhotonLoginMenuEvents()
    {
        _photonUsernameInput.onValueChanged.AddListener(UpdatePhotonUsername);
        _inputFields.Add(_photonUsernameInput);

        _roomnameInput.onValueChanged.AddListener(UpdatePhotonRoomname);
        _inputFields.Add(_roomnameInput);

        _createRoomButton.onClick.AddListener(_photonManager.CreateRoom);
        _buttons.Add(_createRoomButton);

        _joinRoomButton.onClick.AddListener(_photonManager.JoinRoom);
        _buttons.Add(_joinRoomButton);

        _backToPlayFabButton.onClick.AddListener(SetPlayFabLogInCanvasActive);
        _backToPlayFabButton.onClick.AddListener(_photonManager.DisconnectPhoton);
        _buttons.Add(_backToPlayFabButton);

        _connectPhotonButton.onClick.AddListener(_photonManager.ConnectPhoton);
        _buttons.Add(_connectPhotonButton);
    }

    private void UpdatePlayFabUsername(string username) => _playFabAccountManager.PlayFabUserName = username;
    private void UpdatePlayFabPassword(string password) => _playFabAccountManager.PlayFabPassWord = password;
    private void UpdatePlayFabEmail(string email) => _playFabAccountManager.EMail = email;
    private void UpdatePlayFabLoginUsername(string username) => _playFabAccountManager.PlayFabLoginUsername = username;
    private void UpdatePlayFabLoginPassword(string password) => _playFabAccountManager.PlayFabLoginPassword = password;
    private void UpdatePhotonUsername(string username) => _photonManager.PhotonUsername = username;
    private void UpdatePhotonRoomname(string roomname) => _photonManager.Roomname = roomname;

    private void SetCanvasActive(Canvas canvasToActivate)
    {
        foreach (var canvas in _canvas) canvas.enabled = false;
        canvasToActivate.enabled = true;
    }

    private void SetCreateAccountCanvasActive() => SetCanvasActive(_createAccountMenuCanvas);
    private void SetPlayFabLogInCanvasActive() => SetCanvasActive(_photonLoginScreenCanvas);
    private void SetPhotonLogInCanvasActive() => SetCanvasActive(_photonLoginScreenCanvas);
    private void SetMainMenuCanvasActive() => SetCanvasActive(_mainMenuCanvas);
}
