using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class CardResetTrigger : MonoBehaviour
{
    private void Awake()
    {
        EnsureTriggerCollider();
    }

    private void Reset()
    {
        EnsureTriggerCollider();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        CardResettable card = other.GetComponentInParent<CardResettable>();
        if (card != null)
        {
            card.ResetToStart();
        }
    }

    private void EnsureTriggerCollider()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
}
