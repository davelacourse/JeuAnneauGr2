using UnityEngine;

/// <summary>
/// Zone derrière le comptoir. Tout anneau qui y entre est verrouillé : on ne peut
/// pas passer le bras par-dessus le comptoir pour déposer un anneau sur une bouteille.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class CounterZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Le collider qui entre est un maillon de l'anneau : on remonte jusqu'à Ring.
        Ring ring = other.GetComponentInParent<Ring>();
        if (ring != null)
            ring.Lock();
    }
}
