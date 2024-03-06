using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public CinemachineVirtualCamera playcam;
    public CinemachineVirtualCamera startcam;

    private Endless endless;
    private Obstacles obstacles;

    [SerializeField] private GameObject Menu;
    [SerializeField] private GameObject HUD;

    [SerializeField] private TMP_Text uiScore, uiHighScore, smHighScore;

    private static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            // If the instance hasn't been set yet, find it in the scene
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();

                // If no instance exists in the scene, create a new one
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(UIManager).Name);
                    instance = singletonObject.AddComponent<UIManager>();
                }
            }
            return instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        endless = GameObject.Find("Endlessness").GetComponent<Endless>();
        obstacles = GameObject.Find("Obstacles").GetComponent<Obstacles>();

    }

    // Update is called once per frame
    public void StartGame()
    {
        startcam.gameObject.SetActive(false);
        endless.GameStart();
        obstacles.GameStart();
        Menu.SetActive(false);
        HUD.SetActive(true);
    }

    public void EndGame(){
        startcam.gameObject.SetActive(true);
        endless.GameEnd();
        obstacles.GameEnd();
        Menu.SetActive(true);
        HUD.SetActive(false);
        //end endless
        //switch camera
        //change scene colours
    }

    public void UpdateScore(int score){
        uiScore.text = score.ToString();
    }

    public void UpdateHighScore(int score){
        // if(uiHighScore.gameObject.activeSelf){
            // Debug.Log("turning on highscore");
            uiHighScore.gameObject.SetActive(true);
        // }
        //  if(smHighScore.gameObject.activeSelf){
        //     Debug.Log("turning on start menu highscore");
            smHighScore.gameObject.SetActive(true);
        // }
        uiHighScore.text = score.ToString();
        smHighScore.text = score.ToString();
    }

    public void Quit(){
         Application.Quit();
    }
}
