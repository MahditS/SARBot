using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UI;

public class Gripper : MonoBehaviour
{
    public ArmAgent arm;
    public Quaternion armJoint2Quat;
    public Quaternion armJoint1Quat;
    public Camera camera;
    public GameObject target;
    public Quaternion rotation;
    public Transform car;
    public GameObject returnCar;
    public Transform armT;

    public Transform referenceTransform;
    public bool stuck = false;

    

    // Start is called before the first frame update
    void Awake()
    {
        armJoint1Quat = arm.armJoint1.rotation;
        armJoint2Quat = arm.armJoint2.rotation;
       float x = UnityEngine.Random.Range(-1.2f, 5f);
        float z = UnityEngine.Random.Range(-1f, -5.5f);
        Vector3 offset = new Vector3(x, -1.299f, z);

        // Instantiate(arm.dummy, arm.referenceTransform.TransformPoint(offset), Quaternion.identity, arm.referenceTransform);
    }

    // Update is called once per frame
    void Update()
    {

        if (stuck)
        {
            Transform survivor = FindParentWithTag(target.transform, "survivorParent");
            survivor.parent = transform;
            survivor.localPosition = new Vector3(0,0,0);
            survivor.position = new Vector3(survivor.position.x, transform.position.y, survivor.position.z);
            Debug.Log("Parented");

        }

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

    void ResetRagdollVelocity(GameObject ragdollRoot)
    {
        Rigidbody[] bodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in bodies)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

  public void DisableAllChildColliders(GameObject parent)
    {
        Collider[] colliders = parent.GetComponentsInChildren<Collider>(includeInactive: true);

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }



    void OnTriggerEnter(Collider collider)
{
        if (collider.gameObject.CompareTag("survivor"))
        {
            Transform root = FindParentWithTag(collider.transform, "survivorParent");


            // Debug.Log($"Gripper touched: {collider.gameObject.name}, root: {root.name}");

            // if (root != null)
            // {
            //     Destroy(root.gameObject); 
            // }

            arm.SetReward(+10f);

            float x = UnityEngine.Random.Range(-1.35f, 4f);
            float z = UnityEngine.Random.Range(-5.75f, -1.38f);
            Vector3 off = new Vector3(x, -1.299f, z);
            root.position = referenceTransform.TransformPoint(off);

            ResetRagdollVelocity(root.gameObject);
            arm.enabled = false;
            stuck = true;
            target = collider.gameObject;
            returnCar.SetActive(true);
            returnCar.transform.position = car.transform.position;
            returnCar.transform.rotation = rotation;
            armT.parent = returnCar.transform;
            car.gameObject.SetActive(false);
            DisableAllChildColliders(armT.gameObject);
            DisableAllChildColliders(root.gameObject);
            camera.player = returnCar.transform;
        }
}
}
