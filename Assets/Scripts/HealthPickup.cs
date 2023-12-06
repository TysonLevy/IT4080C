using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthPickup : NetworkBehaviour
{
   
    private void OnTriggerEnter(Collider col) { 
        if(col.CompareTag("Player")) {
            this.GetComponent<NetworkObject>().Despawn();
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
