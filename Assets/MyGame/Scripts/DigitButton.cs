using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DigitButton : MonoBehaviour
{
    private enum TerminalInputMode
    {
        AutoFromText,
        NumberDigit,
        BasisValue,
        Submit,
        Delete,
        Disabled
    }

    [Header("Text")]
    [SerializeField] private TMP_Text digitText;

    [Header("XR Press")]
    [SerializeField] private float pressDistanceOnX = 0.04f;
    [SerializeField] private float pressInDuration = 0.06f;
    [SerializeField] private float pressOutDuration = 0.1f;
    [SerializeField] private float debounceSeconds = 0.2f;

    [Header("Terminal Input")]
    [SerializeField] private NumberBaseTerminalController terminal;
    [SerializeField] private TerminalInputMode terminalInputMode = TerminalInputMode.AutoFromText;

    [Header("Basis Selection")]
    [SerializeField] private bool highlightWhenSelectedBasis = true;
    [SerializeField] private Color selectedBasisColor = Color.green;

    [Header("Events")]
    [SerializeField] private UnityEvent<string> pressed;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private XRSimpleInteractable interactable;
    private Renderer[] buttonRenderers;
    private MaterialPropertyBlock propertyBlock;
    private Coroutine pressAnimation;
    private Vector3 startLocalPosition;
    private float lastPressTime = -1f;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        ResolveReferences();
        interactable.selectEntered.AddListener(OnSelectEntered);
        SubscribeToBasisChanges();
        RefreshBasisHighlight();
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        if (terminal != null)
        {
            terminal.BasisChanged.RemoveListener(UpdateBasisHighlight);
        }
    }

    public void Press()
    {
        if (Time.time - lastPressTime < debounceSeconds)
        {
            return;
        }

        lastPressTime = Time.time;

        string label = GetDigitText();
        Debug.Log($"Digit gedr\u00fcckt: {label}", this);

        PlayPressAnimation();
        SendToTerminal(label);
        pressed?.Invoke(label);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        Press();
    }

    private void ResolveReferences()
    {
        if (digitText == null)
        {
            digitText = GetComponentInChildren<TMP_Text>(true);
        }

        if (terminal == null)
        {
            terminal = GetComponentInParent<NumberBaseTerminalController>();
        }

        buttonRenderers = GetComponentsInChildren<Renderer>(true);
        propertyBlock = new MaterialPropertyBlock();

        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }
    }

    private string GetDigitText()
    {
        if (digitText == null)
        {
            return name;
        }

        return digitText.text.Trim();
    }

    private void SendToTerminal(string label)
    {
        if (terminal == null || terminalInputMode == TerminalInputMode.Disabled)
        {
            return;
        }

        switch (terminalInputMode)
        {
            case TerminalInputMode.NumberDigit:
                SendNumberDigit(label);
                break;
            case TerminalInputMode.BasisValue:
                SendBasisValue(label);
                break;
            case TerminalInputMode.Submit:
                terminal.SubmitPhaseOne();
                break;
            case TerminalInputMode.Delete:
                terminal.DeleteLastInput();
                break;
            case TerminalInputMode.AutoFromText:
                SendAuto(label);
                break;
        }
    }

    private void SendAuto(string label)
    {
        if (string.Equals(label, "OK", StringComparison.OrdinalIgnoreCase))
        {
            terminal.SubmitPhaseOne();
            return;
        }

        if (string.Equals(label, "DEL", StringComparison.OrdinalIgnoreCase)
            || string.Equals(label, "DELETE", StringComparison.OrdinalIgnoreCase))
        {
            terminal.DeleteLastInput();
            return;
        }

        if (!int.TryParse(label, out int value))
        {
            return;
        }

        if (IsBasisButton())
        {
            terminal.EnterBasisValue(value);
            return;
        }

        if (value >= 0 && value <= 9)
        {
            terminal.PressNumberDigit(value);
            return;
        }

        terminal.EnterBasisValue(value);
    }

    private void SendNumberDigit(string label)
    {
        if (int.TryParse(label, out int digit))
        {
            terminal.PressNumberDigit(digit);
        }
    }

    private void SendBasisValue(string label)
    {
        if (int.TryParse(label, out int basis))
        {
            terminal.EnterBasisValue(basis);
        }
    }

    private void SubscribeToBasisChanges()
    {
        if (terminal == null)
        {
            return;
        }

        terminal.BasisChanged.RemoveListener(UpdateBasisHighlight);
        terminal.BasisChanged.AddListener(UpdateBasisHighlight);
    }

    private void RefreshBasisHighlight()
    {
        if (terminal != null && terminal.TryGetCurrentBasis(out int basis))
        {
            UpdateBasisHighlight(basis);
            return;
        }

        UpdateBasisHighlight(-1);
    }

    private void UpdateBasisHighlight(int selectedBasis)
    {
        bool selected = highlightWhenSelectedBasis
            && IsBasisButton()
            && int.TryParse(GetDigitText(), out int ownBasis)
            && ownBasis == selectedBasis;

        ApplySelectionColor(selected);
    }

    private bool IsBasisButton()
    {
        if (terminalInputMode == TerminalInputMode.BasisValue)
        {
            return true;
        }

        if (terminalInputMode != TerminalInputMode.AutoFromText)
        {
            return false;
        }

        Transform current = transform;
        while (current != null)
        {
            if (current.name.StartsWith("Basis", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void ApplySelectionColor(bool selected)
    {
        if (buttonRenderers == null)
        {
            return;
        }

        foreach (Renderer buttonRenderer in buttonRenderers)
        {
            if (buttonRenderer == null)
            {
                continue;
            }

            if (!selected)
            {
                buttonRenderer.SetPropertyBlock(null);
                continue;
            }

            propertyBlock.Clear();
            Material material = buttonRenderer.sharedMaterial;
            if (material != null && material.HasProperty(BaseColorId))
            {
                propertyBlock.SetColor(BaseColorId, selectedBasisColor);
            }
            else
            {
                propertyBlock.SetColor(ColorId, selectedBasisColor);
            }

            buttonRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void PlayPressAnimation()
    {
        if (pressAnimation != null)
        {
            StopCoroutine(pressAnimation);
            transform.localPosition = startLocalPosition;
        }

        pressAnimation = StartCoroutine(AnimatePress());
    }

    private IEnumerator AnimatePress()
    {
        Vector3 pressedPosition = startLocalPosition + Vector3.right * pressDistanceOnX;

        yield return MoveButton(startLocalPosition, pressedPosition, pressInDuration);
        yield return MoveButton(pressedPosition, startLocalPosition, pressOutDuration);

        pressAnimation = null;
    }

    private IEnumerator MoveButton(Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f)
        {
            transform.localPosition = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }

        transform.localPosition = to;
    }
}
