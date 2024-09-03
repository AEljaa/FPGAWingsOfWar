using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace HeneGames.Airplane
{
    public class SimpleAirPlaneCollider : NetworkBehaviour
    {
        public bool collideSometing;

        [HideInInspector]
        public SimpleAirPlaneController controller;

        private void OnTriggerEnter(Collider other)
        {
            //Collide someting bad
            if(other.gameObject.GetComponent<SimpleAirPlaneCollider>() == null && other.gameObject.GetComponent<LandingArea>() == null)
            {
                collideSometing = true;
            }
        }
    }
}