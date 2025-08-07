using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UI;
public class ArmAgent : Agent{
    
    [SerializeField] public GameObject target;
    [SerializeField] public Gripper grip;

    [SerializeField] public GameObject dummy;

    public Vector3 offset;

    [SerializeField] public Transform armJoint1;

    [SerializeField] public Transform armJoint2;

    [SerializeField] public Transform referenceTransform;
    [SerializeField] public Transform gripper;
    [SerializeField] public GameObject car;
    

  public Vector3 NormalizeAngles(Vector3 angles)
    {
        return new Vector3(
            Mathf.DeltaAngle(0, angles.x) / 180f,
            Mathf.DeltaAngle(0, angles.y) / 180f,
            Mathf.DeltaAngle(0, angles.z) / 180f
        );
    }
    

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
        armJoint1.rotation = grip.armJoint1Quat;
        armJoint2.rotation = grip.armJoint2Quat;
        GameObject closest = FindClosestWithTag("survivorMain");
        target = closest;
        
        Vector3 trainedBasePos = new Vector3(2.01f, -1.299f, -3.62f);
        Vector3 currentBasePos = car.transform.localPosition;
        offset = trainedBasePos - currentBasePos;


        if (target)
        {
            GameObject G = FindClosestWithTag("survivorParent");

            float x = UnityEngine.Random.Range(-1.85f, 4.5f);
            float z = UnityEngine.Random.Range(-6.25f, -1.38f + 0.5f);
            Vector3 off = new Vector3(x, -1.299f, z);
            // G.transform.position = referenceTransform.TransformPoint(off);
        }

        // car.transform.localPosition = new Vector3(2.01f, -1.29f, -3.62f);




    }



    public override void CollectObservations(VectorSensor sensor)
    {

        GameObject closest = FindClosestWithTag("survivorMain");
        target = closest;

        //ANGLES

        Vector3 joint1Angles = armJoint1.localEulerAngles;
        Vector3 joint2Angles = armJoint2.localEulerAngles;
        sensor.AddObservation(NormalizeAngles(joint1Angles));
        sensor.AddObservation(NormalizeAngles(joint2Angles));

        //OFFSET POSES

        Vector3 shiftedTargetWorldPos = target.transform.position;
        Vector3 shiftedGripperWorldPos = gripper.position; 
        Vector3 shiftedAgentWorldPos = transform.position;

        Vector3 relativeTargetPos = referenceTransform.InverseTransformPoint(shiftedTargetWorldPos);
        Vector3 relativeGripperPos = referenceTransform.InverseTransformPoint(shiftedGripperWorldPos);
        Vector3 relativeAgentPos = referenceTransform.InverseTransformPoint(shiftedAgentWorldPos);


        //GRIPPER & TARGET POSES
        sensor.AddObservation(relativeTargetPos + offset);
        sensor.AddObservation(relativeGripperPos + offset);


        //GRIPPER TO TARGET REWARDS
        float distanceToTarget = Vector3.Distance(relativeGripperPos, relativeTargetPos);
        AddReward(-distanceToTarget * 0.01f);

        if (relativeGripperPos.y < relativeAgentPos.y) 
        {
            AddReward(-0.05f); // Tune this value as needed
        }

        //GRIPPER TO SELF REWARDS
        // float armExtension = Vector3.Distance(relativePosGripper, relativePosSelf);
        // AddReward(armExtension * 0.01f); 
        // Debug.Log(distanceToTarget);
    }
    
 [SerializeField] public Transform rotationReference; 
 
public override void OnActionReceived(ActionBuffers actions)
    {
        int x = actions.DiscreteActions[0];
        int y = actions.DiscreteActions[1];
        int z = actions.DiscreteActions[2];

        float rotationSpeed = 35f;
        float xDir = GetDirectionFromAction(x);
        float yDir = GetDirectionFromAction(y);
        float zDir = GetDirectionFromAction(z);

        Vector3 stableRight = rotationReference.up;
        Vector3 stableUp = rotationReference.forward;

        armJoint1.Rotate(armJoint1.forward * xDir * rotationSpeed * Time.deltaTime, Space.World);
        armJoint1.Rotate(stableRight * yDir * rotationSpeed * Time.deltaTime, Space.World);

        armJoint2.Rotate(armJoint2.forward * zDir * rotationSpeed * Time.deltaTime, Space.World);
    }


    public float GetDirectionFromAction(int action)
    {
        switch (action)
        {
            case 1: return -1f;
            case 2: return 1f;
            default: return 0f;
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        int xc = (int)Mathf.Round(Input.GetAxisRaw("Horizontal"));
        int zc = (int)Mathf.Round(Input.GetAxisRaw("Vertical"));
        int yc = 0;
        if (Input.GetKey("e"))
        {
            yc = -1;
        }
        else if (Input.GetKey("r"))
        {
            yc = 1;
        }
        else if (!Input.GetKey("e") && !Input.GetKey("r"))
        {
            yc = 0;
        }

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
        switch(yc)
        {
            case 0: discreteActions[2] = 0; break;
            case -1: discreteActions[2] = 1; break;
            case 1: discreteActions[2] = 2; break;
        }

    }


}