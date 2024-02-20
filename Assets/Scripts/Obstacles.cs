using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    
//OBSTACLES
    [SerializeField] private List<GameObject> obstaclePool = new List<GameObject>();
    [SerializeField] private List<GameObject> activeObstacles = new List<GameObject>();
    [SerializeField] int maxObstaclesinPool = 50;
    [SerializeField] int obstaclesToStart = 20;
    [SerializeField] int distBetweenObstacles = 30;
    GameObject prevObstacle;
    [SerializeField] float startingPosZ = 30f;
    [SerializeField] float left, right, center;
    [SerializeField] private List<GameObject> obstaclePrefabs;
    public bool gameStart = false;
    public bool obstaclesSpawned = false;


    //Make one list of all scenes
    //One separate list of inactive scenes
    public void GameStart(){
        if(!obstaclesSpawned){
            Debug.Log("Spawning Obstacles");
            SpawnObstacles();
        }
        PlaceObstacles();
        obstaclesSpawned = true;
    }

    public void GameEnd(){
        RemoveObstacles();
        obstaclesSpawned = false;
        prevObstacle = null;
    }

    void PlaceObstacles(){
        for(int i = 1; i < obstaclesToStart; i++){
            PlaceObstacleFromPool();
        }
    }

    void RemoveObstacles(){
        foreach(GameObject obstacle in activeObstacles){
            obstacle.SetActive(false);
            obstaclePool.Add(obstacle);
        }
        activeObstacles = new List<GameObject>();
    }

    public void PlaceObstacleFromPool(){
        Debug.Log("placing obstacle");
        float z = 0;
        if(prevObstacle == null){
            //this is the first obstacle
            z = startingPosZ;
        }
        else{
            z = prevObstacle.transform.position.z + distBetweenObstacles;
        }
        int pickX = Random.Range(0,3);
        float x = 0;
        switch(pickX){
            case 0:
                x = left;
                break;
            case 1:
                x = center;
                break;
            case 2:
                x = right;
                break;
            default:
                break;
        }
        int pickFromPool = Random.Range(0,obstaclePool.Count);
        GameObject pooledToSpawn = obstaclePool[pickFromPool];
        // Debug.Log("y of "+pooledToSpawn.name+" is "+transform.position.y);
        pooledToSpawn.transform.position = new Vector3(x,transform.position.y,z);
        pooledToSpawn.SetActive(true);
        obstaclePool.Remove(pooledToSpawn);
        activeObstacles.Add(pooledToSpawn);
        prevObstacle = pooledToSpawn;
        Obstacle thisObstacle = pooledToSpawn.GetComponent<Obstacle>();
    
        int twoObstacles = Random.Range(0,3);
        //1 in 3 chance of 2 obstacles
        if(twoObstacles == 1 && !thisObstacle.bigObstacle){
            int pickNextFromPool = Random.Range(0,obstaclePool.Count);
            //Pick a different x
            switch(pickX){
                case 0:
                //x was already set to left for the last object so lets make it centre or right
                    if(Random.Range(0,2) == 0){
                        x = center;
                    }
                    else{
                        x = right;
                    }
                    break;
                case 1:
                //x was already set to centre for the last object so lets make it left or right
                    if(Random.Range(0,2) == 0){
                        x = left;
                    }
                    else{
                        x = right;
                    }
                    break;
                case 2:
                //x was already set to right for the last object so lets make it left or centre
                    if(Random.Range(0,2) == 0){
                        x = center;
                    }
                    else{
                        x = left;
                    }
                    break;
                default:
                    break;
            }
            GameObject nextPooledToSpawn = obstaclePool[pickNextFromPool];
            //2nd object cant be big either
            while(nextPooledToSpawn.GetComponent<Obstacle>().bigObstacle){
                pickNextFromPool = Random.Range(0,obstaclePool.Count);
                nextPooledToSpawn = obstaclePool[pickNextFromPool];
            }
            // Debug.Log("y of next pooled to spawn "+pooledToSpawn.name+" is "+transform.position.y);
            nextPooledToSpawn.transform.position = new Vector3(x,transform.position.y,z);
            obstaclePool.Remove(nextPooledToSpawn);
            activeObstacles.Add(nextPooledToSpawn);
            nextPooledToSpawn.SetActive(true);
            prevObstacle = nextPooledToSpawn;
        }
    }
    public void PoolObstacle(GameObject obstacle){
        obstacle.SetActive(false);
        activeObstacles.Remove(obstacle);
        obstaclePool.Add(obstacle);
    }

    void SpawnObstacles(){
        //spawn all obstacles
        // Debug.Log("Spawning obstacles");
        for(int i = 0; i < maxObstaclesinPool; i++){
            int index = Random.Range(0,obstaclePrefabs.Count);
            GameObject newObstacle = Instantiate(obstaclePrefabs[index]);
            // Debug.Log("Just Spawned "+newObstacle);
            newObstacle.SetActive(false);
            obstaclePool.Add(newObstacle);
        }
        obstaclesSpawned = true;
    }
}
