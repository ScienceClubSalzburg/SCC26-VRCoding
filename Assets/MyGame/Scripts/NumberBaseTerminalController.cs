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

    [Header("Target")]
    [SerializeField] private int expectedBasis = 10;
    [SerializeField] private int expectedNumber = 37;
    [SerializeField] private int targetBasis = 2;

    [Header("Messages")]
    [SerializeField] private string startMessage = "Gib die Basis und die Zahl ein.";
    [SerializeField] private string wrongBasisMessage = "Falsche Basis.\nDieses Terminal erwartet die Basis 10.";
    [SerializeField] private string wrongNumberMessage = "Falsche Zahl.\nPr\u00fcfe die eingegebene Zahl noch einmal.";
    [SerializeField] private string missingInputMessage = "Gib zuerst Basis und Zahl ein.";

    [Header("Events")]
    [SerializeField] private UnityEvent phaseTwoStarted;
    [SerializeField] private UnityEvent<int> basisChanged = new UnityEvent<int>();

    private readonly System.Text.StringBuilder basisInput = new System.Text.StringBuilder();
    private readonly System.Text.StringBuilder numberInput = new System.Text.StringBuilder();
    private InputTarget activeInputTarget = InputTarget.Basis;
    private TerminalPhase phase = TerminalPhase.PhaseOne;

    public bool IsPhaseTwo => phase == TerminalPhase.PhaseTwo;
    public string CurrentBasisInput => basisInput.ToString();
    public string CurrentNumberInput => numberInput.ToString();
    public UnityEvent<int> BasisChanged => basisChanged;

    private void Awake()
    {
        ResolveDisplayReferences();
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
        if (phase != TerminalPhase.PhaseOne || basis < 0)
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

        phase = TerminalPhase.PhaseTwo;
        RefreshDisplay(BuildSuccessMessage(number));
        phaseTwoStarted?.Invoke();
    }

    private void AppendDigit(int digit)
    {
        if (phase != TerminalPhase.PhaseOne || digit < 0 || digit > 9)
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
        return $"Eingabe erkannt\n{number} im Dezimalsystem\nIch ben\u00f6tige dieselbe Zahl im Bin\u00e4rsystem.\nZielsystem: Basis {targetBasis}";
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
        basisChanged?.Invoke(TryGetCurrentBasis(out int basis) ? basis : -1);
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

}
