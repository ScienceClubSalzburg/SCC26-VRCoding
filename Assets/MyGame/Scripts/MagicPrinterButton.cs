using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MagicPrinterButton : MonoBehaviour
{
    [SerializeField] private MagicPrinterController printer;
    [SerializeField] private string ignoredCardNameContains = "Card";
    [SerializeField] private Transform buttonVisual;
    [SerializeField] private Vector3 pressedLocalOffset = new Vector3(0f, -0.035f, 0f);
    [SerializeField] private float pressInDuration = 0.08f;
    [SerializeField] private float pressOutDuration = 0.14f;

    private Vector3 startLocalPosition;
    private Coroutine animationRoutine;
    private XRSimpleInteractable interactable;

    private void Awake()
    {
        if (printer == null)
        {
            printer = GetComponentInParent<MagicPrinterController>();
        }

        if (buttonVisual == null)
        {
            buttonVisual = transform;
        }

        startLocalPosition = buttonVisual.localPosition;

        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }
    }

    private void OnEnable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelectEntered);
        }
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
        }
    }

    private void OnMouseDown()
    {
        Press();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsCard(collision.gameObject))
        {
            Press();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsCard(other.gameObject))
        {
            Press();
        }
    }

    public void Press()
    {
        PlayPressAnimation();

        if (printer != null)
        {
            printer.PressGreenButton();
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        Press();
    }

    private void PlayPressAnimation()
    {
        if (buttonVisual == null)
        {
            return;
        }

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            buttonVisual.localPosition = startLocalPosition;
        }

        animationRoutine = StartCoroutine(AnimatePress());
    }

    private IEnumerator AnimatePress()
    {
        Vector3 pressedPosition = startLocalPosition + pressedLocalOffset;

        yield return MoveButton(startLocalPosition, pressedPosition, pressInDuration);
        yield return MoveButton(pressedPosition, startLocalPosition, pressOutDuration);

        animationRoutine = null;
    }

    private IEnumerator MoveButton(Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f)
        {
            buttonVisual.localPosition = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            buttonVisual.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        buttonVisual.localPosition = to;
    }

    private bool IsCard(GameObject other)
    {
        if (other == null || string.IsNullOrEmpty(ignoredCardNameContains))
        {
            return false;
        }

        return other.name.Contains(ignoredCardNameContains) || other.transform.root.name.Contains(ignoredCardNameContains);
    }
}
