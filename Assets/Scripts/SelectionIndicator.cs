using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    // Define materials for arrows
    [Header("ArrowMaterials")]
    [SerializeField] Material ArrowNormal;
    [SerializeField] Material ArrowHover;
    [SerializeField] Material ArrowDrag;

    // Define arrow scale
    [Header("Scaling")]
    [SerializeField] float ArrowScale;

    // Define indicators for when planes are selected
    [Header("Planes")]
    [SerializeField] GameObject[] planeSelectionIndicators;

    // Variables for instances of classes
    private ObjectSelecter S;
    private ObjectMover M;
    private MeshRenderer[] arrows;
    private GameObject MovementUI;

    // Determine how the object moves
    public int SelectedMovementDir = 0;

    void Start()
    {
        // Define all instances
        S = FindObjectOfType<ObjectSelecter>();
        M = FindObjectOfType<ObjectMover>();
        MovementUI = GameObject.FindGameObjectWithTag("MovementArrows");
        arrows = MovementUI.GetComponentsInChildren<MeshRenderer>();
        // Disable arrows
        MovementUI.SetActive(false);
    }

    void Update()
    {
        // Check if an object is selected
        if (S.selectedObject != null)
        {
            // Get bounding box of the object
            Bounds bounds = new Bounds(S.selectedObject.transform.position, Vector3.zero);
            MeshRenderer[] renderers = S.selectedObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }
            // Determine position of the arrows
            Vector3 selectedPos = bounds.max;
            selectedPos.y = 0;
            MovementUI.transform.position = selectedPos;
            MovementUI.transform.localScale = Vector3.one * Vector3.Distance(Camera.main.transform.position, selectedPos) * ArrowScale;
            // Enable arrows
            MovementUI.SetActive(true);
        } 
        else
        {
            // Disable arrows
            MovementUI.SetActive(false);
        }
        // Activate or deactivate the plane selection indicators
        for (int i = 0; i < planeSelectionIndicators.Length; i++)
        {
            planeSelectionIndicators[i].SetActive(planeSelectionIndicators[i].transform.root.gameObject == S.selectedObject);
        }
        // Choose direction in which the object should be moved or rotated
        SelectedMovementDir = CheckForOnHover();
        // Check if mouse button is being pressed
        if (Input.GetMouseButton(0))
        {
            // Create empty string;
            string name = "";
            // Get the name of the currently clicked arrow
            switch (M.MoveDir)
            {
                case 1: name = "ArrowX";    break;
                case 2: name = "ArrowZ";    break;
                case 3: name = "ArrowXZ";   break;
                case 4: name = "ArrowRot";  break;
                default:                    break;
            }
            // Change the material of the clicked arrow
            for (int i = 0; i < arrows.Length; i++)
            {
                if (arrows[i].gameObject.name == name)
                {
                    arrows[i].material = ArrowDrag;
                }
            }
        }
    }

    // Returns the movement state
    public int CheckForOnHover()
    {
        // Check if an object is selected
        if (S.selectedObject != null)
        {
            // Set all arrow materials to the default
            foreach (var a in arrows)
            {
                a.material = ArrowNormal;
            }
            // Cast ray to mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 100);
            // Check if object is hit
            if (hits.Length > 0)
            {
                // Search for arrow
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].transform.gameObject.CompareTag("MovementArrows"))
                    {
                        // Change the material of the current arrow being hovered over
                        MeshRenderer[] meshes = hits[i].transform.gameObject.GetComponents<MeshRenderer>();
                        foreach (var m in meshes)
                        {
                            m.material = ArrowHover;
                        }
                        // Return the movement state
                        switch (hits[i].transform.gameObject.name)
                        {
                            case "ArrowX": return 1;
                            case "ArrowZ": return 2;
                            case "ArrowXZ": return 3;
                            case "ArrowRot": return 4;
                            default: break;
                        }
                    }
                }
            }
        }
        // Return 0 if no movement occurs
        return 0;
    }
}