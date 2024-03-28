using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverHingeMove : MonoBehaviour
{
    public float speed = 5f;
    public float forwardSpeed = 5f;
    public float rotationSpeed = 2f;

    // Left and right
    public float minX, maxX;
    //Down and Up
    public float minY, maxY;

    private Vector3 input;

    [SerializeField] private GameObject riverWhole;

    public bool mobile;

    public float minFlowerDistance = 0.1f; // Minimum distance from bones.
    public float maxFlowerDistance = 0.3f; // Maximum distance from bones.

    private List<Transform> availableBones = new List<Transform>();
    private int currentBoneIndex = 0;

    

    void Awake(){
        input = new Vector3(0,0,0);
       
        InitializeAvailableBones();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(mobile){
            GatherMobileInput();
        }
        else{
            GatherPCInput();
        }
        Move();
    }

    private void Move(){
        

        //Move side to side based on input
        // Get input from the 'a' and 'd' keys
        Vector3 movementRiverHead = new Vector3(0,0,0);

        // Debug.Log("Vertical input = "+verticalInput);

        if(input.x <0 && transform.position.x >= minX){
            //We are allowed to move left
            movementRiverHead += new Vector3(0, 0, input.x) * speed * 2 * Time.deltaTime;
        }
        if(input.x >0 && transform.position.x <= maxX){
            //We are allowed to move right
            movementRiverHead += new Vector3(0, 0, input.x) * speed * 2 * Time.deltaTime;
        }
        // if(input.y >0 && transform.position.y <= maxY){
        //     //we ar allowed to move down
        //     movementRiverHead += new Vector3(0, input.y, 0) * speed * 2 * Time.deltaTime;
        // }
        // if(input.y <0 && transform.position.y >= minY){
        //     movementRiverHead += new Vector3(0, input.y, 0) * speed * 2 * Time.deltaTime;
        // }

        // Move the player's transform
        transform.Translate(movementRiverHead);

        // // riverHead.Rotate(Vector3.left, horizontalInput * speed *10* Time.deltaTime);
        // if(verticalInput >0 && riverWhole.transform.position.y <= maxY){
        //     riverWhole.transform.Rotate(new Vector3(1,0,0), verticalInput * rotationSpeed* Time.deltaTime);
        // }
        // if(verticalInput <0 && riverWhole.transform.position.y >= minY){
        //     riverWhole.transform.Rotate(new Vector3(1,0,0), verticalInput * rotationSpeed* Time.deltaTime);
        // }
        // if(verticalInput==0){
        //     riverWhole.transform.rotation = Quaternion.identity;
        // }
        // riverWhole.transform.position += movement;
    
    }

    private void GatherMobileInput(){
        Vector3 tilt = Input.acceleration;
        tilt = Quaternion.Euler(90,90,0) * tilt;
        input = new Vector3(-1*tilt.z, 0, 0);
    }
     private void GatherMobileInputGyro(){
        Gyroscope tilt = Input.gyro;
        Debug.Log("Gyro tilt = "+tilt.rotationRate);
        // tilt = Quaternion.Euler(90,90,0) * tilt;
        input = new Vector3(-1*tilt.rotationRate.y, 0, 0);
    }

    private void GatherPCInput(){
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        input.x = horizontalInput;
        input.y = verticalInput;
    }

    public void AddFlower(GameObject flower)
    {
        if (availableBones.Count == 0)
        {
            // All bones have been dequeued, so re-queue and shuffle.
            InitializeAvailableBones();
            currentBoneIndex = 0;
        }


        // Choose a random available bone.
        Transform selectedBone = availableBones[currentBoneIndex % availableBones.Count];

        // Randomize the position within the specified range.
        Vector3 randomOffset = Random.Range(minFlowerDistance, maxFlowerDistance) * Random.onUnitSphere;

        // Set the flower's position relative to the chosen bone.
        flower.transform.SetParent(selectedBone);
        flower.transform.localPosition = randomOffset;
        flower.transform.localRotation = Quaternion.identity;
        Vector3 scale = new Vector3(flower.transform.localScale.x * 0.5f, flower.transform.localScale.y * 0.5f, flower.transform.localScale.z * 0.5f);
        flower.transform.localScale = scale;
        flower.transform.localPosition += Vector3.up * 0.01f;

        // Increment the index for the next selection.
        currentBoneIndex++;
    }

    private void ShuffleBones()
    {
        // Fisher-Yates shuffle algorithm for shuffling the list of bones.
        int n = availableBones.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            Transform value = availableBones[k];
            availableBones[k] = availableBones[n];
            availableBones[n] = value;
        }
    }

       private void InitializeAvailableBones()
    {
        Transform armature = riverWhole.transform.Find("Armature");
        availableBones.AddRange(armature.GetComponentsInChildren<Transform>());
        availableBones.Remove(armature);
        availableBones.RemoveAt(0); //Remove first bone
        availableBones.RemoveAt(1); //Remove 2nd bone
        availableBones.RemoveAt(availableBones.Count-1); //Remove last bone
        availableBones.RemoveAt(availableBones.Count-2); //Remove 2nd last bone
        ShuffleBones();
    }
}
