using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicPrinterController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform cardSensor;
    [SerializeField] private Transform printerHead;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private GameObject printedCardVisual;
    [SerializeField] private Sprite printedCardSprite;
    [SerializeField] private ParticleSystem confettiEffect;

    [Header("Detection")]
    [SerializeField] private Vector3 sensorHalfExtents = new Vector3(0.35f, 0.12f, 0.35f);
    [SerializeField] private string cardNameContains = "Card";
    [SerializeField] private LayerMask cardLayerMask = ~0;

    [Header("Printing")]
    [SerializeField] private Vector3 headDownOffset = new Vector3(0f, -0.22f, 0f);
    [SerializeField] private float headMoveDuration = 0.6f;
    [SerializeField] private float printHoldDuration = 1.2f;
    [SerializeField] private string waitingMessage = "Lege die Geburtstagskarte ein.";
    [SerializeField] private string readyMessage = "Karte erkannt.\nJetzt fehlt noch der Name des Geburtstagskindes.";
    [SerializeField] private string missingNameMessage = "Gib den entschlüsselten Namen ein.";
    [SerializeField] private string wrongNameMessage = "Das ist noch nicht der richtige Name.\nPrüft die Emoji-Cäsar-Scheibe noch einmal.";
    [SerializeField] private string printingMessage = "Name erkannt.\nDie Geburtstagskarte wird gedruckt.";
    [SerializeField] private string doneMessage = "Die Geburtstagskarte ist fertig!\nDie Party kann beginnen.";
    [SerializeField] private bool playConfettiOnPrint = false;
    [SerializeField] private string confettiEffectName = "ConfettiParticleEffect";

    [Header("Name Puzzle")]
    [SerializeField] private bool requireCorrectName = true;
    [SerializeField] private string requiredName = "amelie";
    [SerializeField] private string keyboardRootName = "Keyboard";

    [Header("Exit Door")]
    [SerializeField] private Transform exitDoor;
    [SerializeField] private string exitDoorName = "ExitDoor";
    [SerializeField] private string exitDoorWingName = "doorWing";
    [SerializeField] private Vector3 exitDoorOpenEulerOffset = new Vector3(0f, -90f, 0f);
    [SerializeField] private float exitDoorOpenDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource printerAudioSource;
    [SerializeField] private AudioSource backgroundMusicSource;
    // Credit: printerPrintClip - recommended source Kenney "Impact Sounds", CC0, https://kenney.nl/assets/impact-sounds
    [SerializeField] private AudioClip printerPrintClip;
    // Credit: confettiClip - recommended source Kenney "Impact Sounds", CC0, https://kenney.nl/assets/impact-sounds
    [SerializeField] private AudioClip confettiClip;
    // Credit: doorOpenClip - recommended source Kenney "Impact Sounds", CC0, https://kenney.nl/assets/impact-sounds
    [SerializeField] private AudioClip doorOpenClip;
    // Credit: backgroundMusicClip - recommended source Kenney "Music Jingles", CC0, https://kenney.nl/assets/music-jingles
    [SerializeField] private AudioClip backgroundMusicClip;
    [SerializeField, Range(0f, 1f)] private float printerPrintVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float confettiVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float doorOpenVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float backgroundMusicVolume = 0.18f;

    private Vector3 headStartLocalPosition;
    private bool cardDetected;
    private bool isPrinting;
    private bool printed;
    private bool exitDoorOpened;
    private GameObject detectedCardVisual;

    private void Awake()
    {
        AutoWireReferences();
        ResolveAudioSources();

        if (printerHead != null)
        {
            headStartLocalPosition = printerHead.localPosition;
        }

        if (printedCardVisual != null)
        {
            ApplyPrintedCardSprite();
        }

        if (confettiEffect != null)
        {
            confettiEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();

        if (playConfettiOnPrint)
        {
            ResolveConfettiEffect();
        }

        UpdateFeedback();
    }

    private void Update()
    {
        bool detectedNow = IsCardOnSensor();
        if (detectedNow != cardDetected)
        {
            cardDetected = detectedNow;
            UpdateFeedback();
        }

    }

    public void RegisterCard()
    {
        cardDetected = true;
        UpdateFeedback();
    }

    public void ClearCard()
    {
        if (isPrinting || printed)
        {
            return;
        }

        cardDetected = false;
        UpdateFeedback();
    }

    public void PressGreenButton()
    {
        if (!cardDetected || isPrinting || printed)
        {
            return;
        }

        if (requireCorrectName && IsEnteredNameMissing())
        {
            SetFeedback(missingNameMessage);
            return;
        }

        if (requireCorrectName && !IsEnteredNameCorrect())
        {
            SetFeedback(wrongNameMessage);
            return;
        }

        StartCoroutine(PrintRoutine());
    }

    private bool IsEnteredNameCorrect()
    {
        ResolveNameInputField();

        if (nameInputField == null)
        {
            Debug.LogWarning("MagicPrinter could not find a TMP_InputField for the birthday name check.", this);
            return false;
        }

        string enteredName = NormalizeName(nameInputField.text);
        string expectedName = NormalizeName(requiredName);
        return string.Equals(enteredName, expectedName, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsEnteredNameMissing()
    {
        ResolveNameInputField();

        if (nameInputField == null)
        {
            Debug.LogWarning("MagicPrinter could not find a TMP_InputField for the birthday name check.", this);
            return true;
        }

        return string.IsNullOrEmpty(NormalizeName(nameInputField.text));
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim().Replace(" ", string.Empty);
    }

    private IEnumerator PrintRoutine()
    {
        isPrinting = true;
        SetFeedback(printingMessage);
        PlayOneShot(printerPrintClip, printerPrintVolume);

        Vector3 downPosition = headStartLocalPosition + headDownOffset;
        yield return MoveHead(headStartLocalPosition, downPosition, headMoveDuration);
        yield return new WaitForSeconds(printHoldDuration);
        yield return MoveHead(downPosition, headStartLocalPosition, headMoveDuration);

        printed = true;
        isPrinting = false;

        if (printedCardVisual != null)
        {
            ApplyPrintedCardSprite(printedCardVisual);
        }
        else
        {
            ApplyPrintedCardSprite(detectedCardVisual);
        }

        if (playConfettiOnPrint)
        {
            PlayConfettiOnce();
        }

        OpenExitDoor();
        SetFeedback(doneMessage);
    }

    private void OpenExitDoor()
    {
        if (exitDoorOpened)
        {
            return;
        }

        ResolveExitDoor();

        if (exitDoor == null)
        {
            Debug.LogWarning($"MagicPrinter could not find exit door '{exitDoorName}'.", this);
            return;
        }

        exitDoorOpened = true;
        PlayOneShot(doorOpenClip, doorOpenVolume);
        StartCoroutine(OpenExitDoorRoutine());
    }

    private IEnumerator OpenExitDoorRoutine()
    {
        Quaternion closedRotation = exitDoor.localRotation;
        Quaternion openRotation = closedRotation * Quaternion.Euler(exitDoorOpenEulerOffset);

        if (exitDoorOpenDuration <= 0f)
        {
            exitDoor.localRotation = openRotation;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < exitDoorOpenDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / exitDoorOpenDuration);
            exitDoor.localRotation = Quaternion.Slerp(closedRotation, openRotation, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        exitDoor.localRotation = openRotation;
    }

    private void PlayConfettiOnce()
    {
        ResolveConfettiEffect();

        if (confettiEffect == null)
        {
            Debug.LogWarning($"Could not find ParticleSystem '{confettiEffectName}' for MagicPrinter confetti.", this);
            return;
        }

        confettiEffect.gameObject.SetActive(true);
        confettiEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        confettiEffect.Clear(true);
        confettiEffect.Play(true);
        PlayOneShot(confettiClip, confettiVolume);
    }

    private IEnumerator MoveHead(Vector3 from, Vector3 to, float duration)
    {
        if (printerHead == null || duration <= 0f)
        {
            if (printerHead != null)
            {
                printerHead.localPosition = to;
            }

            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            printerHead.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        printerHead.localPosition = to;
    }

    private bool IsCardOnSensor()
    {
        if (cardSensor == null || isPrinting || printed)
        {
            return cardDetected;
        }

        Collider[] hits = Physics.OverlapBox(
            cardSensor.position,
            sensorHalfExtents,
            cardSensor.rotation,
            cardLayerMask,
            QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            if (hit == null || hit.transform == cardSensor || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            if (string.IsNullOrEmpty(cardNameContains) || hit.name.Contains(cardNameContains) || hit.transform.root.name.Contains(cardNameContains))
            {
                detectedCardVisual = FindDetectedCardVisual(hit.transform);
                return true;
            }
        }

        detectedCardVisual = null;
        return false;
    }

    private GameObject FindDetectedCardVisual(Transform hitTransform)
    {
        if (hitTransform == null)
        {
            return null;
        }

        Transform current = hitTransform;
        while (current != null)
        {
            if (!string.IsNullOrEmpty(cardNameContains) && current.name.Contains(cardNameContains))
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return hitTransform.root != null ? hitTransform.root.gameObject : hitTransform.gameObject;
    }

    private void UpdateFeedback()
    {
        if (printed)
        {
            SetFeedback(doneMessage);
        }
        else if (isPrinting)
        {
            SetFeedback(printingMessage);
        }
        else
        {
            SetFeedback(cardDetected ? readyMessage : waitingMessage);
        }
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    private void AutoWireReferences()
    {
        if (cardSensor == null)
        {
            cardSensor = FindChildByName(transform, "Cylinder");
        }

        if (printerHead == null)
        {
            printerHead = FindChildByName(transform, "Head");
        }

        if (feedbackText == null)
        {
            Transform feedbackTransform = FindChildByName(transform, "FeedbackTmp");
            if (feedbackTransform != null)
            {
                feedbackText = feedbackTransform.GetComponent<TMP_Text>();
            }
        }

        ResolveNameInputField();
        ResolveExitDoor();
        ResolveAudioSources();
        if (playConfettiOnPrint)
        {
            ResolveConfettiEffect();
        }

    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private void ResolveConfettiEffect()
    {
        if (confettiEffect == null)
        {
            confettiEffect = FindParticleSystemByName(confettiEffectName);
        }

        ConfigureConfettiEffect();
    }

    private void ResolveAudioSources()
    {
        if (printerAudioSource == null)
        {
            printerAudioSource = GetComponent<AudioSource>();
            if (printerAudioSource == null)
            {
                printerAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        printerAudioSource.playOnAwake = false;
        printerAudioSource.spatialBlend = 1f;

        if (backgroundMusicSource == null && backgroundMusicClip != null)
        {
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        }

        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.playOnAwake = false;
        }
    }

    private void PlayBackgroundMusic()
    {
        if (backgroundMusicClip == null || backgroundMusicSource == null)
        {
            return;
        }

        backgroundMusicSource.clip = backgroundMusicClip;
        backgroundMusicSource.loop = true;
        backgroundMusicSource.volume = backgroundMusicVolume;
        backgroundMusicSource.spatialBlend = 0f;

        if (!backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Play();
        }
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip == null || printerAudioSource == null)
        {
            return;
        }

        printerAudioSource.PlayOneShot(clip, volume);
    }

    private void ConfigureConfettiEffect()
    {
        if (confettiEffect == null)
        {
            return;
        }

        ParticleSystem.MainModule main = confettiEffect.main;
        main.duration = 1.4f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 3.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.075f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = 0.45f;
        main.maxParticles = 90;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = confettiEffect.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)55, (short)75),
        });

        ParticleSystem.ShapeModule shape = confettiEffect.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = 0.18f;
        shape.length = 0.08f;
        shape.randomDirectionAmount = 0.25f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = confettiEffect.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fade = new Gradient();
        fade.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f),
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.72f),
                new GradientAlphaKey(0f, 1f),
            });
        colorOverLifetime.color = fade;

        ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = confettiEffect.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-4f, 4f);

        ParticleSystemRenderer confettiRenderer = confettiEffect.GetComponent<ParticleSystemRenderer>();
        if (confettiRenderer != null)
        {
            confettiRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            confettiRenderer.maxParticleSize = 0.12f;
            confettiRenderer.sortingOrder = 5;
        }
    }

    private void ApplyPrintedCardSprite()
    {
        ApplyPrintedCardSprite(printedCardVisual);
    }

    private void ApplyPrintedCardSprite(GameObject target)
    {
        if (printedCardSprite == null || target == null)
        {
            return;
        }

        if (target.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            spriteRenderer.sprite = printedCardSprite;
        }

        foreach (SpriteRenderer childSpriteRenderer in target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            childSpriteRenderer.sprite = printedCardSprite;
        }

        foreach (Image image in target.GetComponentsInChildren<Image>(true))
        {
            image.sprite = printedCardSprite;
        }
    }

    private void ResolveNameInputField()
    {
        if (nameInputField != null)
        {
            return;
        }

        TMP_InputField[] inputFields = FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_InputField inputField in inputFields)
        {
            if (inputField != null && IsUnderNamedParent(inputField.transform, keyboardRootName))
            {
                nameInputField = inputField;
                return;
            }
        }

        if (inputFields.Length > 0)
        {
            nameInputField = inputFields[0];
        }
    }

    private void ResolveExitDoor()
    {
        if (exitDoor != null)
        {
            return;
        }

        Transform exitDoorRoot = FindSceneTransformByName(exitDoorName);
        if (exitDoorRoot != null)
        {
            Transform doorWing = FindChildByName(exitDoorRoot, exitDoorWingName);
            exitDoor = doorWing != null ? doorWing : exitDoorRoot;
            return;
        }

        exitDoor = FindSceneTransformByName("Door_HouseA_Blue");
    }

    private static bool IsUnderNamedParent(Transform candidate, string parentName)
    {
        if (candidate == null || string.IsNullOrEmpty(parentName))
        {
            return false;
        }

        Transform current = candidate;
        while (current != null)
        {
            if (current.name == parentName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static Transform FindSceneTransformByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform sceneTransform in transforms)
        {
            if (sceneTransform.name == objectName)
            {
                return sceneTransform;
            }
        }

        return null;
    }

    private static ParticleSystem FindParticleSystemByName(string effectName)
    {
        if (string.IsNullOrEmpty(effectName))
        {
            return null;
        }

        ParticleSystem[] particleSystems = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem.name == effectName || particleSystem.transform.root.name == effectName)
            {
                return particleSystem;
            }
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Transform sensor = cardSensor != null ? cardSensor : FindChildByName(transform, "Cylinder");
        if (sensor == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(sensor.position, sensor.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, sensorHalfExtents * 2f);
        Gizmos.matrix = oldMatrix;
    }
}
