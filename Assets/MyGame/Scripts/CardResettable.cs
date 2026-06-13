using UnityEngine;

[DisallowMultipleComponent]
public class CardResettable : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody cardRigidbody;
    private bool hasStartPose;

    private void Awake()
    {
        cardRigidbody = GetComponent<Rigidbody>();
        StoreStartPose();
    }

    private void Start()
    {
        if (!hasStartPose)
        {
            StoreStartPose();
        }
    }

    public void ResetToStart()
    {
        if (!hasStartPose)
        {
            StoreStartPose();
        }

        if (cardRigidbody != null)
        {
            cardRigidbody.linearVelocity = Vector3.zero;
            cardRigidbody.angularVelocity = Vector3.zero;
            cardRigidbody.position = startPosition;
            cardRigidbody.rotation = startRotation;
            cardRigidbody.Sleep();
        }

        transform.SetPositionAndRotation(startPosition, startRotation);
    }

    private void StoreStartPose()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        hasStartPose = true;
    }
}
