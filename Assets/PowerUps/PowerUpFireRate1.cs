using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpFireRate1 : BasePowerUp
{
    public float timeBetweenShots = .2f;
    protected override bool ApplyToPlayer(Player thePickerUpper) {
        if(thePickerUpper.bulletSpawner.timeBetweenShots <= timeBetweenShots) return false;
        
        thePickerUpper.bulletSpawner.timeBetweenShots = timeBetweenShots;
        return true;
    }
}
