using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Starting : MonoBehaviour
{
    public Transform fire1;
    public Transform fire2;
    public Transform fire3;
    public Transform survivor;
    public Camera cam;
    public Move mov;
    public Rigidbody rb;

    public float moveSpeed = 10f;

    public int selectedFire = 1;

    // Start is called before the first frame update
    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKeyDown("1"))
        {
            selectedFire = 1;
        }
        else if (Input.GetKeyDown("2"))
        {
            selectedFire = 2;
        }
        else if (Input.GetKeyDown("3"))
        {
            selectedFire = 3;
        }
        else if (Input.GetKeyDown("4"))
        {
            selectedFire = 4;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1f;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveZ = 1f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            moveZ = -1f;
        }

        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized;
        if (selectedFire == 1)
        {
            fire1.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        }
        else if (selectedFire == 2)
        {
            fire2.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        }
        else if (selectedFire == 3)
        {
            fire3.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        }
        else if (selectedFire == 4)
        {
            survivor.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            rb.isKinematic = false;
            cam.enabled = true;
            mov.enabled = true;
        }
    }
}
