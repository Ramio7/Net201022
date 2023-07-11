using Photon.Pun;
using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CameraController), typeof(CharacterController))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private BulletDiploma _bulletPrefab;
    [SerializeField] private Transform _bulletSpawnPoint;


    public ReactiveProperty<bool> IsFiring = new();
    public ReactiveProperty<float> Health = new();
    public ReactiveProperty<int> BulletsCount = new();

    public const float MaxHealth = 100f;
    public const int MaxBullets = 30;

    public static GameObject LocalPlayerInstance;

    public event Action<float> OnPlayerHpValueChanged;

    public void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        IsFiring.OnValueChanged += Fire;
        CameraController _cameraController = gameObject.GetComponent<CameraController>();

        if (_cameraController != null)
        {
            if (photonView.IsMine)
            {
                _cameraController.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("Missing CameraController Component on player Prefab.", this);
        }
    }

    private void OnDestroy()
    {
        IsFiring.OnValueChanged -= Fire;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0)) IsFiring.SetValue(true);
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (other.TryGetComponent<BulletDiploma>(out var bullet))
        {
            var health = Health.GetValue() - bullet.bulletDamage;
            Health.SetValue(health);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(IsFiring.GetValue());
            stream.SendNext(Health.GetValue());
        }
        else
        {
            IsFiring.SetValue((bool)stream.ReceiveNext());
            var playerHealth = (float)stream.ReceiveNext();
            Health.SetValue(playerHealth);
        }
    }

    private async void Fire(bool isFiring)
    {
        if (!isFiring) return;

        while (isFiring)
        {
            if (BulletsCount.GetValue() == 0)
            {
                await Task.Run(() => WaitReload());
                BulletsCount.SetValue(MaxBullets);
            }
            PhotonNetwork.Instantiate(_bulletPrefab.name, _bulletSpawnPoint.position, Quaternion.identity);
            var bulletsLeft = BulletsCount.GetValue() - 1;
            BulletsCount.SetValue(bulletsLeft);
            await Task.Run(() => FireRateWaiting());
            IsFiring.SetValue(false);
        }
    }

    private Task FireRateWaiting() => Task.Delay(200);

    private Task WaitReload() => Task.Delay(1000);
}
