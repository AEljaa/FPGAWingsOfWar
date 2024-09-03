
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI debugText = null;
    [SerializeField] private Button HostBtn ;
    [SerializeField] private Button ServerBtn ;
    [SerializeField] private Button ClientBtn ;

    public void Awake()
    {
       
        ServerBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        HostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            
        });
            ClientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            
        });
    }
}
