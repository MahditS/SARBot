using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UI;
public class Move : Agent
{
    public Rigidbody rb;
    public float moveSpeed;
    public ArmAgent arm;

    public bool isStuck = false;
    public Vector3 lastPos;
    public Quaternion lastquat;


    [SerializeField] public Transform target;
    public Transform obstacle;
        public Transform obstacle2;
    public Transform obstacle3;


    [SerializeField] private Transform referenceTransform;
    private float? previousDistance = null;
    public float previousDistanceToSurvivor;


    private Vector3 lastDropPoint;
private float dropInterval = 2.5f; // seconds
private float dropTimer = 0f;
private float idleRadius = 6.5f; // meters


    GameObject FindClosestWithTag(string tag)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
        GameObject closestObject = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject obj in taggedObjects)
        {
            Vector3 directionToTarget = obj.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestObject = obj;
            }
        }

        return closestObject;
    }



    public override void OnEpisodeBegin()
    {


        lastDropPoint = referenceTransform.InverseTransformPoint(transform.position);
        dropTimer = 0f;
        GameObject closest = FindClosestWithTag("survivorParent");
        visitedCells.Clear();
        target = closest.transform;
        arm.enabled = false;

        lastCell = new Vector2Int(
       Mathf.FloorToInt(transform.position.x / cellSize),
       Mathf.FloorToInt(transform.position.z / cellSize)
   );
        revisitPenaltyTimer = 0f;
        visitedPositions.Clear();

        if (target)
        {
            GameObject G = FindClosestWithTag("survivorParent");

            // float x = UnityEngine.Random.Range(-6.5f, 7.75f);
            float x = UnityEngine.Random.Range(-16.5f, 40f);
            float z = UnityEngine.Random.Range(-30f, 30f);
            Vector3 off = new Vector3(x, -1.299f, z);
            // G.transform.position = referenceTransform.TransformPoint(off);
            x = UnityEngine.Random.Range(-16.5f, 40f);
            z = UnityEngine.Random.Range(-30f, 30f);
            off = new Vector3(43.8f, -1.299f, -5.2f);
            transform.localPosition = off;
            x = UnityEngine.Random.Range(-16.5f, 40f);
            z = UnityEngine.Random.Range(-30f, 30f);
            off = new Vector3(x, -1.299f, z);
            // obstacle.position = referenceTransform.TransformPoint(off);
            x = UnityEngine.Random.Range(-16.5f, 40f);
            z = UnityEngine.Random.Range(-30f, 30f);
            off = new Vector3(x, -1.299f, z);
            // obstacle2.position = referenceTransform.TransformPoint(off);
            x = UnityEngine.Random.Range(-16.5f, 40f);
            z = UnityEngine.Random.Range(-30f, 30f);
            off = new Vector3(x, -1.299f, z);
            // obstacle3.position = referenceTransform.TransformPoint(off);

        }

        Vector3 relativePosition = referenceTransform.InverseTransformPoint(target.position);
        Vector3 relativePosition2 = referenceTransform.InverseTransformPoint(transform.position);
        Vector3 relativePosition3 = referenceTransform.InverseTransformPoint(obstacle.position);

        previousDistanceToSurvivor = Vector3.Distance(relativePosition2, relativePosition);

    }

    Transform FindParentWithTag(Transform child, string tag)
    {
        Transform current = child.parent;
        while (current != null)
        {
            if (current.CompareTag(tag))
                return current;

            current = current.parent;
        }
        return null; 
    }

    HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();

    HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();

    float cellSize = 3f;


    public override void CollectObservations(VectorSensor sensor)
    {
        GameObject closest = FindClosestWithTag("survivorMain");
        target = closest.transform;

        Vector3 relativePosition = referenceTransform.InverseTransformPoint(target.position);
        Vector3 relativePosition2 = referenceTransform.InverseTransformPoint(transform.position);
        Vector3 relativePosition3 = referenceTransform.InverseTransformPoint(obstacle.position);


        sensor.AddObservation(relativePosition2);

        // Debug.Log(Vector3.Distance(relativePosition2, relativePosition));
    }
    
    private float revisitPenaltyCooldown = 1.0f;
    private float revisitPenaltyTimer = 0f;

    private Vector2Int lastCell;


    public override void OnActionReceived(ActionBuffers actions)
    {
        if (Input.GetKey("r"))
        {
            // EndEpisode();
        }
        Vector3 relativePosition = referenceTransform.InverseTransformPoint(target.position);
        Vector3 relativePosition2 = referenceTransform.InverseTransformPoint(transform.position);
        Vector3 relativePosition3 = referenceTransform.InverseTransformPoint(obstacle.position);

        int x = actions.DiscreteActions[0];
        int z = actions.DiscreteActions[1];

        Vector3 addForce = new Vector3(0, 0, 0);

        switch (x)
        {
            case 0: addForce.x = 0; break;
            case 1: addForce.x = -17f; break;
            case 2: addForce.x = +17f; break;
        }
        switch (z)
        {
            case 0: addForce.z = 0; break;
            case 1: addForce.z = -1f; break;
            case 2: addForce.z = +1f; break;
        }
        rb.velocity = transform.TransformDirection(new Vector3(0, rb.velocity.y, addForce.x * moveSpeed * Time.deltaTime));
        Quaternion rotation = Quaternion.Euler(0, addForce.z, 0); // Rotate 90° around Y
        GetComponent<Rigidbody>().MoveRotation(GetComponent<Rigidbody>().rotation * rotation);

        // if (Vector3.Distance(relativePosition2, relativePosition3) < 12f)
        // {
        //     AddReward(0.0000005f);
        // }

        CheckVisibleTargetDistanceReward();
        Vector2Int currentCell = new Vector2Int(
    Mathf.FloorToInt(transform.position.x / cellSize),
    Mathf.FloorToInt(transform.position.z / cellSize)
);

        float currentDistance = Vector3.Distance(relativePosition2, relativePosition);
        float delta = previousDistanceToSurvivor - currentDistance;

        // AddReward(delta * 0.01f);  // Reward progress
        previousDistanceToSurvivor = currentDistance;

        if (visitedPositions.Add(currentCell))
        {
            AddReward(0.1f); 
        }
        

        dropTimer += Time.deltaTime;
        if (dropTimer >= dropInterval)
        {
            dropTimer = 0f;


            Vector3 localPos = referenceTransform.InverseTransformPoint(transform.position);

            if (Vector3.Distance(localPos, lastDropPoint) < idleRadius)
            {
                AddReward(-0.3f); 
            }


            lastDropPoint = localPos;
        }




        if (Vector3.Distance(relativePosition2, relativePosition) < 4.5f)
        {
            arm.enabled = true;
            SetReward(+30f);
            rb.isKinematic = true;
            lastPos = transform.position;
            lastquat = transform.rotation;
            isStuck = true;
            // EndEpisode();
        }

        // AddReward(-0.000075f);

        if (isStuck)
        {
            transform.position = lastPos;
            transform.rotation = lastquat;

        }

    }
    GameObject GetVisibleTarget()
{
    Vector3 origin = transform.position;
    float rayLength = 40f;

    Vector3[] rayDirections = new Vector3[] {
        transform.forward,
        Quaternion.Euler(0, -25, 0) * transform.forward,
        Quaternion.Euler(0, -20, 0) * transform.forward,
        Quaternion.Euler(0, -15, 0) * transform.forward,
        Quaternion.Euler(0, -10, 0) * transform.forward,
        Quaternion.Euler(0, -5, 0) * transform.forward,
        Quaternion.Euler(0, 0, 0) * transform.forward,
        Quaternion.Euler(0, 5, 0) * transform.forward,
        Quaternion.Euler(0, 10, 0) * transform.forward,
        Quaternion.Euler(0, 15, 0) * transform.forward,
        Quaternion.Euler(0, 20, 0) * transform.forward,
        Quaternion.Euler(0, 25, 0) * transform.forward,



    };

    foreach (var dir in rayDirections)
    {
        Debug.DrawRay(origin, dir * rayLength, new Color(0.5f, 0, 0.5f));

        if (Physics.Raycast(origin, dir, out RaycastHit hit, rayLength))
        {
            if (hit.collider.CompareTag("survivor"))
            {
                return hit.collider.gameObject;
            }
        }
    }

    return null;
}


    void CheckVisibleTargetDistanceReward()
{
    GameObject visibleTarget = GetVisibleTarget(); 
    if (visibleTarget != null)
    {
        float currentDistance = Vector3.Distance(transform.position, visibleTarget.transform.position);

        if (previousDistance.HasValue)
        {
            float delta = previousDistance.Value - currentDistance;
            AddReward(delta * 0.05f); 
        }

        previousDistance = currentDistance;
    }
    else
    {
        previousDistance = null;
    }
}


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        int xc = (int)Mathf.Round(Input.GetAxisRaw("Horizontal"));
        int zc = (int)Mathf.Round(Input.GetAxisRaw("Vertical"));
        switch (zc)
        {
            case 0: discreteActions[0] = 0; break;
            case -1: discreteActions[0] = 1; break;
            case 1: discreteActions[0] = 2; break;
        }
        switch (xc)
        {
            case 0: discreteActions[1] = 0; break;
            case -1: discreteActions[1] = 1; break;
            case 1: discreteActions[1] = 2; break;
        }

    }

    public void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "obstacle")
        {
            AddReward(-0.2f);

            EndEpisode();
        } 
     }


    
}