using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour
{
    [Header("Suspension Settings")]
    [SerializeField, Range(0f, 10000f)] private float suspensionSpringFactor = 5000f;
    [SerializeField, Range(0f, 1000f)] private float suspensionDampingFactor = 300f;
    [SerializeField] private float suspensionRestDistance = 3f;
    [SerializeField] private float wheelRadius = 1f;
    [SerializeField] private float wheelMass = 6.5f;
    [SerializeField] private bool rearWheelDrive = true;

    [Header("Acceleration Settings")]
    [SerializeField, Range(0f, 5000)] private float engineFactor = 2000;

    [Header("Anchor Points")]
    [SerializeField] private Transform[] anchors = new Transform[4];

    private Rigidbody carRigidbody;
    private Transform objTransform;
    private float mass;

    // Start is called before the first frame update
    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        objTransform = GetComponent<Transform>();
        mass = carRigidbody.mass;
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
            var currentAnchor = anchors[i];
            if (Physics.Raycast(currentAnchor.position, -currentAnchor.up, out hit))
            {

                Vector3 springDir = currentAnchor.up;
                
                // Calculate suspension force
                float distanceFromGround = hit.distance - wheelRadius;
                float suspensionExtension = (suspensionRestDistance - distanceFromGround);
                Vector3 suspensionForce = springDir * suspensionSpringFactor * suspensionExtension;

                // Calculate damping force
                Vector3 tireWorldVel = carRigidbody.GetPointVelocity(currentAnchor.position);
                float velY = Vector3.Dot(springDir, tireWorldVel);
                Vector3 suspensionDampingForce = -springDir * suspensionDampingFactor * velY;

                carRigidbody.AddForceAtPosition((suspensionForce + suspensionDampingForce), currentAnchor.position);
            }
        }
    }


    /// <summary>
    /// Apply acceleration force and traction force to the car.
    /// </summary>
    private void ApplySteering()
    {
        float force = Input.GetAxis("Vertical")*engineFactor;
        float radians = Input.GetAxis("Horizontal") *  Mathf.PI/2; // Convert input to radians (2 * pi is a full circle)
       
        Vector3 localInputSteeringDir = new Vector3(Mathf.Sin(radians), 0f,  Mathf.Cos(radians));
        Vector3 worldInputSteeringDir = objTransform.TransformDirection(localInputSteeringDir);
        
        for (int i = 0; i < 4; i++)
        {
            var currentAnchor = anchors[i];

            // calculate steering force
            Vector3 steeringForce = new Vector3(0f, 0f, 0f);
            if (i < 2){
                steeringForce = force * worldInputSteeringDir;
            }

            // calculate traction force
            Vector3 tireTractionDir = currentAnchor.right;
            Vector3 tireWorldVel = carRigidbody.GetPointVelocity(currentAnchor.position);
            Vector3 tractionForce = -0.2f*tireTractionDir*wheelMass*Vector3.Dot(tireTractionDir, tireWorldVel)/Time.fixedDeltaTime;
            
            // Apply Force
            Vector3 totalForce = steeringForce + tractionForce;
            carRigidbody.AddForceAtPosition(totalForce, currentAnchor.position);

            // Visualize the force as a ray starting from the GameObject's position
            Debug.DrawRay(currentAnchor.position, steeringForce, Color.green);
            Debug.DrawRay(currentAnchor.position, tractionForce, Color.red);
            Debug.DrawRay(currentAnchor.position, carRigidbody.GetPointVelocity(currentAnchor.position), Color.blue);
        }
    }
}
