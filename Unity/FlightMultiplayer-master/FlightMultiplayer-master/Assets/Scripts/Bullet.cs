using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using HeneGames.Airplane;

public class Bullet : NetworkBehaviour
{
    public float life = 10;

    [SerializeField]
    public float bulletSpeed = 2000f;

    void Start()
    {
        Invoke(nameof(DestroyBullet), life);
    }

    void DestroyBullet()
    {
        if (!NetworkObject.IsSpawned)
            return;

        if (IsServer)
        {
            NetworkObject.Despawn(true);
        }
    }

    public void SetVelocity(Vector3 velocity)
    {
        var bulletRb = GetComponent<Rigidbody>();
        bulletRb.velocity = velocity;

        if (IsServer)
            SetVelocityClientRpc(velocity);
    }

    [ClientRpc]
    void SetVelocityClientRpc(Vector3 velocity)
    {
        if (!IsHost)
        {
            var bulletRb = GetComponent<Rigidbody>();
            bulletRb.velocity = velocity;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        var other = collision.gameObject;


        if (other.CompareTag("Player"))
        {
            //Debug.Log("Hit Player");
            var player = other.GetComponent<SimpleAirPlaneController>();
            if (player != null)
            {
                player.PrintLocation();
            }
        }

        DestroyBullet();
    }
}