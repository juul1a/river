using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endless : MonoBehaviour
{
    [SerializeField] private List<GameObject> scenes;
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] public float speed = 10f;
    [SerializeField] private float speedIncreaseRate = 0.01f; 
    [SerializeField] public float minZ = -125f;

    public float score = 0.0f;
    public float highScore = 0.0f;

    public bool gameStart = false;

    [SerializeField] private List<GameObject> inactiveScenes;

    [SerializeField] private Transform spawnLocation;

    public void GameStart(){
        gameStart = true;
        speed = 10f;
    }

    void FixedUpdate(){
        //Move forward at speed, all the time
        if(gameStart){
            if(speed < maxSpeed){
                speed = Mathf.Lerp(speed, maxSpeed, speedIncreaseRate * Time.deltaTime);
            }
            Vector3 movement = -1*transform.forward * speed  * Time.fixedDeltaTime;
            score += -1*movement.z / 2;
            UIManager.Instance.UpdateScore(Mathf.RoundToInt(score));
            foreach(GameObject scene in scenes){
                if(scene.activeSelf){ 
                    scene.transform.position += movement;
                    if(scene.transform.position.z < minZ){
                        ShiftScene(scene);
                    }
                }
            }
        }
    }

    void ShiftScene(GameObject scene){
        
        int newSceneIndex = Random.Range(0, inactiveScenes.Count);
        GameObject newScene = inactiveScenes[newSceneIndex]; //pick random scene from list of scenes and set it to active
        newScene.SetActive(true);
        newScene.transform.position = new Vector3(newScene.transform.position.x, newScene.transform.position.y, spawnLocation.position.z);
        // scenes.Add(newScene);
        inactiveScenes.Remove(newScene);
        spawnLocation = newScene.transform.Find("Place").transform;

        // scenes.Remove(scene);
        inactiveScenes.Add(scene);
        scene.SetActive(false);
        
    }

    public void ReduceSpeed(float amount){
        if(speed-amount >= minSpeed){
            speed -= amount;
        }
        else{
            speed = minSpeed;
        }
    }

    public void GameEnd(){
        speed = 0;
        gameStart = false;
        if(score>highScore){
            highScore = score;
            UIManager.Instance.UpdateHighScore(Mathf.RoundToInt(highScore));
        }
        score = 0;
        UIManager.Instance.UpdateScore(Mathf.RoundToInt(score));
    }

    public void AddScore(int inScore){
        score += inScore;
    }
}
