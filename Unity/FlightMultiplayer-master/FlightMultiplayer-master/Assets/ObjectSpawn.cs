using UnityEngine;
using Unity.Netcode;

public class Spawner : MonoBehaviour
{
    [SerializeField] NetworkObjectPool m_ObjectPool;
    [SerializeField] private int m_CoinAmount = 100;
    [SerializeField] private GameObject m_CoinPrefab;

    void Start()
    {
        SpawnCoins();
    }

    void SpawnCoins()
    {
        for (int i = 0; i < m_CoinAmount; i++)
        {
            Vector3 coinPos = new Vector3(Random.Range(0, 5000), Random.Range(0, 500), Random.Range(0, 2000));
            GameObject coin = m_ObjectPool.GetNetworkObject(m_CoinPrefab).gameObject;
            coin.transform.position = coinPos;
            coin.GetComponent<NetworkObject>().Spawn();
        }
    }

    // void Update()
    // {
    //     Check if all coins are collected, then respawn
    //     if (Coin.m_CoinAmount == 0)
    //     {
    //         SpawnCoins();
    //     }
    // }
}