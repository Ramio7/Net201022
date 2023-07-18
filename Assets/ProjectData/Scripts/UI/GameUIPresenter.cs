using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIPresenter : MonoBehaviour
{
    [Header("UI fields")]
    [SerializeField] private Slider _playerHp;
    [SerializeField] private TMP_Text _ammoCounter;
    [SerializeField] private Canvas _exitGameCanvas;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    public float playerMaxHP;

    private void Start()
    {
        HideMouseCursor();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        ShowMouseCursor();
        UnsubscribeEvents();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) SetExitCanvasActiveSelf();
    }

    private void SubscribeEvents()
    {
        _yesButton.onClick.AddListener(LeaveRoom);
        _noButton.onClick.AddListener(SetExitCanvasUnactiveSelf);
    }

    private void UnsubscribeEvents()
    {
        _yesButton.onClick.RemoveAllListeners();
        _noButton.onClick.RemoveAllListeners();
    }

    private void HideMouseCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ShowMouseCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void SetPlayerHPSlider(float hp) => _playerHp.value = hp / playerMaxHP;
    public void SetBulletsCounter(int bulletCount, int maxBullets) => _ammoCounter.text = $"Ammo: {bulletCount} / {maxBullets}";

    private void SetExitCanvasActiveSelf()
    {
        _exitGameCanvas.enabled = true;
        ShowMouseCursor();
    }

    private void SetExitCanvasUnactiveSelf()
    {
        _exitGameCanvas.enabled = false;
        HideMouseCursor();
    }
    private void LeaveRoom() => PhotonNetwork.LeaveRoom();
}
