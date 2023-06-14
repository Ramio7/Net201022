using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPresenter : MonoBehaviour
{
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private TMP_Text _playFabMessage;
    [SerializeField] private Button _connectPlayFabButton;
    [SerializeField] private Button _disconnectPlayFabButton;
    [SerializeField] private Button _connectPhotonButton;
    [SerializeField] private Button _disconnectPhotonButton;
    [SerializeField] private Authorization _authorization;

    private void Start()
    {
        Authorization.PlayFabMessage += UpdatePlayFabMessage;
        SubscribeEvents();
    }

    private void OnDisable()
    {
        _connectPlayFabButton.onClick.RemoveAllListeners();
        _disconnectPlayFabButton.onClick.RemoveAllListeners();
        _connectPhotonButton.onClick.RemoveAllListeners();
        _disconnectPhotonButton.onClick.RemoveAllListeners();
        _usernameInput.onValueChanged.RemoveAllListeners();
    }

    private void SubscribeEvents()
    {
        _usernameInput.onValueChanged.AddListener(UpdateUsername);

        _connectPlayFabButton.onClick.AddListener(_authorization.ConnectPlayFab);
        _connectPlayFabButton.onClick.AddListener(SwitchConnectPlayFabButton);
        _connectPlayFabButton.onClick.AddListener(SetConnectPhotonButtonActive);

        _disconnectPlayFabButton.onClick.AddListener(_authorization.DisconnectPlayFab);
        _disconnectPlayFabButton.onClick.AddListener(SwitchDisconnectPlayFabButton);

        _connectPhotonButton.onClick.AddListener(_authorization.ConnectPhoton);
        _connectPhotonButton.onClick.AddListener(SwitchConnectPhotonButton);

        _disconnectPhotonButton.onClick.AddListener(_authorization.DisconnectPhoton);
        _disconnectPhotonButton.onClick.AddListener(SwitchDisconnectPhotonButton);
    }

    private void UpdateUsername(string username) => _authorization.UserName = username;

    private void UpdatePlayFabMessage(string message, Color messageColor)
    {
        _playFabMessage.text = message;
        _playFabMessage.color = messageColor;
    }

    private void SetConnectPhotonButtonActive() => _connectPhotonButton.gameObject.SetActive(true);

    private void SwitchButtons(Button activeButton, Button unactiveButton)
    {
        activeButton.gameObject.SetActive(false);
        unactiveButton.gameObject.SetActive(true);
    }

    private void SwitchConnectPlayFabButton() => SwitchButtons(_connectPlayFabButton, _disconnectPlayFabButton);
    private void SwitchDisconnectPlayFabButton() => SwitchButtons(_disconnectPlayFabButton, _connectPlayFabButton);
    private void SwitchConnectPhotonButton() => SwitchButtons(_connectPhotonButton, _disconnectPhotonButton);
    private void SwitchDisconnectPhotonButton() => SwitchButtons(_disconnectPhotonButton, _connectPhotonButton);
}
