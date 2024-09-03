using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
public class CameraController : NetworkBehaviour
{
        public GameObject CameraHolder;
        public Vector3 offset;

        public void Update()
        {
            if (SceneManager.GetActiveScene().name == "Game")
            {
                    CameraHolder.transform.position = transform.position + offset;
            }
        }
        public override void OnNetworkSpawn() { // This is basically a Start method
                CameraHolder.SetActive(IsOwner);
                base.OnNetworkSpawn(); // Not sure if this is needed though, but good to have it.
        }
}
