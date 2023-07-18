using UnityEngine;

public interface IDealDamage
{
    public float Damage { get; }
    public GameObject GameObject { get; }

    public bool IsCharged { get; }
}
