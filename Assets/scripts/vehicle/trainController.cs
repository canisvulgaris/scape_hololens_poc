﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class trainController : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have

    public Color baseWheelColor;

    public Vector3 resetForce;
    public Vector3 resetTorque;

    public float brakeForce = 100.0f;

    private float colorInc;
    private bool forward = false;
    private bool reverse = false;
    //private bool acceleration = false;
    private bool braking = false;

    private GameObject terrainControllerObject;
    private TerrainController terrainController;

    void Start()
    {
        terrainControllerObject = GameObject.Find("TerrainController");
        terrainController = (TerrainController)terrainControllerObject.GetComponent<TerrainController>();
    }


    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider, GameObject wheelObject)
    {
        if (wheelObject == null)
        {
            Debug.LogError("wheelObject not found");
            return;
        }

        Transform visualWheel = wheelObject.transform;

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    public void FixedUpdate()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vehicle Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Vehicle Horizontal");

        Vector3 currentVelocity = gameObject.GetComponent<Rigidbody>().velocity;
        Vector3 localVelocity = transform.InverseTransformDirection(currentVelocity);

        //Debug.Log("motor: " + motor + " - localVelocity: " + localVelocity);

        if (localVelocity.z > 0 )
        {
            forward = true;
        }
        else
        {
            forward = false;
        }

        if (localVelocity.z < 0)
        {
            reverse = true;
        }
        else
        {
            reverse = false;
        }

        if (motor < 0 && forward == true)
        {
            braking = true;
        }
        else if (motor > 0 && reverse == true)
        {
            braking = true;
        }
        else
        {
            braking = false;
        }

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            else
            {
                axleInfo.leftWheel.motorTorque = 0;
                axleInfo.rightWheel.motorTorque = 0;
            }

            if (braking)
            {
                axleInfo.leftWheel.brakeTorque = brakeForce;
                axleInfo.rightWheel.brakeTorque = brakeForce;
            }
            else
            {
                axleInfo.leftWheel.brakeTorque = 0;
                axleInfo.rightWheel.brakeTorque = 0;
            }
            ApplyLocalPositionToVisuals(axleInfo.leftWheel, axleInfo.leftWheelObject);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel, axleInfo.rightWheelObject);

            //float leftWheelRotationCurrent = axleInfo.leftWheelObject.transform.eulerAngles.x;
            //float leftWheelRotationSpeed  = 0.0f;

            //if (axleInfo.leftWheelRotation > leftWheelRotationCurrent)
            //{
            //    leftWheelRotationSpeed = (360.0f + leftWheelRotationCurrent - axleInfo.leftWheelRotation) / Time.deltaTime;
            //}
            //else
            //{
            //    leftWheelRotationSpeed = (leftWheelRotationCurrent - axleInfo.leftWheelRotation) / Time.deltaTime;
            //}

            //axleInfo.leftWheelRotation = leftWheelRotationCurrent;

            //float rightWheelRotationCurrent = axleInfo.rightWheelObject.transform.eulerAngles.x;
            //float rightWheelRotationSpeed = 0.0f;

            //if (axleInfo.rightWheelRotation > rightWheelRotationCurrent)
            //{
            //    rightWheelRotationSpeed = (360.0f + rightWheelRotationCurrent - axleInfo.rightWheelRotation) / Time.deltaTime;
            //}
            //else
            //{
            //    rightWheelRotationSpeed = (rightWheelRotationCurrent - axleInfo.rightWheelRotation) / Time.deltaTime;
            //}

            //axleInfo.rightWheelRotation = rightWheelRotationCurrent;

            ////Debug.Log("leftWheelRotationSpeed: " + leftWheelRotationSpeed + " - rightWheelRotationSpeed: " + rightWheelRotationSpeed);
            //colorInc = (leftWheelRotationSpeed / 360.0f);
            //Debug.Log("left colorInc: " + colorInc);
            //axleInfo.leftWheelObject.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(baseWheelColor.r + colorInc, baseWheelColor.g + colorInc, baseWheelColor.b + colorInc);

            //colorInc = (rightWheelRotationSpeed / 360.0f);
            //Debug.Log("right colorInc: " + colorInc);
            //axleInfo.rightWheelObject.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(baseWheelColor.r + colorInc, baseWheelColor.g + colorInc, baseWheelColor.b + colorInc);
        }

        if (Input.GetButton("Reset"))
        {
            float terrainHeight = terrainController.getHeightAtVertex(transform.position);

            if (transform.position.y < terrainHeight + 20.0f)
            {
                transform.position += Vector3.up * 5* Time.deltaTime;
            }

            int arrayIndex = terrainController.getArrayIndex();

            Vector3 targetDir = new Vector3(arrayIndex/2.0f, terrainHeight, arrayIndex/2.0f) - transform.position;
            Vector3 newDir = Vector3.RotateTowards(transform.position, targetDir, Time.deltaTime, 0.1f);

            if (Vector3.Angle(targetDir, newDir) > 10.0f) {                
                transform.rotation = Quaternion.LookRotation(newDir);
            }
        }


    }
}