using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NumberBaseTerminalController : MonoBehaviour
{
    private enum InputTarget
    {
        Basis,
        Number
    }

    private enum TerminalPhase
    {
        PhaseOne,
        PhaseTwo
    }

    [Header("Display")]
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private TMP_Text basisDisplayText;
    [SerializeField] private TMP_Text numberDisplayText;

    [Header("Input Panels")]
    [SerializeField] private GameObject decimalNumberPad;
    [SerializeField] private GameObject binaryDigitPanel;

    [Header("Exit Door")]
    [SerializeField] private Transform exitDoor;
    [SerializeField] private string exitDoorName = "ExitDoor";
    [SerializeField] private string exitDoorWingName = "doorWing";
    [SerializeField] private Vector3 exitDoorOpenEulerOffset = new Vector3(0f, -90f, 0f);
    [SerializeField] private bool useAbsoluteExitDoorOpenEuler;
    [SerializeField] private Vector3 exitDoorOpenPositionOffset = new Vector3(1.2f, 0f, 0f);
    [SerializeField] private float exitDoorOpenDuration = 1f;
    [SerializeField] private bool useHingeOpenAnimation = true;
    [SerializeField] private bool disableDoorCollidersAfterOpen = true;
    [SerializeField] private bool hideDoorWingAfterOpen = false;

    [Header("Target")]
    [SerializeField] private int expectedBasis = 10;
    [SerializeField] private int expectedNumber = 37;

    [Header("Messages")]
    [SerializeField] private string startMessage = "Gib die Basis und die Zahl ein.";
    [SerializeField] private string wrongBasisMessage = "Falsche Basis.\nDieses Terminal erwartet die Basis 10.";
    [SerializeField] private string wrongNumberMessage = "Falsche Zahl.\nPr\u00fcfe die eingegebene Zahl noch einmal.";
    [SerializeField] private string wrongBinaryMessage = "Der Bin\u00e4rcode ist nicht korrekt.\nPr\u00fcfe die Reihenfolge der Reste.";
    [SerializeField] private string binarySuccessMessage = "Zugriff gew\u00e4hrt\n37\u2081\u2080 = 100101\u2082\nGleicher Wert - andere Darstellung\nTEST bestanden";
    [SerializeField] private string missingInputMessage = "Gib zuerst Basis und Zahl ein.";

    [Header("Events")]
    [SerializeField] private UnityEvent phaseTwoStarted;
    [SerializeField] private UnityEvent<int> basisChanged = new UnityEvent<int>();

    private readonly System.Text.StringBuilder basisInput = new System.Text.StringBuilder();
    private readonly System.Text.StringBuilder numberInput = new System.Text.StringBuilder();
    private InputTarget activeInputTarget = InputTarget.Basis;
    private TerminalPhase phase = TerminalPhase.PhaseOne;
    private bool exitDoorOpened;
    private Transform exitDoorAnimationTarget;

    public bool IsPhaseTwo => phase == TerminalPhase.PhaseTwo;
    public string CurrentBasisInput => basisInput.ToString();
    public string CurrentNumberInput => numberInput.ToString();
    public UnityEvent<int> BasisChanged => basisChanged;

    private void Awake()
    {
        ResolveDisplayReferences();
        ResolveInputPanels();
        UpdateInputPanels();
        RefreshDisplay(startMessage);
    }

    public void SelectBasisInput()
    {
        activeInputTarget = InputTarget.Basis;
        RefreshDisplay();
    }

    public void SelectNumberInput()
    {
        activeInputTarget = InputTarget.Number;
        RefreshDisplay();
    }

    public void PressBasisDigit(int digit)
    {
        activeInputTarget = InputTarget.Basis;
        AppendDigit(digit);
        NotifyBasisChanged();
    }

    public void EnterBasisValue(int basis)
    {
        if (basis < 0)
        {
            return;
        }

        activeInputTarget = InputTarget.Basis;
        basisInput.Clear();
        basisInput.Append(basis);
        RefreshDisplay();
        NotifyBasisChanged();
    }

    public void PressNumberDigit(int digit)
    {
        activeInputTarget = InputTarget.Number;
        AppendDigit(digit);
    }

    public void PressDigit(int digit)
    {
        AppendDigit(digit);
    }

    public void DeleteLastInput()
    {
        System.Text.StringBuilder input = GetActiveInput();
        if (input.Length > 0)
        {
            input.Length--;
        }

        RefreshDisplay();
        if (activeInputTarget == InputTarget.Basis)
        {
            NotifyBasisChanged();
        }
    }

    public void ClearInput()
    {
        GetActiveInput().Clear();
        RefreshDisplay();
        if (activeInputTarget == InputTarget.Basis)
        {
            NotifyBasisChanged();
        }
    }

    public void ClearAllInput()
    {
        basisInput.Clear();
        numberInput.Clear();
        activeInputTarget = InputTarget.Basis;
        phase = TerminalPhase.PhaseOne;
        RefreshDisplay(startMessage);
        NotifyBasisChanged();
    }

    public bool TryGetCurrentBasis(out int basis)
    {
        return int.TryParse(basisInput.ToString(), out basis);
    }

    public void SubmitPhaseOne()
    {
        if (phase == TerminalPhase.PhaseTwo)
        {
            SubmitPhaseTwo();
            return;
        }

        if (phase != TerminalPhase.PhaseOne)
        {
            return;
        }

        if (!TryParseInputs(out int basis, out int number))
        {
            RefreshDisplay(missingInputMessage);
            return;
        }

        if (basis != expectedBasis)
        {
            RefreshDisplay(wrongBasisMessage);
            return;
        }

        if (number != expectedNumber)
        {
            RefreshDisplay(wrongNumberMessage);
            return;
        }

        string successMessage = BuildSuccessMessage(number);
        phase = TerminalPhase.PhaseTwo;
        numberInput.Clear();
        activeInputTarget = InputTarget.Number;
        RefreshDisplay(successMessage);
        phaseTwoStarted?.Invoke();
    }

    private void SubmitPhaseTwo()
    {
        string expectedBinary = System.Convert.ToString(expectedNumber, 2);
        if (numberInput.ToString() != expectedBinary)
        {
            RefreshDisplay(wrongBinaryMessage);
            return;
        }

        RefreshDisplay(binarySuccessMessage);
        OpenExitDoor();
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
            Debug.LogWarning($"NumberBaseTerminal could not find exit door '{exitDoorName}'.", this);
            return;
        }

        MakeDoorDynamic(exitDoor);
        exitDoorOpened = true;
        Debug.Log($"NumberBaseTerminal opens exit door '{exitDoor.name}'.", this);
        StartCoroutine(OpenExitDoorRoutine());
    }

    private static void MakeDoorDynamic(Transform doorTransform)
    {
        if (doorTransform == null)
        {
            return;
        }

        foreach (Transform child in doorTransform.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.isStatic = false;
        }
    }

    private IEnumerator OpenExitDoorRoutine()
    {
        Transform animatedDoor = GetExitDoorAnimationTarget();
        Vector3 closedPosition = animatedDoor.localPosition;
        Vector3 openPosition = closedPosition + exitDoorOpenPositionOffset;
        Quaternion closedRotation = animatedDoor.localRotation;
        Quaternion openRotation = useAbsoluteExitDoorOpenEuler
            ? Quaternion.Euler(exitDoorOpenEulerOffset)
            : closedRotation * Quaternion.Euler(exitDoorOpenEulerOffset);

        if (exitDoorOpenDuration <= 0f)
        {
            animatedDoor.localPosition = openPosition;
            animatedDoor.localRotation = openRotation;
            FinishDoorOpen();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < exitDoorOpenDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / exitDoorOpenDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            animatedDoor.localPosition = Vector3.Lerp(closedPosition, openPosition, easedT);
            animatedDoor.localRotation = Quaternion.Slerp(closedRotation, openRotation, easedT);
            yield return null;
        }

        animatedDoor.localPosition = openPosition;
        animatedDoor.localRotation = openRotation;
        FinishDoorOpen();
    }

    private Transform GetExitDoorAnimationTarget()
    {
        if (!useHingeOpenAnimation || exitDoor == null)
        {
            return exitDoor;
        }

        if (exitDoorAnimationTarget != null)
        {
            return exitDoorAnimationTarget;
        }

        Transform parent = exitDoor.parent;
        GameObject hingeObject = new GameObject($"{exitDoor.name}_RuntimeHinge");
        Transform hinge = hingeObject.transform;
        hinge.SetParent(parent, false);
        hinge.SetPositionAndRotation(GetDoorHingePosition(), exitDoor.rotation);
        exitDoor.SetParent(hinge, true);
        exitDoorAnimationTarget = hinge;
        return exitDoorAnimationTarget;
    }

    private Vector3 GetDoorHingePosition()
    {
        MeshFilter meshFilter = exitDoor.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Bounds localBounds = meshFilter.sharedMesh.bounds;
            Vector3 hingeLocalPosition = new Vector3(localBounds.min.x, localBounds.center.y, localBounds.center.z);
            return exitDoor.TransformPoint(hingeLocalPosition);
        }

        Renderer doorRenderer = exitDoor.GetComponentInChildren<Renderer>();
        if (doorRenderer != null)
        {
            Bounds bounds = doorRenderer.bounds;
            return new Vector3(bounds.min.x, bounds.center.y, bounds.center.z);
        }

        return exitDoor.position;
    }

    private void FinishDoorOpen()
    {
        DisableDoorCollidersIfNeeded();
        HideDoorWingIfNeeded();
    }

    private void DisableDoorCollidersIfNeeded()
    {
        if (!disableDoorCollidersAfterOpen || exitDoor == null)
        {
            return;
        }

        Collider[] doorColliders = exitDoor.GetComponentsInChildren<Collider>(true);
        foreach (Collider doorCollider in doorColliders)
        {
            doorCollider.enabled = false;
        }
    }

    private void HideDoorWingIfNeeded()
    {
        if (!hideDoorWingAfterOpen || exitDoor == null)
        {
            return;
        }

        exitDoor.gameObject.SetActive(false);
    }

    private void AppendDigit(int digit)
    {
        if (digit < 0 || digit > 9)
        {
            return;
        }

        GetActiveInput().Append(digit);
        RefreshDisplay();
    }

    private bool TryParseInputs(out int basis, out int number)
    {
        bool hasBasis = int.TryParse(basisInput.ToString(), out basis);
        bool hasNumber = int.TryParse(numberInput.ToString(), out number);
        return hasBasis && hasNumber;
    }

    private string BuildSuccessMessage(int number)
    {
        return $"Eingabe erkannt\n{number} im Dezimalsystem.\nJetzt dieselbe Zahl im Bin\u00e4rsystem.";
    }

    private System.Text.StringBuilder GetActiveInput()
    {
        return activeInputTarget == InputTarget.Basis ? basisInput : numberInput;
    }

    private void RefreshDisplay(string message = null)
    {
        if (basisDisplayText != null)
        {
            basisDisplayText.text = $"Basis: {FormatInput(basisInput)}";
        }

        if (numberDisplayText != null)
        {
            numberDisplayText.text = $"Zahl: {FormatInput(numberInput)}";
        }

        if (outputText != null)
        {
            outputText.text = message ?? BuildInputMessage();
        }
    }

    private string BuildInputMessage()
    {
        string activeLabel = activeInputTarget == InputTarget.Basis ? "Basis" : "Zahl";
        return $"Aktive Eingabe: {activeLabel}\nBasis: {FormatInput(basisInput)}\nZahl: {FormatInput(numberInput)}";
    }

    private static string FormatInput(System.Text.StringBuilder input)
    {
        return input.Length == 0 ? "_" : input.ToString();
    }

    private void NotifyBasisChanged()
    {
        int basis = TryGetCurrentBasis(out int currentBasis) ? currentBasis : -1;
        UpdateInputPanels(basis);
        basisChanged?.Invoke(basis);
    }

    private void UpdateInputPanels()
    {
        UpdateInputPanels(TryGetCurrentBasis(out int basis) ? basis : -1);
    }

    private void UpdateInputPanels(int selectedBasis)
    {
        if (decimalNumberPad != null)
        {
            decimalNumberPad.SetActive(selectedBasis == 10);
        }

        if (binaryDigitPanel != null)
        {
            binaryDigitPanel.SetActive(selectedBasis == 2);
        }
    }

    private void ResolveDisplayReferences()
    {
        if (outputText != null)
        {
            return;
        }

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text.text != "Basis")
            {
                outputText = text;
                return;
            }
        }
    }

    private void ResolveInputPanels()
    {
        if (decimalNumberPad == null)
        {
            decimalNumberPad = FindChildGameObject("DigitPanel-Decimal");
            if (decimalNumberPad == null)
            {
                decimalNumberPad = FindChildGameObject("NumberPad-Decimal");
            }
        }

        if (binaryDigitPanel == null)
        {
            binaryDigitPanel = FindChildGameObject("DigitPanel-Binary");
        }
    }

    private void ResolveExitDoor()
    {
        if (exitDoor != null)
        {
            Transform linkedDoorWing = FindChildTransform(exitDoor, exitDoorWingName);
            if (linkedDoorWing != null)
            {
                exitDoor = linkedDoorWing;
            }

            return;
        }

        GameObject exitDoorRoot = GameObject.Find(exitDoorName);
        if (exitDoorRoot == null)
        {
            return;
        }

        Transform doorWing = FindChildTransform(exitDoorRoot.transform, exitDoorWingName);
        exitDoor = doorWing != null ? doorWing : exitDoorRoot.transform;
    }

    private GameObject FindChildGameObject(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == childName)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private static Transform FindChildTransform(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

}
