using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{

    [SerializeField] private RiverHingeMove rhm;
    public bool acquired = false;

    public Endless endless;

    private Obstacle obstacleScript;

    void Start(){
        obstacleScript = gameObject.GetComponent<Obstacle>();
        rhm = FindObjectOfType<RiverHingeMove>();
    }
    
    void OnTriggerEnter(){
        //Particle effect
        //Add to blob
        if(!acquired){
            obstacleScript.Score();
            rhm.AddFlower(gameObject);
            acquired = true;
            obstacleScript.disable = true;
        }
        //Score increase
        
    }
}
