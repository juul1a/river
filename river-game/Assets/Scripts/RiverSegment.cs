using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverSegment : MonoBehaviour
{
    public Transform prevSegment;
    public int speed = 5;

    // Update is called once per frame
    void FixedUpdate()
    {
        AdjustXY();
    }
    private void AdjustXY(){
        if(prevSegment != null){
            float targetX = prevSegment.position.x;
            float targetY = prevSegment.position.y;

            // Calculate the new X position based on interpolation
            float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * speed);

            float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * speed);

            // Update the position with the new X value while keeping Y and Z the same
            transform.position = new Vector3(newX, newY, transform.position.z);
        }
    }
}
