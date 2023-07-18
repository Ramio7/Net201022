using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(AudioSource))]
public class BulletController : MonoBehaviourPunCallbacks, IDealDamage
{
    [SerializeField] private float _bulletStartingForce = 100;
    [SerializeField] private float _bulletDamage = 10.0f;

    private float _selfDestructionTime;
    private float _bulletLifeTime = 2.0f;

    public float Damage { get => _bulletDamage; }
    public float BulletStartingForce { get => _bulletStartingForce; private set => _bulletStartingForce = value; }

    public GameObject GameObject { get => gameObject; }

    public bool IsCharged { get; private set; }

    public override void OnEnable()
    {
        IsCharged = true;
        _selfDestructionTime = Time.time + _bulletLifeTime;
        var bulletVector = gameObject.transform.position - PlayerController.Instance.CameraTransform.position;
        GetComponent<Rigidbody>().AddForce(bulletVector * _bulletStartingForce, ForceMode.VelocityChange);
    }

    private void Update()
    {
        if (_selfDestructionTime <= Time.time)
        {
            if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player)) player.TakeDamage(Damage);
        Debug.Log($"Bullet faced {collision.gameObject.name}");
        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }
}
