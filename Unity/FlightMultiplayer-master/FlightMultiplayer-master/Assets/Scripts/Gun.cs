using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Gun : NetworkBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;

    [SerializeField]
    public float bulletSpeedMultiplier = 100f;

    [ServerRpc]
    public void FireServerRpc()
    {
        FireServer(Quaternion.Euler(1,1,1) * transform.forward);
    }

    void FireServer(Vector3 direction)
    {
        var bulletGo = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        bulletGo.GetComponent<NetworkObject>().Spawn();

        var velocity = GetComponent<Rigidbody>().velocity;
        velocity += direction * bulletSpeedMultiplier;

        var bullet = bulletGo.GetComponent<Bullet>();
        bullet.SetVelocity(velocity);
    }

    void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.J))
        {
            FireServerRpc();
        }
    }
}