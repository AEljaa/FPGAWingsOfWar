using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Coin : NetworkBehaviour
{
    [SerializeField]
    public float CoinValue = 10;
    public float rotationSpeed;
    public float GetCoinValue() { return CoinValue; }

    public void DestroyCoin()
    {
        if (!NetworkObject.IsSpawned)
            return;

        if (IsServer)
        {
            NetworkObject.Despawn(true);
        }
    }
    public void Update(){
        transform.Rotate(0,0,rotationSpeed);    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (other.CompareTag("Player"))
        {
            Debug.Log($"Coin Collected with value: #{GetCoinValue()}");

            DestroyCoin();
        }
    }
}
