using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnviroSO", menuName = "EnviroSO")]
public class EnviroSO : ScriptableObject
{
    public Material skyboxMaterial;
//Global Volume split toning
    public Color splitToningShadow;
//RIVERFog Settings
    public Gradient fogGradient;
    public float distanceFogIntensity; 
//Directional Light
    public float lightIntensity;
//Liginting Settings -> Environment lighting Sky Colour, Equator Color, Ground Color
   [ColorUsageAttribute(true, true)] public Color[] lightingGradient;

    public bool night;

    public Color RiverShallow, RiverDeep, RiverFoam;

}
