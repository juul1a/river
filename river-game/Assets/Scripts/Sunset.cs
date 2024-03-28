using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sunset : MonoBehaviour
{
    [System.Serializable]
    public class Tintables
    {
        public GameObject tintObj;
        public Color[] gradient;
    }
    public Tintables[] tintObjects;

    [System.Serializable]
    public class SkyboxTinter
    {
        public Skybox skybox;
        public Color[] gradient;
    }
    public SkyboxTinter skyboxTinter;

    public float sunsetDurationSeconds;

    private Gradient[] myCoolGradients;
    private GradientColorKey[][] colorKeys;
    private GradientAlphaKey[][] alphaKeys;
    private float startTime;
    public float rightNow;
    // private StateManager stateManager;

    private string sunsetState;



    // Start is called before the first frame update
    void Start()
    {
        sunsetState = "increasing";
        // stateManager = GameObject.Find ("StateManager").GetComponent<StateManager>();

        myCoolGradients = new Gradient[tintObjects.Length+1]; //# of tintObjectss + skybox
        colorKeys = new GradientColorKey[tintObjects.Length+1][];
        alphaKeys = new GradientAlphaKey[tintObjects.Length+1][];
        

        //skybox will be first
        myCoolGradients[0] = new Gradient();
        colorKeys[0] = new GradientColorKey[skyboxTinter.gradient.Length];
        alphaKeys[0] = new GradientAlphaKey[skyboxTinter.gradient.Length];

        for(int i = 0; i < skyboxTinter.gradient.Length; i++){
            colorKeys[0][i].color = skyboxTinter.gradient[i];
            colorKeys[0][i].time = (float)i/(float)(skyboxTinter.gradient.Length-1);
            // Debug.Log("Time for skybox color key "+i+" is "+colorKeys[0][i].time);
            alphaKeys[0][i].alpha = 0.5f;
            alphaKeys[0][i].time = (float)i/(float)(skyboxTinter.gradient.Length-1);
        }
        myCoolGradients[0].SetKeys(colorKeys[0],alphaKeys[0]);

        for(int i = 1; i < tintObjects.Length+1; i++){
            myCoolGradients[i] = new Gradient();
            colorKeys[i] = new GradientColorKey[tintObjects[i-1].gradient.Length];
            alphaKeys[i] = new GradientAlphaKey[tintObjects[i-1].gradient.Length];
            for(int j = 0; j < tintObjects[i-1].gradient.Length; j++){
                colorKeys[i][j].color = tintObjects[i-1].gradient[j];
                colorKeys[i][j].time = (float)j/(float)(tintObjects[i-1].gradient.Length-1);
                // Debug.Log("Time for tintObjects "+(i-1)+" color key "+j+" is "+colorKeys[i][j].time);
                alphaKeys[i][j].alpha = tintObjects[i-1].gradient[j].a;
                alphaKeys[i][j].time = (float)j/(float)(tintObjects[i-1].gradient.Length-1);
            }
            myCoolGradients[i].SetKeys(colorKeys[i],alphaKeys[i]);
        }
        
        
        startTime = 0f;
        rightNow = startTime;
    }

    // Update is called once per frame
    void Update()
    {    
        // if(stateManager.IsPlaying()){
            if(sunsetState == "increasing"){
                rightNow += Time.deltaTime;
                if(rightNow >= sunsetDurationSeconds){
                    sunsetState = "decreasing";
                }
            }
            else if(sunsetState == "decreasing"){
                rightNow -= Time.deltaTime;
                if(rightNow <= 0){
                    sunsetState = "increasing";
                }   
            }
            float gradNow = rightNow / sunsetDurationSeconds;

            //skybox
            if(skyboxTinter.skybox){
                Color skyColor = myCoolGradients[0].Evaluate(gradNow);
                Skybox mySky = skyboxTinter.skybox;
                Material theSkyBox = mySky.material;
                theSkyBox.SetColor("_Tint", skyColor);
            }

            Color fadeColor;
            for(int i = 1; i < myCoolGradients.Length; ++i){
                fadeColor = myCoolGradients[i].Evaluate(gradNow);
                SpriteRenderer tintSpriteRend = tintObjects[i-1].tintObj.GetComponent<SpriteRenderer>();
                tintSpriteRend.color = fadeColor;
            }

        // }
        //sunsetDuration = Max
        //StartTime = Min
        //rightNow = startTime + time.DeltaTime();
    }
}
