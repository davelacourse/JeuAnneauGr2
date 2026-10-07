using System;
using UnityEngine;

/// <summary>
/// Gestionnaire de partie du stand d'anneaux : il applique les règles, tient le
/// pointage et annonce chaque changement. Il ne sait rien de l'affichage.
/// Ce n'est pas un singleton : chaque stand a le sien.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class RingTossGameManager : MonoBehaviour
{
    [SerializeField] int _pointsPerRing = 10;

    Ring[] _rings;
    AudioSource _successAudio;
    int _score;

    public int Score => _score;

    // Annonce le nouveau pointage à qui veut l'entendre.
    public event Action<int> ScoreChanged;

    void Awake()
    {
        // Les anneaux sont des enfants du stand : pas de liste à remplir à la main.
        _rings = GetComponentsInChildren<Ring>();
        _successAudio = GetComponent<AudioSource>();
    }

    public void AddPoints(Ring ring)
    {
        _score += _pointsPerRing;
        _successAudio.Play();
        ring.PulseThrower(0.7f, 0.3f);
        ScoreChanged?.Invoke(_score);
    }

    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        foreach (Ring ring in _rings)
            ring.ResetToStart();

        _score = 0;
        ScoreChanged?.Invoke(_score);
    }
}
