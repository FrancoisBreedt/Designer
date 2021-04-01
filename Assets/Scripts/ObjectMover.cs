using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    // Snap enable and interval settings
    [Header("Snap settings")]
    public int SnapSetting = 0;
    public float MovementSnap;
    public float RotationSnap;

    // Set the rotation sensitivity
    [SerializeField] float RotationSensitivity;

    // Variable for the instance of the ObjectSelecter class
    private ObjectSelecter S;

    // Variable specifies the object movement direction
    public int MoveDir = 0;
    // Store the position of the cursor relative to the selected object
    private Vector3 currentDifference = Vector3.zero;
    // If the mouse is dragged, this variable stores the initial position of the cursor
    private Vector2 clickScreencreenPos;
    // Remember the rotation of the selected object before it is rotated
    private Quaternion startRotation;

    void Start()
    {
        // Declare instance of the class
        S = FindObjectOfType<ObjectSelecter>();
    }

    void Update()
    {
        // Check if an object is selected
        if (S.selectedObject != null)
        {
            // Set the movement direction
            if (Input.GetMouseButtonDown(0))
            {
                MoveDir = FindObjectOfType<SelectionIndicator>().SelectedMovementDir;
            }
            // Stop moving if mouse button is released
            if (Input.GetMouseButtonUp(0))
            {
                MoveDir = 0;
            }
            // Move the object
            if (Input.GetMouseButton(0) && MoveDir != 0)
            {
                Move();
            }
        }
    }

    // Move the selected object
    void Move()
    {
        // Create new vector that will store the move data
        Vector3 movement = Vector3.zero;
        // Save current position of the selected object
        Vector3 currentPos = S.selectedObject.transform.position;
        // Cast ray through all objects at the cursor position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000);
        // Search for valid hit
        RaycastHit hit = new RaycastHit();
        bool hitFound = false;
        for (int i = 0; i < hits.Length; i++)
        {
            // If the raycast hit the ground, a valid hit is found
            if (hits[i].transform.gameObject.CompareTag("Ground"))
            {
                hit = hits[i];
                hitFound = true;
                break;
            }
        }
        // Run this code only if the cursor points at the ground
        if (hitFound)
        {
            // Set relative position and start rotation variables
            if (Input.GetMouseButtonDown(0))
            {
                currentDifference = hit.point - currentPos;
                clickScreencreenPos = Input.mousePosition;
                startRotation = S.selectedObject.transform.rotation;
            }
            // Set the movement
            movement = hit.point - currentDifference - currentPos;
            // Limit the movement based on the move direction
            if (MoveDir == 1)
            {
                movement.z = 0;
            }
            if (MoveDir == 2)
            {
                movement.x = 0;
            }
            // Set the movement to 0 if the object needs to be rotated
            if (MoveDir == 4)
            {
                movement = Vector3.zero;
            }
        }
        // Snap the object if snap is enabled
        if (SnapSetting == 1)
        {
            movement.x = Mathf.Round((currentPos.x + movement.x) / MovementSnap) * MovementSnap - currentPos.x;
            movement.z = Mathf.Round((currentPos.z + movement.z) / MovementSnap) * MovementSnap - currentPos.z;
        }
        // Get the mesh renderer and the bounds of the selected object for future use
        Bounds b = new Bounds(S.selectedObject.transform.position, Vector3.zero);
        MeshRenderer[] renderers = S.selectedObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            b.Encapsulate(r.bounds);
        }
        // Limit the movement based on the walls
        movement.x = b.min.x + movement.x > -5 ? movement.x : 0;
        movement.z = b.min.z + movement.z > -5 ? movement.z : 0;
        // Move the object
        S.selectedObject.transform.position = currentPos + movement;
        // Check if object needs to rotate
        if (MoveDir == 4)
        {
            // Get the rotation value
            float rotation = RotationSensitivity * (Input.mousePosition.x - clickScreencreenPos.x) - startRotation.eulerAngles.y;
            // Snap the rotation if snap is enabled
            if (SnapSetting == 1)
            {
                rotation = Mathf.Round(rotation / RotationSnap) * RotationSnap;
            }
            // Rotate the object
            S.selectedObject.transform.rotation = Quaternion.Euler(Vector3.down * rotation);
        }
        // Move the object out of the walls if it has rotated into the walls
        if (b.min.x < -5)
        {
            S.selectedObject.transform.position += Vector3.right * (-5 - b.min.x);
        }
        if (b.min.z < -5)
        {
            S.selectedObject.transform.position += Vector3.forward * (-5 - b.min.z);
        }
    }
}