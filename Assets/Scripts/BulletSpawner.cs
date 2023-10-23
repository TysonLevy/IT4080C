using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletSpawner : NetworkBehaviour
{
    public Rigidbody BulletPrefab;
    public float timeBetweenShots = .5f;
    private float bulletSpeed = 40f;
    private float timeSinceLastFire = 0;

    void Update() {
        if (timeSinceLastFire < timeBetweenShots) timeSinceLastFire += Time.deltaTime;
    }

    [ServerRpc]
    public void FireServerRpc(ServerRpcParams rpcParams = default) {
        if (timeSinceLastFire < timeBetweenShots) return;
        timeSinceLastFire = 0f;
        Rigidbody newBullet = Instantiate(BulletPrefab, transform.position, transform.rotation);
        newBullet.velocity = transform.forward * bulletSpeed;
        newBullet.gameObject.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        Destroy(newBullet.gameObject, 3);
    }
}
