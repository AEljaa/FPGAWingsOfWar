using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HeneGames.Airplane
{
    [RequireComponent(typeof(Rigidbody))]
    public class SimpleAirPlaneController : NetworkBehaviour
    {
        public enum AirplaneState
        {
            Flying,
            Landing,
            Takeoff,
        }

        #region Private variables

        private List<SimpleAirPlaneCollider> airPlaneColliders = new List<SimpleAirPlaneCollider>();

        private float maxSpeed = 0.6f;
        private float speedMultiplier;
        private float currentYawSpeed;
        private float currentPitchSpeed;
        private float currentRollSpeed;
        private float currentSpeed;
        private float currentEngineLightIntensity;
        private float currentEngineSoundPitch;

        private bool planeIsDead;

        private Rigidbody rb;
        private Runway currentRunway;

        //Input variables
        private float inputH;
        private float inputV;
        private bool inputTurbo;
        private bool inputYawLeft;
        private bool inputYawRight;

        #endregion

        public AirplaneState airplaneState;

        [SerializeField]
        public bool useFPGA = true;

        [Header("Wing trail effects")]
        [Range(0.01f, 1f)]
        [SerializeField] private float trailThickness = 0.045f;
        [SerializeField] private TrailRenderer[] wingTrailEffects;

        [Header("Rotating speeds")]
        [Range(5f, 500f)]
        [SerializeField] private float yawSpeed = 50f;

        [Range(5f, 500f)]
        [SerializeField] private float pitchSpeed = 100f;

        [Range(5f, 500f)]
        [SerializeField] private float rollSpeed = 200f;

        [Header("Rotating speeds multiplers when turbo is used")]
        [Range(0.1f, 5f)]
        [SerializeField] private float yawTurboMultiplier = 0.3f;

        [Range(0.1f, 5f)]
        [SerializeField] private float pitchTurboMultiplier = 0.5f;

        [Range(0.1f, 5f)]
        [SerializeField] private float rollTurboMultiplier = 1f;

        [Header("Moving speed")]
        [Range(5f, 100f)]
        [SerializeField] private float defaultSpeed = 10f;

        [Range(10f, 200f)]
        [SerializeField] private float turboSpeed = 20f;

        [Range(0.1f, 50f)]
        [SerializeField] private float accelerating = 10f;

        [Range(0.1f, 50f)]
        [SerializeField] private float deaccelerating = 5f;

        [Header("Turbo settings")]
        [Range(0f, 100f)]
        [SerializeField] private float turboHeatingSpeed;

        [Range(0f, 100f)]
        [SerializeField] private float turboCooldownSpeed;

        [Header("Turbo heat values")]
        [Tooltip("Real-time information about the turbo's current temperature (do not change in the editor)")]
        [Range(0f, 100f)]
        [SerializeField] private float turboHeat;

        [Tooltip("You can set this to determine when the turbo should cease overheating and become operational again")]
        [Range(0f, 100f)]
        [SerializeField] private float turboOverheatOver;

        [SerializeField] private bool turboOverheat;

        [Header("Sideway force")]
        [Range(0.1f, 15f)]
        [SerializeField] private float sidewaysMovement = 15f;

        [Range(0.001f, 0.05f)]
        [SerializeField] private float sidewaysMovementXRot = 0.012f;

        [Range(0.1f, 5f)]
        [SerializeField] private float sidewaysMovementYRot = 1.5f;

        [Range(-1, 1f)]
        [SerializeField] private float sidewaysMovementYPos = 0.1f;

        [Header("Engine sound settings")]
        [SerializeField] private AudioSource engineSoundSource;

        [SerializeField] private float maxEngineSound = 1f;

        [SerializeField] private float defaultSoundPitch = 1f;

        [SerializeField] private float turboSoundPitch = 1.5f;

        [Header("Engine propellers settings")]
        [Range(10f, 10000f)]
        [SerializeField] private float propelSpeedMultiplier = 100f;

        [SerializeField] private GameObject[] propellers;

        [Header("Turbine light settings")]
        [Range(0.1f, 20f)]
        [SerializeField] private float turbineLightDefault = 1f;

        [Range(0.1f, 20f)]
        [SerializeField] private float turbineLightTurbo = 5f;

        [SerializeField] private Light[] turbineLights;

        [Header("Colliders")]
        [SerializeField] private Transform crashCollidersRoot;

        [Header("Takeoff settings")]
        [Tooltip("How far must the plane be from the runway before it can be controlled again")]
        [SerializeField] private float takeoffLenght = 30f;

        [SerializeField] public int health = 100;
        [SerializeField] public GameObject networkManagerUI;

        private string FPGA_data;

        private TcpListener niosReadCon;
        private TcpClient niosReadClient;
        private TcpClient niosWriteCon;

        private Thread sendThread;
        private Thread receiveThread;

        [SerializeField]
        public float yawDeadzone = 0.08f;

        [SerializeField]
        public int roundsRemaining = 500;

        private int lastVal = 500;
        private bool shouldUpdate = true;
        
        [SerializeField]
        public int fireRate = 700;
        
        private float nextShot;

        public Transform bulletSpawnPoint;
        [SerializeField]
        public GameObject bulletPrefab;

        [SerializeField]
        public float bulletSpeedMultiplier = 100f;
        private bool shouldShoot;

        private int initialSendCount = 5;


    private void StartServer()
    {
        try
        {
            // Create a socket object for receiving from Nios
            IPAddress niosReadAddr = IPAddress.Parse("127.0.0.1");
            int niosReadPort = 49152;
            niosReadCon = new TcpListener(niosReadAddr, niosReadPort);
            niosReadCon.Start();
            Debug.Log("Waiting for a connection from Nios...");

            niosReadClient = niosReadCon.AcceptTcpClient();
            Debug.Log("Connection established with Nios: " + niosReadClient.Client.RemoteEndPoint);

            // Create a socket object for sending to Nios
            IPAddress niosWriteAddr = IPAddress.Parse("127.0.0.1");
            int niosWritePort = 49153;
            niosWriteCon = new TcpClient();
            niosWriteCon.Connect(niosWriteAddr, niosWritePort);
            Debug.Log("Connected to Nios");

            // Start a thread for sending to Nios
            sendThread = new Thread(Send2Nios);
            sendThread.Start();

            // Start a thread for receiving from Nios
            receiveThread = new Thread(ReceiveFrmNios);
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while starting server: " + e.Message);
        }
    }

    private void Send2Nios()
    {
        try
        {
            while (true)
            {
                if (shouldUpdate || roundsRemaining != lastVal || initialSendCount > 0)
                {
                    string message = roundsRemaining.ToString();
                    if (message.ToLower() == "exit")
                    {
                        break;
                    }
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    niosWriteCon.GetStream().Write(data, 0, data.Length);
                    shouldUpdate = false;
                    lastVal = roundsRemaining;
                    if (initialSendCount > 0)
                    {
                        initialSendCount -= 1;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while sending to Nios: " + e.Message);
        }
        finally
        {
            niosWriteCon.Close();
        }
    }

            


    private void ReceiveFrmNios()
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[512];
                int bytesRead = niosReadClient.GetStream().Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                SetFPGAData(data);

                Debug.Log("Received from Nios: " + data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while receiving from Nios: " + e.Message);
        }
        finally
        {
            niosReadClient.Close();
        }
    }

    void OnApplicationQuit()
    {
        if (sendThread != null && sendThread.IsAlive)
        {
            sendThread.Abort();
        }

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }

        if (niosReadCon != null)
        {
            niosReadCon.Stop();
        }

        if (niosReadClient != null)
        {
            niosReadClient.Close();
        }

        if (niosWriteCon != null)
        {
            niosWriteCon.Close();
        }
    }

        private void Start()
        {
            if (useFPGA)
                StartServer();

            //Setup speeds
            maxSpeed = defaultSpeed;
            currentSpeed = defaultSpeed;
            ChangeSpeedMultiplier(1f);

            //Get and set rigidbody
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            SetupColliders(crashCollidersRoot);

            //var managerUI = networkManagerUI.GetComponent<NetworkUIScript>();
            //managerUI.UpdateHealthValue(health);
            roundsRemaining = 500;
            shouldUpdate = true;
        }

        public void PrintLocation()
        {
            Debug.Log($"#{transform.position}");
        }


        // Method to convert raw values to usable inputs
        // value[0] = x_val
        // value[1] = y_val
        // value[2] = z_val
        // value[3] = key_0
        // value[4] = key_1
        // value[5] = sw (needs to be converted)
        public void SetFPGAData(string data)
        {
            Debug.Log(data);

            string[] values = data.Split("\n");
            int count = 0; 
            for (int i = 0; i < values.Length; i++) {
                count = 0;
                for (int j = 0; j < values[i].Length; j++){
                    if (values[i][j] == ':')
                    {count++;}
                }
                if (values[i].Length > 0){
                    if (values[i][0] == 'x' && count == 6) {
                        string[] data_vals = values[i].Split(":");
                        string x_coord = data_vals[1];
                        string y_coord = data_vals[2];
                        string z_coord = data_vals[3];
                        /*Debug.Log($"coords, {x_coord}, {y_coord}, {z_coord}");*/
                        float horizontalVal = Convert.ToInt32(x_coord, 16) / 180.0f;
                        float verticalVal = Convert.ToInt32(y_coord, 16) / 180.0f;
                        float yawVal = Convert.ToInt32(z_coord, 16) / 180.0f;

                        if(Math.Abs(horizontalVal) < 0.05 /*|| Math.Abs(horizontalVal) > 0.8*/)
                            { horizontalVal = 0; }

                        if (Math.Abs(verticalVal) < 0.05 /*|| Math.Abs(verticalVal) > 0.8*/)
                        { verticalVal = 0; }

                        if (Math.Abs(yawVal) < 0.05 /*|| Math.Abs(yawVal) > 0.8*/)
                        { yawVal = 0; }

                        if (horizontalVal < 0) {
                            horizontalVal = (float)(-(Math.Abs(horizontalVal)));
                            //horizontalVal = (float)(-Math.Sqrt(Math.Abs(horizontalVal)));
                        }
                        else {
                            //horizontalVal = (float)(Math.Sqrt(Math.Abs(horizontalVal)));
                            horizontalVal = (float)(Math.Abs(horizontalVal));
                        }

                        if (verticalVal < 0 ) {
                            //verticalVal = (float)(-Math.Sqrt(Math.Abs(verticalVal)));
                            verticalVal = (float)(-(Math.Abs(verticalVal)));
                        }
                        else {
                            //verticalVal = (float)(Math.Sqrt(Math.Abs(verticalVal)));
                            verticalVal = (float)(Math.Abs(verticalVal));
                        }

                        horizontalVal *= (float)1.25;
                        verticalVal *= (float)1.25;
                        inputH = -horizontalVal;
                        inputV = -verticalVal;

                        // Shooting mechanics
                        shouldShoot = data_vals[4] == "1";
                        inputTurbo = data_vals[5] == "1";
                    }
                }
            }
        }

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

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }

            AudioSystem();
            HandleInputs();

            FlyingUpdate();

            if ((shouldShoot || Input.GetMouseButtonDown(1)) && roundsRemaining > 0) {
                FireServerRpc();
                shouldShoot = false;
                shouldUpdate = true;
                roundsRemaining -= 1;

            }
        }

        public void TakeDamage(int damage)
        {
            health -= damage;

            if (health <= 0)
            {
                DestroyPlane();
            }

            //var managerUI = networkManagerUI.GetComponent<NetworkUIScript>();
            //managerUI.UpdateHealthValue(health);

            Debug.Log($"Damage taken: #{damage}");
        }

        void DestroyPlane()
        {
            if (!NetworkObject.IsSpawned)
            {
                return;
            }

            if (IsServer)
            {
                NetworkObject.Despawn(true);
            }
        }

        #region Flying State

        private void FlyingUpdate()
        {
            UpdatePropellersAndLights();

            //Airplane move only if not dead
            if (!planeIsDead)
            {
                Movement();
                SidewaysForceCalculation();
            }
            else
            {
                ChangeWingTrailEffectThickness(0f);
            }

            //Crash
            if (!planeIsDead && HitSometing())
            {
                Crash();
            }
        }

        private void SidewaysForceCalculation()
        {
            float _mutiplierXRot = sidewaysMovement * sidewaysMovementXRot;
            float _mutiplierYRot = sidewaysMovement * sidewaysMovementYRot;

            float _mutiplierYPos = sidewaysMovement * sidewaysMovementYPos;

            //Right side 
            if (transform.localEulerAngles.z > 270f && transform.localEulerAngles.z < 360f)
            {
                float _angle = (transform.localEulerAngles.z - 270f) / (360f - 270f);
                float _invert = 1f - _angle;

                transform.Rotate(Vector3.up * (_invert * _mutiplierYRot) * Time.deltaTime);
                transform.Rotate(Vector3.right * (-_invert * _mutiplierXRot) * currentPitchSpeed * Time.deltaTime);

                transform.Translate(transform.up * (_invert * _mutiplierYPos) * Time.deltaTime);
            }

            //Left side
            if (transform.localEulerAngles.z > 0f && transform.localEulerAngles.z < 90f)
            {
                float _angle = transform.localEulerAngles.z / 90f;

                transform.Rotate(-Vector3.up * (_angle * _mutiplierYRot) * Time.deltaTime);
                transform.Rotate(Vector3.right * (-_angle * _mutiplierXRot) * currentPitchSpeed * Time.deltaTime);

                transform.Translate(transform.up * (_angle * _mutiplierYPos) * Time.deltaTime);
            }

            //Right side down
            if (transform.localEulerAngles.z > 90f && transform.localEulerAngles.z < 180f)
            {
                float _angle = (transform.localEulerAngles.z - 90f) / (180f - 90f);
                float _invert = 1f - _angle;

                transform.Translate(transform.up * (_invert * _mutiplierYPos) * Time.deltaTime);
                transform.Rotate(Vector3.right * (-_invert * _mutiplierXRot) * currentPitchSpeed * Time.deltaTime);
            }

            //Left side down
            if (transform.localEulerAngles.z > 180f && transform.localEulerAngles.z < 270f)
            {
                float _angle = (transform.localEulerAngles.z - 180f) / (270f - 180f);

                transform.Translate(transform.up * (_angle * _mutiplierYPos) * Time.deltaTime);
                transform.Rotate(Vector3.right * (-_angle * _mutiplierXRot) * currentPitchSpeed * Time.deltaTime);
            }
        }

        private void Movement()
        {
            //Move forward
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

            //Rotate airplane by inputs
            transform.Rotate(Vector3.forward * -inputH * currentRollSpeed * Time.deltaTime);
            transform.Rotate(Vector3.right * inputV * currentPitchSpeed * Time.deltaTime);

            //Rotate yaw
            if (inputYawRight)
            {
                transform.Rotate(Vector3.up * currentYawSpeed * Time.deltaTime);
            }
            else if (inputYawLeft)
            {
                transform.Rotate(-Vector3.up * currentYawSpeed * Time.deltaTime);
            }

            //Accelerate and deacclerate
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += accelerating * Time.deltaTime;
            }
            else
            {
                currentSpeed -= deaccelerating * Time.deltaTime;
            }

            //Turbo
            if (inputTurbo && !turboOverheat)
            {
                //Turbo overheating
                if(turboHeat > 100f)
                {
                    turboHeat = 100f;
                    turboOverheat = true;
                }
                else
                {       
                    //Add turbo heat
                    turboHeat += Time.deltaTime * turboHeatingSpeed;
                }

                //Set speed to turbo speed and rotation to turbo values
                maxSpeed = turboSpeed;

                currentYawSpeed = yawSpeed * yawTurboMultiplier;
                currentPitchSpeed = pitchSpeed * pitchTurboMultiplier;
                currentRollSpeed = rollSpeed * rollTurboMultiplier;

                //Engine lights
                currentEngineLightIntensity = turbineLightTurbo;

                //Effects
                ChangeWingTrailEffectThickness(trailThickness);

                //Audio
                currentEngineSoundPitch = turboSoundPitch;
            }
            else
            {
                //Turbo cooling down
                if(turboHeat > 0f)
                {
                    turboHeat -= Time.deltaTime * turboCooldownSpeed;
                }
                else
                {
                    turboHeat = 0f;
                }

                //Turbo cooldown
                if (turboOverheat)
                {
                   if(turboHeat <= turboOverheatOver)
                   {
                        turboOverheat = false;
                   }
                }

                //Speed and rotation normal
                maxSpeed = defaultSpeed * speedMultiplier;

                currentYawSpeed = yawSpeed;
                currentPitchSpeed = pitchSpeed;
                currentRollSpeed = rollSpeed;

                //Engine lights
                currentEngineLightIntensity = turbineLightDefault;

                //Effects
                ChangeWingTrailEffectThickness(0f);

                //Audio
                currentEngineSoundPitch = defaultSoundPitch;
            }
        }

        #endregion

        #region Landing State

        public void AddLandingRunway(Runway _landingThisRunway)
        {
            currentRunway = _landingThisRunway;
        }

        //My trasform is runway landing adjuster child
        private void LandingUpdate()
        {
            UpdatePropellersAndLights();

            ChangeWingTrailEffectThickness(0f);

            //Stop speed
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime);

            //Set local rotation to zero
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0f,0f,0f), 2f * Time.deltaTime);
        }

        #endregion

        #region Takeoff State

        private void TakeoffUpdate()
        {
            UpdatePropellersAndLights();

            //Reset colliders
            foreach (SimpleAirPlaneCollider _airPlaneCollider in airPlaneColliders)
            {
                _airPlaneCollider.collideSometing = false;
            }

            //Accelerate
            if (currentSpeed < turboSpeed)
            {
                currentSpeed += (accelerating * 2f) * Time.deltaTime;
            }

            //Move forward
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

            //Far enough from the runaway go back to flying state
            float _distanceToRunway = Vector3.Distance(transform.position, currentRunway.transform.position);
            if(_distanceToRunway > takeoffLenght)
            {
                currentRunway = null;
                airplaneState = AirplaneState.Flying;
            }
        }

        #endregion

        #region Audio
        private void AudioSystem()
        {
            if (engineSoundSource == null)
                return;

            if (airplaneState == AirplaneState.Flying)
            {
                engineSoundSource.pitch = Mathf.Lerp(engineSoundSource.pitch, currentEngineSoundPitch, 10f * Time.deltaTime);

                if (planeIsDead)
                {
                    engineSoundSource.volume = Mathf.Lerp(engineSoundSource.volume, 0f, 10f * Time.deltaTime);
                }
                else
                {
                    engineSoundSource.volume = Mathf.Lerp(engineSoundSource.volume, maxEngineSound, 1f * Time.deltaTime);
                }
            }
            else if (airplaneState == AirplaneState.Landing)
            {
                engineSoundSource.pitch = Mathf.Lerp(engineSoundSource.pitch, defaultSoundPitch, 1f * Time.deltaTime);
                engineSoundSource.volume = Mathf.Lerp(engineSoundSource.volume, 0f, 1f * Time.deltaTime);
            }
            else if (airplaneState == AirplaneState.Takeoff)
            {
                engineSoundSource.pitch = Mathf.Lerp(engineSoundSource.pitch, turboSoundPitch, 1f * Time.deltaTime);
                engineSoundSource.volume = Mathf.Lerp(engineSoundSource.volume, maxEngineSound, 1f * Time.deltaTime);
            }
        }

        #endregion

        #region Private methods

        private void UpdatePropellersAndLights()
        {
            if(!planeIsDead)
            {
                //Rotate propellers if any
                if (propellers.Length > 0)
                {
                    RotatePropellers(propellers, currentSpeed * propelSpeedMultiplier);
                }

                //Control lights if any
                if (turbineLights.Length > 0)
                {
                    ControlEngineLights(turbineLights, currentEngineLightIntensity);
                }
            }
            else
            {
                //Rotate propellers if any
                if (propellers.Length > 0)
                {
                    RotatePropellers(propellers, 0f);
                }

                //Control lights if any
                if (turbineLights.Length > 0)
                {
                    ControlEngineLights(turbineLights, 0f);
                }
            }
        }

        private void SetupColliders(Transform _root)
        {
            //if (_root == null)
            //    return;

            ////Get colliders from root transform
            //Collider[] colliders = _root.GetComponentsInChildren<Collider>();

            ////If there are colliders put components in them
            //for (int i = 0; i < colliders.Length; i++)
            //{
            //    //Change collider to trigger
            //    colliders[i].isTrigger = false;

            //    GameObject _currentObject = colliders[i].gameObject;

            //    //Add airplane collider to it and put it on the list
            //    SimpleAirPlaneCollider _airplaneCollider = _currentObject.AddComponent<SimpleAirPlaneCollider>();
            //    airPlaneColliders.Add(_airplaneCollider);

            //    //Add airplane conroller reference to collider
            //    _airplaneCollider.controller = this;

            //    //Add rigid body to it
            //    Rigidbody _rb = _currentObject.AddComponent<Rigidbody>();
            //    _rb.useGravity = true;
            //    _rb.isKinematic = false;
            //    _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            //}
        }

        private void RotatePropellers(GameObject[] _rotateThese, float _speed)
        {
            for (int i = 0; i < _rotateThese.Length; i++)
            {
                _rotateThese[i].transform.Rotate(Vector3.forward * -_speed * Time.deltaTime);
            }
        }

        private void ControlEngineLights(Light[] _lights, float _intensity)
        {
            for (int i = 0; i < _lights.Length; i++)
            {
                if(!planeIsDead)
                {
                    _lights[i].intensity = Mathf.Lerp(_lights[i].intensity, _intensity, 10f * Time.deltaTime);
                }
                else
                {
                    _lights[i].intensity = Mathf.Lerp(_lights[i].intensity, 0f, 10f * Time.deltaTime);
                }
               
            }
        }

        private void ChangeWingTrailEffectThickness(float _thickness)
        {
            for (int i = 0; i < wingTrailEffects.Length; i++)
            {
                wingTrailEffects[i].startWidth = Mathf.Lerp(wingTrailEffects[i].startWidth, _thickness, Time.deltaTime * 10f);
            }
        }

        private bool HitSometing()
        {
            for (int i = 0; i < airPlaneColliders.Count; i++)
            {
                if (airPlaneColliders[i].collideSometing)
                {
                    //Reset colliders
                    foreach(SimpleAirPlaneCollider _airPlaneCollider in airPlaneColliders)
                    {
                        _airPlaneCollider.collideSometing = false;
                    }

                    return true;
                }
            }

            return false;
        }

        private void Crash()
        {
            //Set rigidbody to non cinematic
            //rb.isKinematic = false;
            //rb.useGravity = true;

            ////Change every collider trigger state and remove rigidbodys
            //for (int i = 0; i < airPlaneColliders.Count; i++)
            //{
            //    airPlaneColliders[i].GetComponent<Collider>().isTrigger = false;
            //    Destroy(airPlaneColliders[i].GetComponent<Rigidbody>());
            //}

            //Kill player
            //planeIsDead = true;

            //Here you can add your own code...
        }

        #endregion

        #region Variables

        /// <summary>
        /// Returns a percentage of how fast the current speed is from the maximum speed between 0 and 1
        /// </summary>
        /// <returns></returns>
        public float PercentToMaxSpeed()
        {
            float _percentToMax = (currentSpeed * speedMultiplier) / turboSpeed;

            return _percentToMax;
        }

        public bool PlaneIsDead()
        {
            return planeIsDead;
        }

        public bool UsingTurbo()
        {
            if(maxSpeed == turboSpeed)
            {
                return true;
            }

            return false;
        }

        public float CurrentSpeed()
        {
            return currentSpeed * speedMultiplier;
        }

        /// <summary>
        /// Returns a turbo heat between 0 and 100
        /// </summary>
        /// <returns></returns>
        public float TurboHeatValue()
        {
            return turboHeat;
        }

        public bool TurboOverheating()
        {
            return turboOverheat;
        }

        /// <summary>
        /// With this you can adjust the default speed between one and zero
        /// </summary>
        /// <param name="_speedMultiplier"></param>
        public void ChangeSpeedMultiplier(float _speedMultiplier)
        {
            if(_speedMultiplier < 0f)
            {
                _speedMultiplier = 0f;
            }

            if(_speedMultiplier > 1f)
            {
                _speedMultiplier = 1f;
            }

            speedMultiplier = _speedMultiplier;
        }

        #endregion

        #region Inputs

        private void HandleInputs()
        {
            if (!useFPGA)
            {
                //Rotate inputs
                inputH = Input.GetAxis("Horizontal");
                inputV = Input.GetAxis("Vertical");

                //Yaw axis inputs
                inputYawLeft = Input.GetKey(KeyCode.Q);
                inputYawRight = Input.GetKey(KeyCode.E);


                // Debug.Log($"InputH: #{inputYawLeft}, inputV: #{inputYawRight}");

                //Turbo
                inputTurbo = Input.GetKey(KeyCode.LeftShift);
            }
        }

        #endregion
    }
}
