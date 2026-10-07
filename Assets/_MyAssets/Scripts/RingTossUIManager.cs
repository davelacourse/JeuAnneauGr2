using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Interface du stand : montre le pointage et transmet l'appui sur le bouton
/// de relance au gestionnaire de partie. Aucune règle de jeu ici.
/// </summary>
public class RingTossUIManager : MonoBehaviour
{
    [SerializeField] RingTossGameManager _gameManager;
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] XRBaseInteractable _resetButton;

    void OnEnable()
    {
        _gameManager.ScoreChanged += ShowScore;
        _resetButton.selectEntered.AddListener(OnResetPressed);
    }

    void OnDisable()
    {
        _gameManager.ScoreChanged -= ShowScore;
        _resetButton.selectEntered.RemoveListener(OnResetPressed);
    }

    // Le panneau affiche le pointage dès le départ, sans attendre un premier point.
    void Start()
    {
        ShowScore(_gameManager.Score);
    }

    void ShowScore(int score)
    {
        _scoreText.text = $"Pointage : {score}";
    }

    void OnResetPressed(SelectEnterEventArgs args)
    {
        _gameManager.ResetGame();
    }
}
