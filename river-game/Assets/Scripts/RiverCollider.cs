using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverCollider : MonoBehaviour
{
    [SerializeField] GameObject splash;
    void OnTriggerEnter(Collider col){
        if(col.tag == "Obstacle"){
            Obstacle obsty = col.gameObject.GetComponent<Obstacle>();
            if(obsty == null)
            {
                obsty = col.transform.parent.gameObject.GetComponent<Obstacle>();
            }
            Endless endless = GameObject.Find("Endlessness").GetComponent<Endless>();
            UIManager uim = GameObject.Find("Canvas").GetComponent<UIManager>();
            if(obsty.endGame){
                uim.EndGame();
            }
            else{
                endless.ReduceSpeed(5f);
                obsty.Score();
            }
            GameObject splashy = Instantiate(splash);
            // splashy.transform.parent = transform;
            splashy.transform.position = transform.position;
        }
    }
}
