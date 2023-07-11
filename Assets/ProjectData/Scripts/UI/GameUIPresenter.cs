using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIPresenter : MonoBehaviour
{
    [SerializeField] private GameObject _playerStatisticsPrefab;

    [Header("UI fields")]
    [SerializeField] private Button _exitGameButton;
    [SerializeField] private Slider _playerHp;
    [SerializeField] private TMP_Text _ammoCounter;
    [SerializeField] private TMP_Text _gameEventsBar;
    [SerializeField] private Canvas _gameStatisticsCanvas;


}
