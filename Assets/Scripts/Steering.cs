using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour
{
    [SerializeField] float suspension_spring_factor = 10f;
    [SerializeField] float suspension_damping_factor = 3f;
    [SerializeField] float suspension_rest_distance = 3f;
    Rigidbody car_rigidbody;

    [SerializeField] Transform[] anchors = new Transform[4];
    RaycastHit[] hits = new RaycastHit[4];

    // Start is called before the first frame update
    void Start()
    {
        car_rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        for (int i = 0; i < 4; i++)
            ApplySuspension(anchors[i], hits[i]);
    }

    void ApplySuspension(Transform anchor, RaycastHit hit){
        if(Physics.Raycast(anchor.position, -anchor.up, out hit)){
            // Calculate suspension force
            float distanceFromGround = hit.distance;
            Vector3 suspension_force = transform.up*suspension_spring_factor*(suspension_rest_distance - distanceFromGround);
            
            // Calculate Damping Force
            Vector3 suspension_damping_force_val = -suspension_damping_factor*car_rigidbody.velocity.y*Vector3.up;

            car_rigidbody.AddForceAtPosition((suspension_force+suspension_damping_force_val), anchor.position, ForceMode.Acceleration);
        }
    }

}
