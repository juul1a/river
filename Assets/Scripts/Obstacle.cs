using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Obstacle : MonoBehaviour
{
   [SerializeField] private GameObject floatingTextPrefab;
   private Obstacles obstaclesScript;
   public Endless endlessScript;
   public bool bigObstacle;
   public bool endGame = false;
   public float littleMoreSpeed = 0f;
   public bool disable;

   public Color plusTextColor, minusTextColor;

   public int score;
   
   void Start(){
    endlessScript = FindObjectOfType<Endless>();
    obstaclesScript = FindObjectOfType<Obstacles>();
   }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameObject.activeSelf && endlessScript.gameStart && !disable)
        {
            Vector3 movement = -1*transform.forward * (endlessScript.speed +littleMoreSpeed) * Time.fixedDeltaTime;
                transform.position += movement;
                if(transform.position.z < endlessScript.minZ){
                    obstaclesScript.PoolObstacle(gameObject);
                    obstaclesScript.PlaceObstacleFromPool();
                }
            }
    }

    public void Score(){
        endlessScript.AddScore(score);
        if(floatingTextPrefab != null){
            GameObject go = Instantiate(floatingTextPrefab);
            go.transform.position = new Vector3(transform.position.x, go.transform.position.y, transform.position.z);
            go.GetComponent<TMP_Text>().text = score.ToString();
            if(score<0){
                go.GetComponent<FloatingText>().SetTextColour(minusTextColor);
            }
            else if(score>0){
                go.GetComponent<FloatingText>().SetTextColour(plusTextColor);
            }
            
        }
        //floating text
    }
}
