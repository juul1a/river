using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlpool : MonoBehaviour
{
    public float rotationSpeed = 30f; // Adjust the rotation speed as needed

    // Update is called once per frame
    void Update()
    {
        // Rotate the object around the X-axis (pitch) continuously
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
