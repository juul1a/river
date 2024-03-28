using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FlatKit;

public class EnviroShift : MonoBehaviour
{
    //sphere 2 we fade in and out
    //if we are in sphere 1 then set sphere 2 and fade sphere 2 in
    //if we are in sphere 2 then set sphere 1 and fade sphere 2 out

    //fade between split toning shadow colour
    public GameObject sphere1;
    public GameObject sphere2;
    public GameObject activeSphere;
    public FogSettings fogSettings;
    public Light directionalLight;
    public Volume gv;
    public SplitToning splitToning;
    public Gradient newGrad;

    // public Gradient splitToningGradient;
    public Gradient[] lightingGradients;

    public float currentLightIntensity;
    public float currentFogIntensity;
    public float skyBowlTransition;

    public float sphereLerpFrom, sphereLerpTo, currentSphereAlpha;

    public EnviroSO[] allEnviros;
    public EnviroSO oldEnviro;
    public EnviroSO newEnviro;

    public float transitionDurationSeconds;
    public float stayDurationSeconds;
    public float rightNow;
    public string sunsetState = "stay";

    private int indexSO = 0;

    //Stay ni enviro
    //switching to next enviro

    private Gradient SetGradient(Color colourOld, Color colourNew){
        // Debug.Log("Setting gradient");
        Gradient gradient = new Gradient();
        GradientColorKey[] colourKey = new GradientColorKey[2];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        colourKey[0].color = colourOld;
        colourKey[1].color = colourNew;
        colourKey[0].time = 0;
        colourKey[1].time = 1;
        
        alphaKey[0].alpha = colourOld.a;
        alphaKey[1].alpha = colourNew.a;
        alphaKey[0].time = 0;
        alphaKey[1].time = 1;
        
        gradient.SetKeys(colourKey, alphaKey);
        // Debug.Log("Setting gradient to "+gradient);
        return gradient;
    }

    

    void Start(){
        sunsetState = "stay";
        rightNow = 0;
        newEnviro = allEnviros[indexSO];
        SetNow();
    }

    Gradient LerpGradients(Gradient gradient1, Gradient gradient2, float t)
    {
        Gradient resultGradient = new Gradient();
        resultGradient.mode = gradient1.mode;

        // Assuming both gradients have the same number of keys
        int keyCount = gradient1.colorKeys.Length;

        GradientColorKey[] colorKeys = new GradientColorKey[keyCount];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[keyCount];

        for (int i = 0; i < keyCount; i++)
        {
            // Lerp between the color keys and alpha keys
            // Debug.Log("Lerping color "+gradient1.colorKeys[i].color+" to "+gradient2.colorKeys[i].color);
            colorKeys[i].color = Color.Lerp(gradient1.colorKeys[i].color, gradient2.colorKeys[i].color, t);
            colorKeys[i].time = gradient1.colorKeys[i].time;

            // Debug.Log("Lerping alpha "+gradient1.alphaKeys[i].alpha+" to "+gradient2.alphaKeys[i].alpha);
            alphaKeys[i].alpha = Mathf.Lerp(gradient1.alphaKeys[i].alpha, gradient2.alphaKeys[i].alpha, t);
            alphaKeys[i].time = gradient1.alphaKeys[i].time;
        }

        resultGradient.SetKeys(colorKeys, alphaKeys);

        return resultGradient;
    }

    void Update(){
        rightNow += Time.deltaTime;
        //Just finished "transition" period, trigger the stay
        if(sunsetState == "transition" && rightNow >= transitionDurationSeconds){ 
             if(activeSphere == sphere1){
                activeSphere = sphere2;
             }
             else if(activeSphere == sphere2){
                activeSphere = sphere1;
             }
            rightNow = 0;
            sunsetState = "stay";
        }
        //Just finished "stay" period, trigger transition
        else if(sunsetState == "stay" && rightNow >= stayDurationSeconds){
            rightNow = 0;
            sunsetState = "transition";
            indexSO++;
            if(indexSO == allEnviros.Length){
                indexSO = 0;
            }
            oldEnviro = newEnviro;
            newEnviro = allEnviros[indexSO];
            // splitToningGradient = new Gradient();
            // splitToningGradient = SetGradient(oldEnviro.splitToningShadow, newEnviro.splitToningShadow);
            lightingGradients = new Gradient[newEnviro.lightingGradient.Length];
            for(int i = 0; i < lightingGradients.Length; i++){
                lightingGradients[i] = SetGradient(oldEnviro.lightingGradient[i], newEnviro.lightingGradient[i]);
            }
            if(activeSphere == sphere1){
                //set new material alpha to 0
                Color newMatColor = newEnviro.skyboxMaterial.GetColor("_Color");
                newMatColor.a = 0;
                newEnviro.skyboxMaterial.SetColor("_Color", newMatColor);

                sphere2.GetComponent<Renderer>().material = newEnviro.skyboxMaterial;

                sphereLerpFrom = 0.0f;
                sphereLerpTo = 1.0f;
                
                // //Reset old material alpha to 1
                // Color oldMatColor = oldmat.GetColor("_Color");
                // oldMatColor.a = 1;
                // oldmat.SetColor("_Color", oldMatColor);
            }
            else if(activeSphere == sphere2){
                // Material oldmat = sphere1.GetComponent<Renderer>().material;

                //set new material alpha to 1
                Color newMatColor = newEnviro.skyboxMaterial.GetColor("_Color");
                newMatColor.a = 1;
                newEnviro.skyboxMaterial.SetColor("_Color", newMatColor);
                
                sphereLerpFrom = 1.0f;
                sphereLerpTo = 0.0f;

                // //Reset old material alpha to 1
                // Color newColor = oldmat.GetColor("_Color");
                // newColor.a = 1;
                // oldmat.SetColor("_Color", newColor);

                sphere1.GetComponent<Renderer>().material = newEnviro.skyboxMaterial;
            }
        }

        if(sunsetState == "transition"){
            float gradNow = rightNow / transitionDurationSeconds;
            float t = Mathf.Clamp01(gradNow);
            //SPLIT TONING SHADOW
            //make gradient from old colour to new colour
            if (gv.profile.TryGet(out splitToning))
            {
                // Debug.Log("Got the split toning");
                // Access the split toning parameters
                // splitToning = gv.profile.colorGrading.splitToning;
                // splitToning = gv.GetComponent<SplitToning>();
                splitToning.shadows.Interp(oldEnviro.splitToningShadow, newEnviro.splitToningShadow, t);
            }
            

            //LIGHTING
            //3 gradients
            //Move from old to new for each one
            // Change the sky color
            RenderSettings.ambientSkyColor = lightingGradients[0].Evaluate(gradNow);
            // Change the ground color
            RenderSettings.ambientGroundColor = lightingGradients[2].Evaluate(gradNow);
            // Change the equator color
            RenderSettings.ambientEquatorColor = lightingGradients[1].Evaluate(gradNow);

            //LIGHT INTENSITY
            //slowly add or subtract to old vlaue to get to new value
            // Calculate the interpolation factor between 0 and 1 based on current time and duration
            currentLightIntensity = Mathf.Lerp(oldEnviro.lightIntensity, newEnviro.lightIntensity, t);
            directionalLight.intensity = currentLightIntensity;

            //FOG INTENSITY
            currentFogIntensity = Mathf.Lerp(oldEnviro.distanceFogIntensity, newEnviro.distanceFogIntensity, t);
            fogSettings.distanceFogIntensity = currentFogIntensity;

            newGrad = LerpGradients(oldEnviro.fogGradient, newEnviro.fogGradient, gradNow);

            fogSettings.distanceGradient = newGrad;

            //RIVER COLOURS
            


            //SKYBOWL
            //slowly shift the alfa based on which sky bowl we are in
            currentSphereAlpha = Mathf.Lerp(sphereLerpFrom, sphereLerpTo, t);
            Material sphereMat = sphere2.GetComponent<Renderer>().material;
            Color newColor = sphereMat.GetColor("_Color");
            newColor.a = currentSphereAlpha;
            sphereMat.SetColor("_Color", newColor);
            
        }
        else if(sunsetState == "stay"){
            float gradNow = rightNow / stayDurationSeconds;
        }
    }

    void SetNow(){
        directionalLight.intensity = newEnviro.lightIntensity;
        fogSettings.distanceGradient = newEnviro.fogGradient;
        // if (gv.profile.TryGet(out splitToning))
        // {
        //     splitToning.shadows = newEnviro.splitToningShadow;
        // }
        Color newMatColor = newEnviro.skyboxMaterial.GetColor("_Color");
        newMatColor.a = 1;
        newEnviro.skyboxMaterial.SetColor("_Color", newMatColor);
        activeSphere.GetComponent<Renderer>().material = newEnviro.skyboxMaterial;
        RenderSettings.ambientSkyColor = newEnviro.lightingGradient[0];
        RenderSettings.ambientGroundColor = newEnviro.lightingGradient[1];
        RenderSettings.ambientEquatorColor = newEnviro.lightingGradient[2];
    }
}
