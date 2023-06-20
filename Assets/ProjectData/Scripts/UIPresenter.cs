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
    [SerializeField] private Button _createLobbyButton;
    [SerializeField] private Button _joinLobbyButton;
    [SerializeField] private Button _backToPlayFabButton;
    [SerializeField] private TMP_Text _lobbyListText;

    [Header("Utility")]
    [SerializeField] private Authorization _authorization;
    private List<Button> _buttons = new();
    private List<Canvas> _canvas = new();
    private List<TMP_InputField> _inputFields = new();

    private void Start()
    {
        SubscribeEvents();
        
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        SubcribeMainMenuEvents();
        SubscribeCreateAccountMenuEvents();

        _createLobbyButton.onClick.AddListener(_authorization.ConnectPlayFab);

        _joinLobbyButton.onClick.AddListener(_authorization.DisconnectPlayFab);
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
        _usernameInput.onValueChanged.AddListener(UpdateUsername);
        _inputFields.Add(_usernameInput);

        _passwordInput.onValueChanged.AddListener(UpdatePassword);
        _inputFields.Add(_passwordInput);

        _emailInput.onValueChanged.AddListener(UpdateEmail);
        _inputFields.Add(_emailInput);

        _createNewAccountButton.onClick.AddListener(_authorization.ConnectPlayFab);
        _createNewAccountButton.onClick.AddListener(SetPlayFabLogInCanvasActive);
        _buttons.Add(_createNewAccountButton);

        _backButton.onClick.AddListener(SetMainMenuCanvasActive);
        _buttons.Add(_backButton);
    }

    private void UpdateUsername(string username) => _authorization.UserName = username;
    private void UpdatePassword(string password) => _authorization.PassWord = password;
    private void UpdateEmail(string email) => _authorization.EMail = email;

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
