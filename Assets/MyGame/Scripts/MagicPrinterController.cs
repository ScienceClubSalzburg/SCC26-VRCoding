using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MagicPrinterController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform cardSensor;
    [SerializeField] private Transform printerHead;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private GameObject printedCardVisual;
    [SerializeField] private ParticleSystem confettiEffect;

    [Header("Detection")]
    [SerializeField] private Vector3 sensorHalfExtents = new Vector3(0.35f, 0.12f, 0.35f);
    [SerializeField] private string cardNameContains = "Card";
    [SerializeField] private LayerMask cardLayerMask = ~0;

    [Header("Printing")]
    [SerializeField] private Vector3 headDownOffset = new Vector3(0f, -0.22f, 0f);
    [SerializeField] private float headMoveDuration = 0.6f;
    [SerializeField] private float printHoldDuration = 1.2f;
    [SerializeField] private string waitingMessage = "Finde die Karte!";
    [SerializeField] private string readyMessage = "Drücke den grünen Knopf um zu starten.";
    [SerializeField] private string printingMessage = "Karte wird gedruckt...";
    [SerializeField] private string doneMessage = "Karte ist gedruckt!";
    [SerializeField] private string wrongNameMessage = "Falscher Name!";
    [SerializeField] private string confettiEffectName = "ConfettiParticleEffect";

    [Header("Name Puzzle")]
    [SerializeField] private bool requireCorrectName = true;
    [SerializeField] private string requiredName = "amelie";
    [SerializeField] private string keyboardRootName = "Keyboard";

    private Vector3 headStartLocalPosition;
    private bool cardDetected;
    private bool isPrinting;
    private bool printed;

    private void Awake()
    {
        AutoWireReferences();

        if (printerHead != null)
        {
            headStartLocalPosition = printerHead.localPosition;
        }

        if (printedCardVisual != null)
        {
            printedCardVisual.SetActive(false);
        }

        if (confettiEffect != null)
        {
            confettiEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Start()
    {
        ResolveConfettiEffect();
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

        if (Application.isEditor && Input.GetKeyDown(KeyCode.E))
        {
            PressGreenButton();
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

        Vector3 downPosition = headStartLocalPosition + headDownOffset;
        yield return MoveHead(headStartLocalPosition, downPosition, headMoveDuration);
        yield return new WaitForSeconds(printHoldDuration);
        yield return MoveHead(downPosition, headStartLocalPosition, headMoveDuration);

        printed = true;
        isPrinting = false;

        if (printedCardVisual != null)
        {
            printedCardVisual.SetActive(true);
        }

        PlayConfettiOnce();
        SetFeedback(doneMessage);
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
                return true;
            }
        }

        return false;
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
        ResolveConfettiEffect();
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
