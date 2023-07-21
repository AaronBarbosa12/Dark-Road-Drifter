using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour
{
    [Header("Suspension Settings")]
    [SerializeField, Range(0f, 100f)] private float suspensionSpringFactor = 10f;
    [SerializeField, Range(0f, 10f)] private float suspensionDampingFactor = 3f;
    [SerializeField] private float suspensionRestDistance = 3f;

    [Header("Anchor Points")]
    [SerializeField] private Transform[] anchors = new Transform[4];

    private Rigidbody carRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        ApplySuspension();
    }

    /// <summary>
    /// Apply suspension force and damping to the car.
    /// </summary>
    private void ApplySuspension()
    {
        RaycastHit hit;
        for (int i = 0; i < anchors.Length; i++)
        {
            if (Physics.Raycast(anchors[i].position, -anchors[i].up, out hit))
            {
                // Calculate suspension force
                float distanceFromGround = hit.distance;
                Vector3 suspensionForce = transform.up * suspensionSpringFactor * (suspensionRestDistance - distanceFromGround);

                // Calculate damping force
                Vector3 suspensionDampingForce = -suspensionDampingFactor * carRigidbody.velocity.y * Vector3.up;

                carRigidbody.AddForceAtPosition((suspensionForce + suspensionDampingForce), anchors[i].position, ForceMode.Acceleration);
            }
        }
    }
}
