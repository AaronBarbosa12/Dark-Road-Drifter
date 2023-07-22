using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour
{
    [Header("Suspension Settings")]
    [SerializeField, Range(0f, 100f)] private float suspensionSpringFactor = 3f;
    [SerializeField, Range(0f, 100f)] private float suspensionDampingFactor = 3f;
    [SerializeField] private float suspensionRestDistance = 3f;
    [SerializeField] private float wheelRadius = 1f;

    [Header("Acceleration Settings")]
    [SerializeField, Range(0f, 2000f)] private float engineFactor = 5f;

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
        ApplySteering();
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
                float distanceFromGround = hit.distance - wheelRadius;
                float suspensionExtension = (suspensionRestDistance - distanceFromGround);
                Vector3 suspensionForce = transform.up * suspensionSpringFactor * suspensionExtension;

                // Calculate damping force
                Vector3 suspensionDampingForce = -suspensionDampingFactor * carRigidbody.velocity.y * transform.up;

                carRigidbody.AddForceAtPosition((suspensionForce + suspensionDampingForce), anchors[i].position, ForceMode.Acceleration);
            }
        }
    }


    /// <summary>
    /// Apply acceleration force and traction force to the car.
    /// </summary>
    private void ApplySteering()
    {
        RaycastHit hit;
        Vector3 forwardForce = Input.GetAxis("Vertical")*engineFactor*transform.forward;
        for (int i = 0; i < anchors.Length; i++)
        {
            carRigidbody.AddForceAtPosition(forwardForce, anchors[i].position);
            // Visualize the force as a ray starting from the GameObject's position
            Debug.DrawRay(anchors[i].position, forwardForce, Color.green);
        }
    }
}
