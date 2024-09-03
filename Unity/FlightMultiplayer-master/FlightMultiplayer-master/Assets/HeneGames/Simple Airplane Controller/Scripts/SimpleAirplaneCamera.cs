using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using static HeneGames.Airplane.SimpleAirPlaneController;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace HeneGames.Airplane
{
    public class SimpleAirplaneCamera : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private SimpleAirPlaneController airPlaneController;
        [SerializeField] private CinemachineFreeLook freeLook;
        [SerializeField] private GameObject cameraObject;
        [Header("Camera values")]
        [SerializeField] private float cameraDefaultFov = 60f;
        [SerializeField] private float cameraTurboFov = 40f;

        private void Start()
        {
            //Lock and hide mouse
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name == "Game")
            {
                CameraFovUpdate();
            }
        }

        public override void OnNetworkSpawn()
        {
            cameraObject.SetActive(IsOwner);
            base.OnNetworkSpawn();
        }

        private void CameraFovUpdate()
        {
            //Turbo
            if(!airPlaneController.PlaneIsDead() && airPlaneController.airplaneState == AirplaneState.Flying)
            {
                if (Input.GetKey(KeyCode.LeftShift) && !airPlaneController.TurboOverheating())
                {
                    ChangeCameraFov(cameraTurboFov);
                }
                else
                {
                    ChangeCameraFov(cameraDefaultFov);
                }
            }
            else
            {
                ChangeCameraFov(cameraDefaultFov);
            }
        }

        public void ChangeCameraFov(float _fov)
        {
            float _deltatime = Time.deltaTime * 100f;
            freeLook.m_Lens.FieldOfView = Mathf.Lerp(freeLook.m_Lens.FieldOfView, _fov, 0.05f * _deltatime);
        }
    }
}