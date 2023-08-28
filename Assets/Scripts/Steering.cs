using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour
{
    [Header("Car Suspension Settings")]
    [SerializeField, Range(0f, 10000f)] private float suspensionSpringFactor = 5000f;
    [SerializeField, Range(0f, 1000f)] private float suspensionDampingFactor = 300f;
    [SerializeField] private float suspensionRestDistance = 3f;
    [SerializeField] private float wheelRadius = 0.5f;
    [SerializeField] private float wheelMass = 50f;

    [Header("Acceleration Settings")]
    [SerializeField, Range(0f, 10000)] private float maxVelocity = 1000f;
    [SerializeField, Range(0f, 20000)] private float maxTorque= 4000f;
    public AnimationCurve accelerationCurve;

    [Header("Wheel Anchor Points")]
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
        ApplyForces();
    }

    /// <summary>
    /// Apply acceleration, traction and suspension forces to car
    /// </summary>
    private void ApplyForces()
    {
        Vector3 steeringForce;
        Vector3 tractionForce;
        Vector3 suspensionForce;

        float pedalInput = Input.GetAxis("Vertical");
        for (int i = 0; i < 4; i++)
        {
            var currentAnchor = anchors[i];

            // Calculate steering Force (front wheel drive)
            if (i < 2){
                steeringForce = calcSteeringForce(currentAnchor, pedalInput);
            }
            else{
                steeringForce = new Vector3(0, 0f, 0f);
            }
            
            tractionForce = calcTractionForce(currentAnchor, currentAnchor.right);
            suspensionForce = calcSuspensionForce(currentAnchor);

            // Apply all forces
            Vector3 totalForce = steeringForce  + suspensionForce + tractionForce;
            carRigidbody.AddForceAtPosition(totalForce, currentAnchor.position);            
            Debug.DrawRay(currentAnchor.position, steeringForce, Color.green);
        }
    }

    /// <summary>
    /// Calculate traction force on the given wheel.
    /// </summary>
    private Vector3 calcTractionForce(Transform currentAnchor, Vector3 tractionDir){
        Vector3 tireWorldVel = carRigidbody.GetPointVelocity(currentAnchor.position);
        float tractionDirVel = Vector3.Dot(tireWorldVel, tractionDir);
        Vector3 tractionForce = (-0.2f * tractionDir * wheelMass * tractionDirVel)/Time.fixedDeltaTime;
        return tractionForce;
    }

    /// <summary>
    /// Apply suspension force on the given wheel.
    /// </summary>
    private Vector3 calcSuspensionForce(Transform currentAnchor)
    {
        RaycastHit hit;
        Vector3 totalForce;
        Vector3 springDir = currentAnchor.up;
        
        if (Physics.Raycast(currentAnchor.position, -springDir, out hit))
        {
            
            // Calculate suspension force
            float distanceFromGround = hit.distance - wheelRadius;
            float suspensionExtension = (suspensionRestDistance - distanceFromGround);
            Vector3 suspensionForce = springDir * suspensionSpringFactor * suspensionExtension;

            // Calculate damping force
            Vector3 tireWorldVel = carRigidbody.GetPointVelocity(currentAnchor.position);
            float velY = Vector3.Dot(springDir, tireWorldVel);
            Vector3 suspensionDampingForce = -springDir * suspensionDampingFactor * velY;

            totalForce = suspensionForce + suspensionDampingForce;
            return totalForce;
        }
        else{
            totalForce = new Vector3(0, 0f, 0f);
        }
        return totalForce;
    }

    /// <summary>
    /// Calculate acceleration force on given wheel
    /// </summary>
    private Vector3 calcSteeringForce(Transform currentAnchor, float pedalInput){    
        Vector3 steeringForce;
        float torqueFactor = 0f;

        float groundPlaneVel = calcGroundPlaneVelocity(currentAnchor);
        
        if (pedalInput >= 0){
            // Accelerate up until we reach max speed
            float velocityRatio = Mathf.Abs(groundPlaneVel/maxVelocity);
            if (0f <= velocityRatio && velocityRatio <= 1f){
                torqueFactor = accelerationCurve.Evaluate(velocityRatio);
            }
            else if (velocityRatio > 1f){
                torqueFactor = 0;
            }
            steeringForce = pedalInput * maxTorque * torqueFactor * currentAnchor.forward;
        }
        else {
            // Brake
            steeringForce = calcTractionForce(currentAnchor, currentAnchor.forward);
        }

        return steeringForce;
    }
    /// <summary>
    /// Calculate velocity in the ground plane
    /// </summary>
    private float calcGroundPlaneVelocity(Transform currentAnchor){
        Vector3 tireWorldVel = carRigidbody.GetPointVelocity(currentAnchor.position);
        // Calculate the normal vector of the plane by taking the cross product of vectorA and vectorB
        Vector3 planeNormal = Vector3.Cross(currentAnchor.forward, currentAnchor.right).normalized;
        // Project the velocity vector onto the plane using vector projection formula
        float groundPlaneVel = (tireWorldVel - Vector3.Dot(tireWorldVel, planeNormal) * planeNormal).magnitude;
        return groundPlaneVel;
    }
}
