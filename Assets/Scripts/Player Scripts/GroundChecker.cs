using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] private float offset = 0.1f;
    [SerializeField] private float radius = 0.4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private bool isGrounded;

    public bool IsGrounded { get; private set; }

    private void Update()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + offset, transform.position.z);
        IsGrounded = Physics.CheckSphere(spherePosition, radius, groundMask, QueryTriggerInteraction.Ignore);
        isGrounded = IsGrounded;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + offset, transform.position.z);
        Gizmos.DrawWireSphere(spherePosition, radius);
    }
}