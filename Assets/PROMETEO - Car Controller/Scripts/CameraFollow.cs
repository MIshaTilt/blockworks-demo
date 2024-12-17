using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform carTransform;
    [Range(1, 10)]
    public float followSpeed = 2;
    [Range(1, 10)]
    public float lookSpeed = 5;

    private Vector3 initialCameraOffset;

    void Start()
    {
        // Calculate the initial offset between the camera and the car
        initialCameraOffset = transform.position - carTransform.position;
    }

    void FixedUpdate()
    {
        //FollowCar();
        LookAtCar();
    }

    private void FollowCar()
    {
        // Calculate the target position for the camera
        Vector3 targetPosition = carTransform.position + initialCameraOffset;
        // Smoothly interpolate the camera's position towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    private void LookAtCar()
    {
        // Calculate the direction from the c   amera to the car
        //Vector3 lookDirection = carTransform.position - transform.position;
        // Calculate the target rotation for the camera to look at the car
        //Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        // Smoothly interpolate the camera's rotation towards the target rotation
        float _rotateX = Mathf.Lerp(transform.rotation.x, transform.rotation.x + carTransform.rotation.x, lookSpeed * Time.deltaTime); 

        transform.Rotate(_rotateX, transform.rotation.y, transform.rotation.z, Space.Self);
    }   

}
