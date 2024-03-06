using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{

    [SerializeField] private RiverHingeMove rhm;
    public bool acquired = false;

    public Endless endless;

    private Obstacle obstacleScript;
    private Obstacles obstaclesScript;

    void Start(){
        obstacleScript = gameObject.GetComponent<Obstacle>();
        rhm = FindObjectOfType<RiverHingeMove>();
    }
    
    void OnTriggerEnter(){
        //Particle effect
        //Add to blob
        if(!acquired){
            acquired = true;
            obstacleScript.Score();
            GameObject newFlower = Instantiate(this.gameObject);
            newFlower.GetComponent<Obstacle>().disable = true;
            Debug.Log("Instantiated "+newFlower.name);
            rhm.AddFlower(newFlower);

            //Turn off the mesh renderer
             for (int i = 0; i < transform.childCount && i < 2; i++)
            {
                // Get the MeshRenderer component of the child
                MeshRenderer meshRenderer = transform.GetChild(i).GetComponent<MeshRenderer>();

                // Check if the MeshRenderer component exists
                if (meshRenderer != null)
                {
                    // Disable the MeshRenderer
                    meshRenderer.enabled = false;
                }
                else
                {
                    Debug.LogWarning("MeshRenderer component not found on child " + i);
                }
            }
        
    }
}
}
