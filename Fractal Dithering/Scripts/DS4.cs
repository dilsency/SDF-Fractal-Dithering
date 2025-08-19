//DS4.cs
using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DS4
{
    // Gyroscope
    public static ButtonControl gyroX = null;
    public static ButtonControl gyroY = null;
    public static ButtonControl gyroZ = null;
    // Accelerometer
    public static ButtonControl accelX = null;
    public static ButtonControl accelY = null;
    public static ButtonControl accelZ = null;

    public static Gamepad controller = null;

    private static int startIndex = 14;// 13 or 14 ONLY

    public static Gamepad getController(string layoutFile = null)
    {
        int fileIndex = 2;
        string fileName = "";
        if (fileIndex == 0)
        {
            fileName = "DualShockGamepad Custom.json";
        }
        else if (fileIndex == 1)
        {
            fileName = "DualShock4GamepadHID Custom.json";
        }
        else if (fileIndex == 2)
        {
            fileName = "DualShock4GamepadHID Custom 2.json";
        }

        var ds4 = Gamepad.current;
        if (ds4 == null) { return null; }


        // Read layout from JSON file
        string layout = File.ReadAllText(layoutFile == null ? ("Assets/JSON/" + fileName) : layoutFile);

        // Overwrite the default layout
        InputSystem.RegisterLayoutOverride(layout, "DualShock4GamepadHIDCustom");


        DS4.controller = ds4;
        bindControls(DS4.controller);

        Debug.Log(DS4.controller.layout);

        return DS4.controller;
    }

    private static void bindControls(Gamepad ds4)
    {
        gyroX = ds4.GetChildControl<ButtonControl>("gyro X " + (startIndex + 2 * 0).ToString());
        gyroY = ds4.GetChildControl<ButtonControl>("gyro Y " + (startIndex + 2 * 1).ToString());
        gyroZ = ds4.GetChildControl<ButtonControl>("gyro Z " + (startIndex + 2 * 2).ToString());

        accelX = ds4.GetChildControl<ButtonControl>("accel X " + (startIndex + 2 * 3).ToString());
        accelY = ds4.GetChildControl<ButtonControl>("accel Y " + (startIndex + 2 * 4).ToString());
        accelZ = ds4.GetChildControl<ButtonControl>("accel Z " + (startIndex + 2 * 5).ToString());
    }

    public static Vector3 getGyroVec3(float scale = 1)
    {
        float gyroXProcessed = processRawData(gyroX.ReadValue()) * scale;
        float gyroYProcessed = processRawData(gyroY.ReadValue()) * scale;
        float gyroZProcessed = -processRawData(gyroZ.ReadValue()) * scale;

        return new Vector3(gyroXProcessed, gyroYProcessed, gyroZProcessed);
    }

    public static Quaternion getGyro(float scale = 1)
    {
        float gyroXProcessed = processRawData(gyroX.ReadValue()) * scale;
        float gyroYProcessed = processRawData(gyroY.ReadValue()) * scale;
        float gyroZProcessed = -processRawData(gyroZ.ReadValue()) * scale;

        return Quaternion.Euler(gyroXProcessed, gyroYProcessed, gyroZProcessed);
    }

    public static Vector3 getAccel(float scale = 1)
    {
        float accelXProcessed = processRawDataAccel(accelX.ReadValue()) * scale;
        float accelYProcessed = processRawDataAccel(accelY.ReadValue()) * scale;
        float accelZProcessed = -processRawDataAccel(accelZ.ReadValue()) * scale;

        return new Vector3(accelXProcessed, accelYProcessed, accelZProcessed);
    }

    private static float processRawData(float data)
    {
        return data > 0.5 ? 1 - data : -data;
    }

    private static float processRawDataAccel(float data)
    {
        return data;
    }
}