using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Comportement d'un anneau : il retient sa place de départ, se souvient de la
/// main qui l'a lancé, ne se lance qu'une fois et sonne quand il frappe quelque chose.
/// Il ne compte aucun point : c'est le rôle du RingTossGameManager.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody), typeof(AudioSource))]
public class Ring : MonoBehaviour
{
    [Tooltip("Vitesse d'impact minimale pour jouer le son, en m/s.")]
    [SerializeField] float _minImpactSpeed = 0.5f;

    [Tooltip("Vitesse d'impact qui donne le volume maximal, en m/s.")]
    [SerializeField] float _maxImpactSpeed = 5f;

    XRGrabInteractable _grab;
    Rigidbody _rigidbody;
    AudioSource _audio;
    HapticImpulsePlayer _thrower;
    InteractionLayerMask _grabLayers;
    Vector3 _startPosition;
    Quaternion _startRotation;

    public bool IsHeld => _grab.isSelected;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rigidbody = GetComponent<Rigidbody>();
        _audio = GetComponent<AudioSource>();

        // Un anneau enfoncé dans un collider en ressort doucement, sans être éjecté.
        _rigidbody.maxDepenetrationVelocity = 1f;

        // La place de départ est notée avant que quiconque ait touché l'anneau.
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        // Le masque d'origine, pour rendre l'anneau saisissable de nouveau au reset.
        _grabLayers = _grab.interactionLayers;
    }

    void OnEnable()
    {
        _grab.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        _grab.selectExited.RemoveListener(OnReleased);
    }

    // Au moment du lâcher, on retient le Haptic Impulse Player de la main qui lance,
    // comme dans ObjectHaptics au cours 6.
    void OnReleased(SelectExitEventArgs args)
    {
        Transform interactor = args.interactorObject.transform;
        _thrower = interactor.GetComponentInParent<HapticImpulsePlayer>();

        // Changer de main relâche l'anneau un court instant avant que l'autre main le
        // prenne : on attend une image avant de décider qu'il a vraiment été lâché.
        StartCoroutine(LockIfReleased());
    }

    IEnumerator LockIfReleased()
    {
        yield return null;

        // Toujours dans aucune main : l'anneau a été lancé ou lâché, il est verrouillé.
        if (!IsHeld)
            Lock();
    }

    /// <summary>Rend l'anneau impossible à saisir jusqu'au prochain reset.</summary>
    public void Lock()
    {
        // On ne désactive pas le XR Grab Interactable : il applique la vitesse du
        // lancer après le lâcher, et désactivé, il ne le ferait plus.
        // Sans couche commune, aucune main ne peut le saisir ; s'il est encore tenu,
        // l'Interaction Manager le lâche normalement.
        _grab.interactionLayers = 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        float speed = collision.relativeVelocity.magnitude;
        if (speed < _minImpactSpeed)
            return;

        // Plus le choc est fort, plus le son est fort ; une légère variation de
        // hauteur évite que tous les rebonds sonnent pareil.
        _audio.volume = Mathf.InverseLerp(_minImpactSpeed, _maxImpactSpeed, speed);
        _audio.pitch = Random.Range(0.9f, 1.1f);
        _audio.Play();
    }

    /// <summary>Fait vibrer la main qui a lancé l'anneau.</summary>
    public void PulseThrower(float amplitude, float duration)
    {
        if (_thrower != null)
            _thrower.SendHapticImpulse(amplitude, duration);
    }

    /// <summary>Replace l'anneau à son point de départ, immobile.</summary>
    public void ResetToStart()
    {
        // Un anneau tenu reste dans la main : on ne l'arrache pas au joueur.
        if (IsHeld)
            return;

        StopAllCoroutines();
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        // Avec l'interpolation, le Rigidbody contrôle la position : on le déplace lui aussi.
        transform.SetPositionAndRotation(_startPosition, _startRotation);
        _rigidbody.position = _startPosition;
        _rigidbody.rotation = _startRotation;
        _thrower = null;
        _grab.interactionLayers = _grabLayers;
    }
}
