using System.Text;
using TMPro;
using UnityEngine;

public class DecimalDivisionCalculator : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TMP_Text inputText;
    [SerializeField] private TMP_Text divisorText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text remainderText;

    [Header("Calculation")]
    [SerializeField] private int divisor = 2;
    [SerializeField] private string emptyInputText = "00";
    [SerializeField] private string emptyOutputText = "00";

    private readonly StringBuilder input = new StringBuilder();

    private void Awake()
    {
        ResolveReferences();
        RefreshDisplay();
    }

    public void PressNumberDigit(int digit)
    {
        if (digit < 0 || digit > 9)
        {
            return;
        }

        input.Append(digit);
        RefreshDisplay();
    }

    public void DeleteLastInput()
    {
        if (input.Length > 0)
        {
            input.Length--;
        }

        RefreshDisplay();
    }

    public void ClearInput()
    {
        input.Clear();
        RefreshDisplay();
    }

    public void Submit()
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (divisorText != null)
        {
            divisorText.text = divisor.ToString();
        }

        if (inputText != null)
        {
            inputText.text = input.Length == 0 ? emptyInputText : input.ToString();
        }

        if (resultText == null && remainderText == null)
        {
            return;
        }

        if (input.Length == 0 || divisor == 0 || !int.TryParse(input.ToString(), out int value))
        {
            SetOutputs(emptyOutputText, emptyOutputText);
            return;
        }

        int result = value / divisor;
        int remainder = value % divisor;
        SetOutputs(result.ToString(), remainder.ToString());
    }

    private void SetOutputs(string result, string remainder)
    {
        if (resultText != null)
        {
            resultText.text = result;
        }

        if (remainderText != null)
        {
            remainderText.text = remainder;
        }
    }

    private void ResolveReferences()
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            switch (text.name)
            {
                case "ZahlEingabe":
                    inputText = inputText != null ? inputText : text;
                    break;
                case "Divisor":
                    divisorText = divisorText != null ? divisorText : text;
                    break;
                case "ZahlErgebnis":
                    resultText = resultText != null ? resultText : text;
                    break;
                case "ZahlRest":
                    remainderText = remainderText != null ? remainderText : text;
                    break;
            }
        }
    }
}
