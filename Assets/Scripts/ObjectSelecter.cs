using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectSelecter : MonoBehaviour
{
    // Variable for the selected object
    public GameObject selectedObject = null;

    void Update()
    {
        // Check if mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast to cursor position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Make sure that the hit object is not UI or movement arrows
                if (!EventSystem.current.IsPointerOverGameObject() && !hit.transform.gameObject.CompareTag("MovementArrows"))
                {
                    // Set the selected object variable
                    selectedObject = hit.transform.root.gameObject;
                }
            } 
        }
        // Deselect the selected object
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            selectedObject = null;
        }
    }
}