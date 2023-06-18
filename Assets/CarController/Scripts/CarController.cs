using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.Serialization;

public enum GearState
{
    Neutral,
    Running,
    CheckingChange,
    Changing
};

public class CarController : MonoBehaviour
{
    LogitechGSDK.LogiControllerPropertiesData properties;
    private string actualState;
    private string activeForces;
    private string propertiesEdit;
    private string buttonStatus;
    private string forcesLabel;
    string[] activeForceAndEffect;
    private LogitechGSDK.DIJOYSTATE2ENGINES rec;

    
    
    private Rigidbody playerRB;
    public WheelColliders colliders;
    public Transforms transforms;
    public WheelParticles wheelParticles;
    private float gasInput;
    private float brakeInput;
    private float steeringInput;
    public GameObject smokePrefab;
    public float motorPower;
    public float brakePower;
    public float slipAngle;
    public float speed;
    private float speedClamped;
    public float maxSpeed;
    public AnimationCurve steeringCurve;
    
    public int isEngineRunning;

    public float RPM;
    public float redLine;
    public float idleRPM;
    public TMP_Text rpmText;
    public TMP_Text gearText;
    public Transform rpmNeedle;
    public float minNeedleRotation;
    public float maxNeedleRotation;
    public int currentGear;

    public float[] gearRatios;
    public float differentialRatio;
    private float currentTorque;
    private float clutch;
    private float wheelRPM;
    public AnimationCurve hpToRPMCurve;
    private GearState gearState;
    public float increaseGearRPM;
    public float decreaseGearRPM;
    public float changeGearTime=0.5f;

    public GameObject tireTrail;
    public Material brakeMaterial;
    public Color brakingColor;
    public float brakeColorIntensity;
    
    // Force Feedback
    [SerializeField] private AccelerationCalculator accelerationCalculator;
    [SerializeField] [Range(0,100)] private int offsetPercentage;
    [FormerlySerializedAs("coeficientPercentage")]
    [SerializeField] [Range(0,100)] private int saturationPercentage;
    [SerializeField] private float normalizedStrength;

    [SerializeField] private int forceMagnitude;

    private bool guiDisplay = false;
    
    //For steering wheel movements
    public WheelRotator SteeringWheel;
    
    // Start is called before the first frame update
    void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        InitiateParticles();
        
        activeForces = "";
        propertiesEdit = "";
        actualState = "";
        buttonStatus = "";
        forcesLabel = "Press the following keys to activate forces and effects on the steering wheel / gaming controller \n";
        forcesLabel += "Spring force : S\n";
        forcesLabel += "Constant force : C\n";
        forcesLabel += "Damper force : D\n";
        forcesLabel += "Side collision : Left or Right Arrow\n";
        forcesLabel += "Front collision : Up arrow\n";
        forcesLabel += "Dirt road effect : I\n";
        forcesLabel += "Bumpy road effect : B\n";
        forcesLabel += "Slippery road effect : L\n";
        forcesLabel += "Surface effect : U\n";
        forcesLabel += "Car Airborne effect : A\n";
        forcesLabel += "Soft Stop Force : O\n";
        forcesLabel += "Set example controller properties : PageUp\n";
        forcesLabel += "Play Leds : P\n";
        activeForceAndEffect = new string[9];
        Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
    }

    void OnApplicationQuit()
    {
        Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());
    }
    
    void OnGUI()
    {            
        if (Input.GetKeyUp(KeyCode.G))
            guiDisplay = true;
        else
            guiDisplay = false;
        
        if (guiDisplay)
        {
            activeForces = GUI.TextArea(new Rect(10, 10, 180, 200), activeForces, 400);
            propertiesEdit = GUI.TextArea(new Rect(200, 10, 200, 200), propertiesEdit, 400);
            actualState = GUI.TextArea(new Rect(410, 10, 300, 200), actualState, 1000);
            buttonStatus = GUI.TextArea(new Rect(720, 10, 300, 200), buttonStatus, 1000);
            GUI.Label(new Rect(10, 400, 800, 400), forcesLabel);
        }
    }
    
    void InitiateParticles()
    {
        if (smokePrefab)
        {
            wheelParticles.FRWheel = Instantiate(smokePrefab, colliders.FRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FRWheel.transform)
                .GetComponent<ParticleSystem>();
            wheelParticles.FLWheel = Instantiate(smokePrefab, colliders.FLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FLWheel.transform)
                .GetComponent<ParticleSystem>();
            wheelParticles.RRWheel = Instantiate(smokePrefab, colliders.RRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RRWheel.transform)
                .GetComponent<ParticleSystem>();
            wheelParticles.RLWheel = Instantiate(smokePrefab, colliders.RLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RLWheel.transform)
                .GetComponent<ParticleSystem>();
        }
        if (tireTrail)
        {
            wheelParticles.FRWheelTrail = Instantiate(tireTrail, colliders.FRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FRWheel.transform)
                .GetComponent<TrailRenderer>();
            wheelParticles.FLWheelTrail = Instantiate(tireTrail, colliders.FLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FLWheel.transform)
                .GetComponent<TrailRenderer>();
            wheelParticles.RRWheelTrail = Instantiate(tireTrail, colliders.RRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RRWheel.transform)
                .GetComponent<TrailRenderer>();
            wheelParticles.RLWheelTrail = Instantiate(tireTrail, colliders.RLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RLWheel.transform)
                .GetComponent<TrailRenderer>();
        }
    }
    // Update is called once per frame

    void Update()
    {
                //All the test functions are called on the first device plugged in(index = 0)
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {

            //CONTROLLER PROPERTIES
            StringBuilder deviceName = new StringBuilder(256);
            LogitechGSDK.LogiGetFriendlyProductName(0, deviceName, 256);
            propertiesEdit = "Current Controller : " + deviceName + "\n";
            propertiesEdit += "Current controller properties : \n\n";
            LogitechGSDK.LogiControllerPropertiesData actualProperties = new LogitechGSDK.LogiControllerPropertiesData();
            LogitechGSDK.LogiGetCurrentControllerProperties(0, ref actualProperties);
            propertiesEdit += "forceEnable = " + actualProperties.forceEnable + "\n";
            propertiesEdit += "overallGain = " + actualProperties.overallGain + "\n";
            propertiesEdit += "springGain = " + actualProperties.springGain + "\n";
            propertiesEdit += "damperGain = " + actualProperties.damperGain + "\n";
            propertiesEdit += "defaultSpringEnabled = " + actualProperties.defaultSpringEnabled + "\n";
            propertiesEdit += "combinePedals = " + actualProperties.combinePedals + "\n";
            propertiesEdit += "wheelRange = " + actualProperties.wheelRange + "\n";
            propertiesEdit += "gameSettingsEnabled = " + actualProperties.gameSettingsEnabled + "\n";
            propertiesEdit += "allowGameSettings = " + actualProperties.allowGameSettings + "\n";

            //CONTROLLER STATE
            actualState = "Steering wheel current state : \n\n";
            
            rec = LogitechGSDK.LogiGetStateUnity(0);
            actualState += "x-axis position :" + rec.lX + "\n";
            actualState += "y-axis position :" + rec.lY + "\n";
            actualState += "z-axis position :" + rec.lZ + "\n";
            actualState += "x-axis rotation :" + rec.lRx + "\n";
            actualState += "y-axis rotation :" + rec.lRy + "\n";
            actualState += "z-axis rotation :" + rec.lRz + "\n";
            actualState += "extra axes positions 1 :" + rec.rglSlider[0] + "\n";
            actualState += "extra axes positions 2 :" + rec.rglSlider[1] + "\n";
            switch (rec.rgdwPOV[0])
            {
                case (0): actualState += "POV : UP\n"; break;
                case (4500): actualState += "POV : UP-RIGHT\n"; break;
                case (9000): actualState += "POV : RIGHT\n"; break;
                case (13500): actualState += "POV : DOWN-RIGHT\n"; break;
                case (18000): actualState += "POV : DOWN\n"; break;
                case (22500): actualState += "POV : DOWN-LEFT\n"; break;
                case (27000): actualState += "POV : LEFT\n"; break;
                case (31500): actualState += "POV : UP-LEFT\n"; break;
                default: actualState += "POV : CENTER\n"; break;
            }

            //Button status :

            buttonStatus = "Button pressed : \n\n";
            for (int i = 0; i < 128; i++)
            {
                if (rec.rgbButtons[i] == 128)
                {
                    buttonStatus += "Button " + i + " pressed\n";
                }

            }

            /* THIS AXIS ARE NEVER REPORTED BY LOGITECH CONTROLLERS 
             * 
             * actualState += "x-axis velocity :" + rec.lVX + "\n";
             * actualState += "y-axis velocity :" + rec.lVY + "\n";
             * actualState += "z-axis velocity :" + rec.lVZ + "\n";
             * actualState += "x-axis angular velocity :" + rec.lVRx + "\n";
             * actualState += "y-axis angular velocity :" + rec.lVRy + "\n";
             * actualState += "z-axis angular velocity :" + rec.lVRz + "\n";
             * actualState += "extra axes velocities 1 :" + rec.rglVSlider[0] + "\n";
             * actualState += "extra axes velocities 2 :" + rec.rglVSlider[1] + "\n";
             * actualState += "x-axis acceleration :" + rec.lAX + "\n";
             * actualState += "y-axis acceleration :" + rec.lAY + "\n";
             * actualState += "z-axis acceleration :" + rec.lAZ + "\n";
             * actualState += "x-axis angular acceleration :" + rec.lARx + "\n";
             * actualState += "y-axis angular acceleration :" + rec.lARy + "\n";
             * actualState += "z-axis angular acceleration :" + rec.lARz + "\n";
             * actualState += "extra axes accelerations 1 :" + rec.rglASlider[0] + "\n";
             * actualState += "extra axes accelerations 2 :" + rec.rglASlider[1] + "\n";
             * actualState += "x-axis force :" + rec.lFX + "\n";
             * actualState += "y-axis force :" + rec.lFY + "\n";
             * actualState += "z-axis force :" + rec.lFZ + "\n";
             * actualState += "x-axis torque :" + rec.lFRx + "\n";
             * actualState += "y-axis torque :" + rec.lFRy + "\n";
             * actualState += "z-axis torque :" + rec.lFRz + "\n";
             * actualState += "extra axes forces 1 :" + rec.rglFSlider[0] + "\n";
             * actualState += "extra axes forces 2 :" + rec.rglFSlider[1] + "\n";
             */

            int shifterTipe = LogitechGSDK.LogiGetShifterMode(0);
            string shifterString = "";
            if (shifterTipe == 1) shifterString = "Gated";
            else if (shifterTipe == 0) shifterString = "Sequential";
            else shifterString = "Unknown";
            actualState += "\nSHIFTER MODE:" + shifterString;




            // FORCES AND EFFECTS 
            activeForces = "Active forces and effects :\n";

            /*//Spring Force -> S
            if (Input.GetKeyUp(KeyCode.S))
            {
                if (LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_SPRING))
                {
                    LogitechGSDK.LogiStopSpringForce(0);
                    activeForceAndEffect[0] = "";
                }
                else
                {
                    LogitechGSDK.LogiPlaySpringForce(0, 50, 50, 50);
                    activeForceAndEffect[0] = "Spring Force\n ";
                }
            }

            //Constant Force -> C
            if (Input.GetKeyUp(KeyCode.C))
            {
                if (LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_CONSTANT))
                {
                    LogitechGSDK.LogiStopConstantForce(0);
                    activeForceAndEffect[1] = "";
                }
                else
                {
                    LogitechGSDK.LogiPlayConstantForce(0, 50);
                    activeForceAndEffect[1] = "Constant Force\n ";
                }
            }

            //Damper Force -> D
            if (Input.GetKeyUp(KeyCode.D))
            {
                if (LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_DAMPER))
                {
                    LogitechGSDK.LogiStopDamperForce(0);
                    activeForceAndEffect[2] = "";
                }
                else
                {
                    LogitechGSDK.LogiPlayDamperForce(0, 50);
                    activeForceAndEffect[2] = "Damper Force\n ";
                }
            }

            //Side Collision Force -> left or right arrow
            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                LogitechGSDK.LogiPlaySideCollisionForce(0, 60);
            }

            //Front Collision Force -> up arrow
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                LogitechGSDK.LogiPlayFrontalCollisionForce(0, 60);
            }

            //Dirt Road Effect-> I
            if (Input.GetKeyUp(KeyCode.I))
            {
                if (LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_DIRT_ROAD))
                {
                    LogitechGSDK.LogiStopDirtRoadEffect(0);
                    activeForceAndEffect[3] = "";
                }
                else
                {
                    LogitechGSDK.LogiPlayDirtRoadEffect(0, 50);
                    activeForceAndEffect[3] = "Dirt Road Effect\n ";
                }

            }

            //Bumpy Road Effect-> B
            if (Input.GetKeyUp(KeyCode.B))
            {
                if (LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_BUMPY_ROAD))
                {
                    LogitechGSDK.LogiStopBumpyRoadEffect(0);
                    activeForceAndEffect[4] = "";
                }
                else
                {
                    LogitechGSDK.LogiPlayBumpyRoadEffect(0, 50);
                    activeForceAndEffect[4] = "Bumpy Road Effect\n";
                }

            }

            //Slippery Road Effect-> L
            if (Input.GetKeyUp(KeyCode.L))
            {
                if (LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_SLIPPERY_ROAD))
                {
                    LogitechGSDK.LogiStopSlipperyRoadEffect(0);
                    activeForceAndEffect[5] = "";
                }
                else
                {
                    LogitechGSDK.LogiPlaySlipperyRoadEffect(0, 50);
                    activeForceAndEffect[5] = "Slippery Road Effect\n ";
                }
            }

            //Surface Effect-> U
            if (Input.GetKeyUp(KeyCode.U))
            {
                if (LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_SURFACE_EFFECT))
                {
                    LogitechGSDK.LogiStopSurfaceEffect(0);
                    activeForceAndEffect[6] = "";
                }
                else
                {
                    LogitechGSDK.LogiPlaySurfaceEffect(0, LogitechGSDK.LOGI_PERIODICTYPE_SQUARE, 50, 1000);
                    activeForceAndEffect[6] = "Surface Effect\n";
                }
            }

            //Car Airborne -> A
            if (Input.GetKeyUp(KeyCode.A))
            {
                if (LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_CAR_AIRBORNE))
                {
                    LogitechGSDK.LogiStopCarAirborne(0);
                    activeForceAndEffect[7] = "";
                }
                else
                {
                    LogitechGSDK.LogiPlayCarAirborne(0);
                    activeForceAndEffect[7] = "Car Airborne\n ";
                }
            }

            //Soft Stop Force -> O
            if (Input.GetKeyUp(KeyCode.O))
            {
                if (LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_SOFTSTOP))
                {
                    LogitechGSDK.LogiStopSoftstopForce(0);
                    activeForceAndEffect[8] = "";
                }
                else
                {
                    LogitechGSDK.LogiPlaySoftstopForce(0, 20);
                    activeForceAndEffect[8] = "Soft Stop Force\n";
                }
            }

            //Set preferred controller properties -> PageUp
            if (Input.GetKeyUp(KeyCode.PageUp))
            {
                //Setting example values
                properties.wheelRange = 90;
                properties.forceEnable = true;
                properties.overallGain = 80;
                properties.springGain = 80;
                properties.damperGain = 80;
                properties.allowGameSettings = true;
                properties.combinePedals = false;
                properties.defaultSpringEnabled = true;
                properties.defaultSpringGain = 80;
                LogitechGSDK.LogiSetPreferredControllerProperties(properties);

            }

            //Play leds -> P
            if (Input.GetKeyUp(KeyCode.P))
            {
                LogitechGSDK.LogiPlayLeds(0, 20, 20, 20);
            }*/

            for (int i = 0; i < 9; i++)
            {
                activeForces += activeForceAndEffect[i];
            }

        }
        else if (!LogitechGSDK.LogiIsConnected(0))
        {
            actualState = "PLEASE PLUG IN A STEERING WHEEL OR A FORCE FEEDBACK CONTROLLER";
        }
        else
        {
            actualState = "THIS WINDOW NEEDS TO BE IN FOREGROUND IN ORDER FOR THE SDK TO WORK PROPERLY";
        }
        
        
        rpmNeedle.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minNeedleRotation, maxNeedleRotation, RPM / (redLine*1.1f)));
        rpmText.text = RPM.ToString("0,000")+"rpm";
        gearText.text = (gearState==GearState.Neutral)?"N":(currentGear + 1).ToString();
        speed = colliders.RRWheel.rpm*colliders.RRWheel.radius*2f*Mathf.PI /10f;
        speedClamped = Mathf.Lerp(speedClamped, speed, Time.deltaTime);
        CheckInput();
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
        CheckParticles();
        ApplyWheelPositions();

        SetCenterForce();
    }

    void CheckInput()
    {
        //gasInput = Input.GetAxis("Vertical"); // pour les pédale
        gasInput = 0;

        gasInput += (rec.lRz - rec.lY) / 32767f;
        
        if (Mathf.Abs(gasInput) > 0 && isEngineRunning == 0)
        {
            StartCoroutine(GetComponent<EngineAudio>().StartEngine());
            gearState = GearState.Running;
        }
        steeringInput = rec.lX;
        SteeringWheel.wheelControllerRotation = rec.lX;

        slipAngle = Vector3.Angle(transform.forward, playerRB.velocity-transform.forward);

                //fixed code to brake even after going on reverse by Andrew Alex 
        float movingDirection = Vector3.Dot(transform.forward, playerRB.velocity);
        if (gearState != GearState.Changing)
        {
            if (gearState == GearState.Neutral)
            {
                clutch = 0;
                if (Mathf.Abs( gasInput )> 0) 
                    gearState = GearState.Running;
            }
            else
            {
                clutch = Input.GetKey(KeyCode.LeftShift) ? 0 : Mathf.Lerp(clutch, 1, Time.deltaTime);
            }
        }
        else
        {
            clutch = 0;
        }
        if (movingDirection < -0.5f && gasInput > 0)
        {
            brakeInput = Mathf.Abs(gasInput);
        }
        else if (movingDirection > 0.5f && gasInput < 0)
        {
            brakeInput = Mathf.Abs(gasInput);
        }
        else
        {
            brakeInput = 0;
        }
        


        /*
        old tutorial code
        if (slipAngle < 120f) {
            if (gasInput < 0)
            {
                brakeInput = Mathf.Abs( gasInput);
                gasInput = 0;
            }
            else
            {
                brakeInput = 0;
            }
        }
        else
        {
            brakeInput = 0;
        }*/

    }
    void ApplyBrake()
    {
        colliders.FRWheel.brakeTorque = brakeInput * brakePower* 0.7f ;
        colliders.FLWheel.brakeTorque = brakeInput * brakePower * 0.7f;

        colliders.RRWheel.brakeTorque = brakeInput * brakePower * 0.3f;
        colliders.RLWheel.brakeTorque = brakeInput * brakePower *0.3f;
        if (brakeMaterial)
        {
            if (brakeInput > 0)
            {
                brakeMaterial.EnableKeyword("_EMISSION");
                brakeMaterial.SetColor("_EmissionColor", brakingColor*Mathf.Pow(2,brakeColorIntensity));
            }
            else
            {
                brakeMaterial.DisableKeyword("_EMISSION");
                brakeMaterial.SetColor("_EmissionColor", Color.black);
            }
        }


    }
    void ApplyMotor() {

        currentTorque = CalculateTorque();
        colliders.RRWheel.motorTorque = currentTorque * gasInput;
        colliders.RLWheel.motorTorque = currentTorque * gasInput;

    }

    private void SetCenterForce()
    {        
        // Calculate the force magnitude based on the lateral force measured at the front tires
        forceMagnitude = accelerationCalculator.CalculateLateralForce(); // Implement your own logic to calculate lateral force

        // Set the center force feedback effect
        LogitechGSDK.LogiPlayConstantForce(0, forceMagnitude);
        // Get the current acceleration value
        //float acceleration = accelerationCalculator.acceleration;

        // Calculate the normalized force feedback strength based on the acceleration value
        ///normalizedStrength = Mathf.Clamp01(acceleration);

        // Set the center force feedback effect
        //LogitechGSDK.LogiPlaySpringForce(0, offsetPercentage, saturationPercentage, (int)(normalizedStrength * 100));

    }
    float CalculateTorque()
    {
        float torque = 0;
        if (RPM < idleRPM + 200 && gasInput==0 && currentGear == 0)
        {
            gearState = GearState.Neutral;
        }
        if (gearState == GearState.Running && clutch > 0)
        {
            if (RPM > increaseGearRPM)
            {
                StartCoroutine(ChangeGear(1));
            }
            else if (RPM < decreaseGearRPM)
            {
                StartCoroutine(ChangeGear(-1));
            }
        }
        if (isEngineRunning > 0)
        {
            if (clutch < 0.1f)
            {
                RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM, redLine * gasInput) + Random.Range(-50, 50), Time.deltaTime);
            }
            else
            {
                wheelRPM = Mathf.Abs((colliders.RRWheel.rpm + colliders.RLWheel.rpm) / 2f) * gearRatios[currentGear] * differentialRatio;
                RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM - 100, wheelRPM), Time.deltaTime * 3f);
                torque = (hpToRPMCurve.Evaluate(RPM / redLine) * motorPower / RPM) * gearRatios[currentGear] * differentialRatio * 5252f * clutch;
            }
        }
        return torque;
    }

    void ApplySteering()
    {

        float steeringAngle = steeringInput*steeringCurve.Evaluate(speed)/32767;
        if (slipAngle < 120f)
        {
            steeringAngle += Vector3.SignedAngle(transform.forward, playerRB.velocity + transform.forward, Vector3.up);
        }
        steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);
        colliders.FRWheel.steerAngle = steeringAngle;
        colliders.FLWheel.steerAngle = steeringAngle;
    }

    void ApplyWheelPositions()
    {
        UpdateWheel(colliders.FRWheel, transforms.FRWheel); 
        UpdateWheel(colliders.FLWheel, transforms.FLWheel);
        UpdateWheel(colliders.RRWheel, transforms.RRWheel);
        UpdateWheel(colliders.RLWheel, transforms.RLWheel);
    }
    void CheckParticles() {
        WheelHit[] wheelHits = new WheelHit[4];
        colliders.FRWheel.GetGroundHit(out wheelHits[0]);
        colliders.FLWheel.GetGroundHit(out wheelHits[1]);

        colliders.RRWheel.GetGroundHit(out wheelHits[2]);
        colliders.RLWheel.GetGroundHit(out wheelHits[3]);

        float slipAllowance = 0.2f;
        if ((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipAllowance)){
            wheelParticles.FRWheel.Play();
            wheelParticles.FRWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.FRWheel.Stop();

            wheelParticles.FRWheelTrail.emitting = false;
        }
        if ((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipAllowance)){
            wheelParticles.FLWheel.Play();

            wheelParticles.FLWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.FLWheel.Stop();

            wheelParticles.FLWheelTrail.emitting = false;
        }
        if ((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipAllowance)){
            wheelParticles.RRWheel.Play();

            wheelParticles.RRWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.RRWheel.Stop();

            wheelParticles.RRWheelTrail.emitting = false;
        }
        if ((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipAllowance)){
            wheelParticles.RLWheel.Play();

            wheelParticles.RLWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.RLWheel.Stop();

            wheelParticles.RLWheelTrail.emitting = false;
        }


    }
    void UpdateWheel(WheelCollider coll, Transform wheelTransform)
    {
        Quaternion quat;
        Vector3 position;
        coll.GetWorldPose(out position, out quat);
        position.z = wheelTransform.position.z;
        wheelTransform.position = position;
        wheelTransform.rotation = quat;
    }
    public float GetSpeedRatio()
    {
        var gas = Mathf.Clamp(Mathf.Abs( gasInput), 0.5f, 1f);
        return RPM * gas / redLine;
    }
    IEnumerator ChangeGear(int gearChange)
    {
        gearState = GearState.CheckingChange;
        if (currentGear + gearChange >= 0)
        {
            if (gearChange > 0)
            {
                //increase the gear
                yield return new WaitForSeconds(0.7f);
                if (RPM < increaseGearRPM || currentGear >= gearRatios.Length - 1)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            if (gearChange < 0)
            {
                //decrease the gear
                yield return new WaitForSeconds(0.1f);

                if (RPM > decreaseGearRPM || currentGear <= 0)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            gearState = GearState.Changing;
            yield return new WaitForSeconds(changeGearTime);
            currentGear += gearChange;
        }

        if(gearState!=GearState.Neutral)
        gearState = GearState.Running;
    }
}
[System.Serializable]
public class WheelColliders
{
    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider RRWheel;
    public WheelCollider RLWheel;
}
[System.Serializable]
public class Transforms
{
    public Transform FRWheel;
    public Transform FLWheel;
    public Transform RRWheel;
    public Transform RLWheel;
}
[System.Serializable]
public class WheelParticles{
    public ParticleSystem FRWheel;
    public ParticleSystem FLWheel;
    public ParticleSystem RRWheel;
    public ParticleSystem RLWheel;

    public TrailRenderer FRWheelTrail;
    public TrailRenderer FLWheelTrail;
    public TrailRenderer RRWheelTrail;
    public TrailRenderer RLWheelTrail;

}
