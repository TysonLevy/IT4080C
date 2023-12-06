using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    public Slider healthUI;
    
    private void OnEnable() {
        GetComponent<Player>().playerHP.OnValueChanged += HealthChanged;
    }

    private void OnDisable()
    {
        GetComponent<Player>().playerHP.OnValueChanged -= HealthChanged;
    }

    private void HealthChanged(int previousValue, int newValue) { 
        if(newValue/100f > 1) { 
            healthUI.value = 1;
        } else {
            healthUI.value = newValue / 100f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
