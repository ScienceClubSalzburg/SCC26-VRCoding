using ithappy.Animals_FREE;
using UnityEngine;

[RequireComponent(typeof(CreatureMover))]
public class ChickenWander : MonoBehaviour
{
    [Header("Wander Area")]
    [SerializeField] private float wanderRadius = 2.5f;
    [SerializeField] private float arriveDistance = 0.25f;
    [SerializeField] private Vector2 idleTimeRange = new Vector2(0.4f, 1.4f);

    [Header("Movement")]
    [SerializeField] private bool run;
    [SerializeField] private float obstacleCheckDistance = 0.45f;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private float bounceRetargetDistance = 1.5f;

    [Header("Player Avoidance")]
    [SerializeField] private Transform player;
    [SerializeField] private float fleeDistance = 1.8f;
    [SerializeField] private float fleeRetargetDistance = 2.2f;
    [SerializeField] private bool runWhenPlayerIsNear = true;

    private CreatureMover mover;
    private Vector3 center;
    private Vector3 destination;
    private float idleTimer;
    private Vector3 lastHitNormal;
    private bool hitObstacle;

    private void Awake()
    {
        mover = GetComponent<CreatureMover>();

        MovePlayerInput playerInput = GetComponent<MovePlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        center = transform.position;
        ResolvePlayer();
        PickNewDestination();
    }

    private void Update()
    {
        if (mover == null)
        {
            return;
        }

        ResolvePlayer();

        if (TryGetFleeDirection(out Vector3 fleeDirection))
        {
            FleeFromPlayer(fleeDirection);
            return;
        }

        if (idleTimer > 0f)
        {
            idleTimer -= Time.deltaTime;
            mover.SetInput(Vector2.zero, transform.position + transform.forward, false, false);
            return;
        }

        Vector3 toDestination = destination - transform.position;
        toDestination.y = 0f;

        if (hitObstacle)
        {
            PickDestinationAwayFromObstacle();
            hitObstacle = false;
            return;
        }

        if (toDestination.sqrMagnitude <= arriveDistance * arriveDistance || IsBlocked(toDestination.normalized))
        {
            StartIdle();
            PickNewDestination();
            return;
        }

        Vector3 direction = toDestination.normalized;
        mover.SetInput(Vector2.up, transform.position + direction, run, false);
    }

    private void ResolvePlayer()
    {
        if (player != null)
        {
            return;
        }

        if (Camera.main != null)
        {
            player = Camera.main.transform;
        }
    }

    private bool TryGetFleeDirection(out Vector3 direction)
    {
        direction = Vector3.zero;

        if (player == null)
        {
            return false;
        }

        Vector3 awayFromPlayer = transform.position - player.position;
        awayFromPlayer.y = 0f;

        if (awayFromPlayer.sqrMagnitude > fleeDistance * fleeDistance)
        {
            return false;
        }

        if (awayFromPlayer.sqrMagnitude <= Mathf.Epsilon)
        {
            awayFromPlayer = transform.forward;
        }

        direction = awayFromPlayer.normalized;
        return true;
    }

    private void FleeFromPlayer(Vector3 direction)
    {
        idleTimer = 0f;

        if (IsBlocked(direction))
        {
            direction = Quaternion.Euler(0f, Random.Range(90f, 160f) * (Random.value > 0.5f ? 1f : -1f), 0f) * direction;
        }

        destination = ClampToWanderArea(transform.position + direction.normalized * fleeRetargetDistance);
        mover.SetInput(Vector2.up, transform.position + direction, run || runWhenPlayerIsNear, false);
    }

    private void StartIdle()
    {
        idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
        mover.SetInput(Vector2.zero, transform.position + transform.forward, false, false);
    }

    private void PickNewDestination()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        destination = center + new Vector3(randomCircle.x, 0f, randomCircle.y);
    }

    private bool IsBlocked(Vector3 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return false;
        }

        Vector3 origin = transform.position + Vector3.up * 0.25f;
        if (!Physics.SphereCast(origin, 0.12f, direction, out RaycastHit hit, obstacleCheckDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return !hit.transform.IsChildOf(transform);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 drawCenter = Application.isPlaying ? center : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(drawCenter, wanderRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(destination, 0.08f);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.IsChildOf(transform) || hit.normal.y > 0.5f)
        {
            return;
        }

        lastHitNormal = Vector3.ProjectOnPlane(hit.normal, Vector3.up).normalized;
        hitObstacle = lastHitNormal.sqrMagnitude > Mathf.Epsilon;
    }

    private void PickDestinationAwayFromObstacle()
    {
        Vector3 away = lastHitNormal;
        if (away.sqrMagnitude <= Mathf.Epsilon)
        {
            away = -transform.forward;
        }

        Vector3 side = Quaternion.Euler(0f, Random.Range(-55f, 55f), 0f) * away;
        destination = ClampToWanderArea(transform.position + side.normalized * bounceRetargetDistance);
        StartIdle();
    }

    private Vector3 ClampToWanderArea(Vector3 target)
    {
        Vector3 offset = target - center;
        offset.y = 0f;

        if (offset.magnitude > wanderRadius)
        {
            offset = offset.normalized * wanderRadius;
        }

        return center + offset;
    }
}
