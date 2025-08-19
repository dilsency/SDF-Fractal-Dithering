/*
	hold ⌘ then K 0
	to fold all
*/
/*
	hold ⌘ then K L
	to toggle fold currently marked
*/

using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

using TMPro;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 2f;

    [SerializeField]
    private float lookSpeedH = 2f;

    [SerializeField]
    private float lookSpeedV = 2f;

    [SerializeField]
    private float zoomSpeed = 2f;

    [SerializeField]
    private float dragSpeed = 3f;

    [SerializeField]
    private GameObject targetLookAt;

    [SerializeField]
    private TextMeshProUGUI uiText;

    private float yaw = 0f;
    private float pitch = 0f;

    InputAction moveAction;
    InputAction moveUpDownAction;
    InputAction enableCameraAction;
    InputAction lookAction;
    InputAction gyroscopeAction;
    InputAction pauseGyroscopeAction;

    Gamepad controller;

    Vector3 gyroVec3;
    Quaternion gyro;
    Vector3 accel;

    bool gyroEnabled = false;
    bool accelEnabled = false;

    string resultString;

    private void Start()
    {
        // only once, look at specified object
        this.transform.LookAt(targetLookAt.transform);

        if (uiText != null)
        {
            uiText.text = "hej test";
            resultString = "";
        }

        //
        gyroVec3 = new Vector3();
        gyro = new Quaternion();
        accel = new Vector3();

        //
        Debug.Log("before: attempt to set controller");
        this.controller = DS4.getController();
        Debug.Log("after: attempt to set controller");

        //
        moveAction = InputSystem.actions.FindAction("Move");
        moveUpDownAction = InputSystem.actions.FindAction("MoveUpDown");
        enableCameraAction = InputSystem.actions.FindAction("EnableCamera");
        lookAction = InputSystem.actions.FindAction("Look");
        gyroscopeAction = InputSystem.actions.FindAction("Gyroscope");
        pauseGyroscopeAction = InputSystem.actions.FindAction("PauseGyroscope");

        // turn on gyro / accel
        if (UnityEngine.InputSystem.Gyroscope.current != null)
        {
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
            gyroEnabled = true;
            Debug.Log("DID enable gyroscope");
        }
        else
        {   
            Debug.Log("did NOT enable gyroscope");
        }
        
        if (UnityEngine.InputSystem.Accelerometer.current != null)
        {
            InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);
            accelEnabled = true;
            Debug.Log("DID enable accelerometer");
        }
        else
        {
            Debug.Log("did NOT enable accelerometer");
        }

        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
        }

        // Initialize the correct initial rotation
        this.yaw = this.transform.eulerAngles.y;
        this.pitch = this.transform.eulerAngles.x;
    }

    private bool isEnabledCameraMovement = false;

    private void Update()
    {
        UpdateConnectGamepad();

        UpdateGamepad();

        UpdateCameraRotation();
        UpdateCameraRotationGyro();
        UpdateCameraPosition();

        UpdateHUD();
    }

    private void UpdateConnectGamepad()
    {
        // early return: both enabled
        if (gyroEnabled && accelEnabled) { return; }

        if(this.controller == null){ return; }
        InputSystem.EnableDevice(this.controller);

        //
        if (!gyroEnabled)
        {
            if (UnityEngine.InputSystem.Gyroscope.current != null)
            {
                InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
                gyroEnabled = true;
                Debug.Log("DID enable gyroscope");
            }
        }

        if (!accelEnabled)
        {
            if (UnityEngine.InputSystem.Accelerometer.current != null)
            {
                InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);
                accelEnabled = true;
                Debug.Log("DID enable accelerometer");
            }
        }
        
        //test only
        gyroEnabled = true;
        accelEnabled = true;
    }

    private float throttleGetController = 0;
    private float throttleGetControllerMax = 3;

    private void UpdateGamepad()
    {
        // no controller?
        if (controller == null)
        {
            // throttle on get controller, if there is none
            throttleGetController += Time.deltaTime;
            if (throttleGetController >= throttleGetControllerMax)
            {
                throttleGetController = 0;
                controller = DS4.getController();
            }
            return;
        }

        //
        int gyroScale = 4000;
        gyroVec3 = DS4.getGyroVec3(gyroScale * Time.deltaTime);
        gyro = DS4.getGyro(gyroScale * Time.deltaTime);
        accel = DS4.getAccel(gyroScale);

        return;

        if (true == true)
        {
            //
            this.yaw += this.lookSpeedH * accel.x * 0.001f;
        }
        else
        {
            //
            //this.yaw += this.lookSpeedH * gyro.eulerAngles.x;
            //this.pitch -= this.lookSpeedV * q.eulerAngles.y;
        }

        //
        this.transform.eulerAngles = new Vector3(this.pitch, this.yaw, 0f);

    }

    private void UpdateCameraRotation()
    {
        // one Could separate mouse movement from gamepad stick movement
        // not particularly important though

        // get from input system
        //Vector2 enableCameraValue = enableCameraAction.ReadValue<Vector2>();
        Vector2 lookValue = lookAction.ReadValue<Vector2>();

        isEnabledCameraMovement = (enableCameraAction.IsPressed());
        if (Gamepad.current != null) { isEnabledCameraMovement = true; }

        // early return: not enabled
        if (!isEnabledCameraMovement) { return; }

        //
        float mouseX = lookValue.x;
        float mouseY = lookValue.y;

        //
        this.yaw += this.lookSpeedH * mouseX;
        this.pitch -= this.lookSpeedV * mouseY;

        //
        this.transform.eulerAngles = new Vector3(this.pitch, this.yaw, 0f);
    }

    private void UpdateCameraRotationGyro()
    {
        // early return: no gyro
        if (UnityEngine.InputSystem.Gyroscope.current == null) { return; }

        // early return: pause button held
        //if(pauseGyroscopeAction.IsPressed()){ return; }

        //Vector3 gyroValue = gyroscopeAction.ReadValue<Vector3>();

        //
        if (UnityEngine.InputSystem.Gyroscope.current == null) { return; }
        if (UnityEngine.InputSystem.Gyroscope.current.angularVelocity == null) { return; }
        Vector3 gyro1 = UnityEngine.InputSystem.Gyroscope.current.angularVelocity.ReadValue();
        if (gyro1 == null) { return; }

        // polarity
        // depends on if screen is held or not
        //int polarity = (pauseGyroscopeAction.IsPressed() ? -1 : 1);

        //
        this.yaw += this.lookSpeedH * gyro1.y * -1;
        this.pitch -= this.lookSpeedV * gyro1.x;

        //
        this.transform.eulerAngles = new Vector3(this.pitch, this.yaw, 0f);

        // attitude is apparently rotation, especially applicable for an android device
        //Console.WriteLine(AttitudeSensor.current.attitude.ReadValue());
    }

    private void UpdateCameraPosition()
    {
        // get from input system
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        Vector2 moveUpDownValue = moveUpDownAction.ReadValue<Vector2>();

        // move left with a / d
        transform.Translate(moveValue.x * Time.deltaTime * moveSpeed, 0, 0, Space.Self);

        // move forward with w / s
        transform.Translate(0, 0, moveValue.y * Time.deltaTime * moveSpeed, Space.Self);

        // move up with e / q
        transform.Translate(0, moveUpDownValue.y * Time.deltaTime * moveSpeed, 0, Space.Self);
    }

    private void UpdateHUD()
    {
        // early return
        if (uiText == null) { return; }
        if (controller == null) { uiText.text = "no controller"; return; }

        //
        resultString = "";

        //
        resultString += "controller layout: " + controller.layout.ToString() + "\n";
        resultString += "\n";
        resultString += "both enabled: " + (gyroEnabled ? "Y" : "N") + "/" + (accelEnabled ? "Y" : "N") + "\n";
        resultString += "\n";
        resultString += "gyro vec3" + "\n";
        resultString += gyroVec3.x.ToString("n2") + " | " + gyroVec3.y.ToString("n2") + " | " + gyroVec3.z.ToString("n2") + "\n";
        resultString += "\n";
        resultString += "gyro" + "\n";
        resultString += gyro.eulerAngles.x.ToString("n2") + " | " + gyro.eulerAngles.y.ToString("n2") + " | " + gyro.eulerAngles.z.ToString("n2") + "\n";
        resultString += "\n";
        resultString += "accel" + "\n";
        resultString += accel.x.ToString("n2") + " | " + accel.y.ToString("n2") + " | " + accel.z.ToString("n2") + "\n";

        //
        uiText.text = resultString;
    }
}