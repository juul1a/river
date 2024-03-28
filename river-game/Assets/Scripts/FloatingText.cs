using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    // Start is called before the first frame update

    public int destroyTime = 1;

    void Start()
    {
        
        Destroy(gameObject, destroyTime);
    }

    // Update is called once per frame
    public void SetTextColour(Color textColor)
    {
        this.GetComponent<TMP_Text>().color = textColor;
    }
}
