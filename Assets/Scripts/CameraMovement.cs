using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // For sensitivity control
    [Header("Sensitivity")]
    [SerializeField] float xSensitivity;
    [SerializeField] float ySensitivity;
    [SerializeField] float zoomSensitivity;

    // Use separate rig to rotate in the second direction
    [SerializeField] Transform rig;

    void Update()
    {
        // Get the amount to rotate in both directions
        float angleA = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
        float angleB = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
        // Get zoom amount
        float zoom = Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity * Time.deltaTime;
        // Check if any of the movements are valid
        bool[] validTurns = TestInput(angleA, angleB, zoom);
        // Rotate if valid
        if (validTurns[0] && Input.GetMouseButton(2))
        {
            transform.Rotate(Vector3.up, angleA);
        }
        if (validTurns[1] && Input.GetMouseButton(2))
        {
            rig.Rotate(Vector3.right, angleB);
        }
        if (validTurns[2])
        {
            // Get local position of the camera
            Vector3 pos = Camera.main.transform.localPosition;
            // Change local position of the camera
            pos.z -= zoom;
            Camera.main.transform.localPosition = pos;
        }
    }

    // Test if the rotation and zoom is valid
    bool[] TestInput(float angleA, float angleB, float radius)
    {
        // Predict rotations
        float newAngleA = transform.rotation.eulerAngles.y + angleA;
        float newAngleB = rig.rotation.eulerAngles.x + angleB;
        float newRadius = Camera.main.transform.localPosition.z - radius;
        // Check validity and return all values
        return new bool[]{newAngleA > 5 && newAngleA < 85, newAngleB < 355 && newAngleB > 275, newRadius > 5 && newRadius < 27};
    }
}