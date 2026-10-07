using System.Collections;
using UnityEngine;

/// <summary>
/// Zone de pointage au centre de l'anneau. Un point n'est accordé que si une
/// bouteille est encore dans l'anneau après un délai : un anneau qui ne fait
/// que rebondir sur une bouteille ne compte pas.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class RingScoreZone : MonoBehaviour
{
    [Tooltip("Temps pendant lequel une bouteille doit rester dans l'anneau, en secondes.")]
    [SerializeField] float _confirmDelay = 3f;

    Ring _ring;
    RingTossGameManager _gameManager;
    int _bottlesInside;
    bool _scored;

    void Awake()
    {
        // L'anneau et le gestionnaire du stand sont des parents de la zone. On les cherche ici, une
        // fois pour toutes : pendant une saisie, XRI détache l'anneau du stand.
        _ring = GetComponentInParent<Ring>();
        _gameManager = GetComponentInParent<RingTossGameManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bouteille"))
            return;

        _bottlesInside++;

        // Un anneau posé ne compte qu'une fois, même s'il bouge un peu sur sa bouteille.
        if (_scored)
            return;

        // Un seul délai à la fois : chaque nouvelle entrée repart le compte.
        StopAllCoroutines();
        StartCoroutine(ConfirmScore());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Bouteille"))
            return;

        _bottlesInside--;

        // L'anneau a quitté toutes les bouteilles : il pourra marquer de nouveau.
        if (_bottlesInside <= 0)
        {
            _bottlesInside = 0;
            _scored = false;
        }
    }

    IEnumerator ConfirmScore()
    {
        yield return new WaitForSeconds(_confirmDelay);

        // Toujours une bouteille dans l'anneau, et l'anneau n'est pas dans une main.
        if (_bottlesInside > 0 && !_ring.IsHeld)
        {
            _scored = true;
            _gameManager.AddPoints(_ring);
        }
    }
}
