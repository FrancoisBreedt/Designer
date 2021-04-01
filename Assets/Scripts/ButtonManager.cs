using HSVPicker;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SimpleFileBrowser.FileBrowser;

public class ButtonManager : MonoBehaviour
{
    // Prefabs of placeable objects
    [SerializeField] GameObject[] ObjectPrefabs;
    // Plane meshrenderers
    [SerializeField] MeshRenderer[] Planes;
    // Sprites for the snap toggle button
    [SerializeField] Sprite[] SnapSprites;
    // Snap settings input fields
    [SerializeField] InputField MovementSnap;
    [SerializeField] InputField RotationSnap;

    // Object in which data about the placed objects are temporarily stored
    public ObjectData currentObjects;

    // Will contain instances of classes
    private ObjectMover M;
    private ObjectSelecter S;
    private ColorPicker ColorPicker;
    // Additional panels to show and hide
    private GameObject RemovePanel;
    private GameObject AddPanel;

    // Contains all the message boxes
    private readonly List<GameObject> MessageBoxes = new List<GameObject>();
    // Contains previous actions for the undo function
    private readonly List<string> PreviousActions = new List<string>();
    // Contains future actions for the redo button
    private readonly List<string> NextActions = new List<string>();

    // Used to check when user stops moving an object
    private bool movementListener = false;

    void Start()
    {
        // Set to instances of the classes
        M = FindObjectOfType<ObjectMover>();
        S = FindObjectOfType<ObjectSelecter>();
        ColorPicker = FindObjectOfType<ColorPicker>();
        // Disable the color picker
        ColorPicker.gameObject.SetActive(false);
        // Add the current layout to the list of previous actions
        AddLayout();
        // Find respective objects by name
        RectTransform[] r = transform.parent.gameObject.GetComponentsInChildren<RectTransform>();
        for (int i = 0; i < r.Length; i++)
        {
            if (r[i].gameObject.name == "Remove")
            {
                RemovePanel = r[i].transform.parent.gameObject;
            }
            if (r[i].gameObject.name == "ObjectAdd")
            {
                AddPanel = r[i].transform.gameObject;
                // Hide panel
                AddPanel.SetActive(false);
            }
            if (r[i].gameObject.name == "MessageBoxNew" || r[i].gameObject.name == "MessageBoxOpen" || r[i].gameObject.name == "MessageBoxExit")
            {
                MessageBoxes.Add(r[i].transform.gameObject);
                // Hide messages
                r[i].gameObject.SetActive(false);
            }
        }
        // Set the text of the snapping input boxes
        MovementSnap.text = M.MovementSnap.ToString();
        RotationSnap.text = M.RotationSnap.ToString();
    }

    void Update()
    {
        // Check for movement of an object
        CheckForObjectMovement();
        // Enable remove panel if an object is selected
        RemovePanel.SetActive(S.selectedObject != null && S.selectedObject.CompareTag("Selectable"));
        // Show snap input boxes if snapping is on
        MovementSnap.gameObject.SetActive(M.SnapSetting == 1);
        RotationSnap.gameObject.SetActive(M.SnapSetting == 1);
        // Hide color picker if no object is selected
        ColorPicker.gameObject.SetActive(ColorPicker.gameObject.activeSelf && S.selectedObject != null);
    }

    // When text in the movement snapping input box is entered
    public void EditMovementSnap()
    {
        // Set the movement snap if a valid value is entered
        if (float.TryParse(MovementSnap.text, out float snap))
        {
            M.MovementSnap = snap;
        }
        else
        {
            MovementSnap.text = M.MovementSnap.ToString();
        }
    }

    // When text in the rotation snapping input box is entered
    public void EditRotationSnap()
    {
        // Set the rotation snap if a valid value is entered
        if (float.TryParse(RotationSnap.text, out float snap))
        {
            M.RotationSnap = snap;
        }
        else
        {
            RotationSnap.text = M.RotationSnap.ToString();
        }
    }

    // Open the object add panel if the user wants to add an object
    public void AddObject()
    {
        AddPanel.SetActive(true);
    }

    // Add an object into the layout
    public void Add(int index)
    {
        // Check if object exists
        if (index < ObjectPrefabs.Length)
        {
            // Add the object and change the name in order to simplify exporting
            GameObject G = Instantiate(ObjectPrefabs[index], Vector3.zero, Quaternion.identity);
            G.name = ObjectPrefabs[index].name;
        }
        // Close the panel
        AddPanel.SetActive(false);
    }

    // Remove an object from the layout
    public void RemoveObject()
    {
        Destroy(S.selectedObject);
    }

    // Toggle the snap setting
    public void ToggleSnap()
    {
        // Change snap setting
        M.SnapSetting = ++M.SnapSetting % SnapSprites.Length;
        // Get the image that needs to be changed
        Image[] images = transform.parent.gameObject.GetComponentsInChildren<Image>();
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].gameObject.name == "Snap")
            {
                // Change the image
                images[i].sprite = SnapSprites[M.SnapSetting];
            }
        }
    }

    // When the user wants to exit the program
    public void Exit(bool check)
    {
        if (check)
        {
            // Prompt the user to export first
            MessageBoxes[2].SetActive(true);
        }
        else
        {
            // Exit application
            Application.Quit();
        }
    }

    // Close given window
    public void CloseWindow(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    // Enable or disable the color picker
    public void ToggleColour()
    {
        ColorPicker.gameObject.SetActive(!ColorPicker.gameObject.activeSelf);
    }

    // Change the colour of the selected object
    public void ChangeColor(Color c)
    {
        // Check if S exists and has a selected object
        if (S != null && S.selectedObject != null)
        {
            // Change all the materials' colors
            MeshRenderer[] meshes = S.selectedObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var m in meshes)
            {
                m.material.color = c;
            }
        }
    }

    // Open new layout
    public void NewFile(bool check)
    {
        if (check)
        {
            // Prompt user to export first
            MessageBoxes[0].SetActive(true);
        }
        else
        {
            // Clear object data
            currentObjects = new ObjectData();
            // Clear actions
            PreviousActions.Clear();
            NextActions.Clear();
            // Load the data
            SetSceneObjects(currentObjects);
        }
    }

    // Import layout
    public void ImportFile(bool check)
    {
        if (check)
        {
            // Prompt user to export
            MessageBoxes[1].SetActive(true);
        }
        else
        {
            // Create delegate functions
            OnSuccess onSuccess = new OnSuccess(LoadFile);
            OnCancel onCancel = new OnCancel(Cancelled);
            // Set pick mode
            PickMode pickMode = PickMode.Files;
            // Set filter
            Filter filter = new Filter("*", ".layout");
            SetFilters(false, filter);
            // Open load dialog
            ShowLoadDialog(onSuccess, onCancel, pickMode, false, null, null, "Choose File To Import", "Import");
        }
}
    // Export layout
    public void ExportFile()
    {
        // Create delegate functions
        OnSuccess onSuccess = new OnSuccess(SaveFile);
        OnCancel onCancel = new OnCancel(Cancelled);
        // Set pick mode
        PickMode pickMode = PickMode.Files;
        // Set filter
        Filter filter = new Filter("*", ".layout");
        SetFilters(false, filter);
        // Open save dialog
        ShowSaveDialog(onSuccess, onCancel, pickMode, false, null, null, "Choose Export Location", "Export");
    }

    // Load layout file at selected location
    private void LoadFile(string[] strings)
    {
        currentObjects = JsonUtility.FromJson<ObjectData>(System.IO.File.ReadAllText(strings[0]));
        SetSceneObjects(currentObjects);
    }

    // Save the layout in preferred location
    private void SaveFile(string[] strings)
    {
        AddLayout();
        System.IO.File.WriteAllText(strings[0], JsonUtility.ToJson(currentObjects));
    }

    // This is called when user cancels
    private void Cancelled() { }

    // Add current layout to action lists
    public void AddLayout(string L = "Previous")
    {
        // Create lists to store values
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Selectable");
        List<int> objectTypes = new List<int>();
        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();
        List<Color> colors = new List<Color>();
        List<Color> planeColors = new List<Color>();
        // Add all the data to the ObjectData object
        foreach (var g in gameObjects)
        {
            for (int i = 0; i < ObjectPrefabs.Length; i++)
            {
                if (g.name == ObjectPrefabs[i].name)
                {
                    objectTypes.Add(i);
                    break;
                }
            }
            positions.Add(g.transform.position);
            rotations.Add(g.transform.rotation);
            colors.Add(g.GetComponentInChildren<MeshRenderer>().material.color);
        }
        foreach (var m in Planes)
        {
            planeColors.Add(m.material.color);
        }
        currentObjects.objectTypes = objectTypes.ToArray();
        currentObjects.positions = positions.ToArray();
        currentObjects.rotations = rotations.ToArray();
        currentObjects.colors = colors.ToArray();
        currentObjects.planeColors = planeColors.ToArray();
        // The default argument adds the data to the previous actions
        if (L == "Previous")
        {
            PreviousActions.Add(JsonUtility.ToJson(currentObjects));
        }
        // Any other argument adds the data to the next actions
        else
        {
            NextActions.Add(JsonUtility.ToJson(currentObjects));
        }
    }

    // Undo action
    public void Undo()
    {
        // Check if there are any previous actions
        if (PreviousActions.Count > 0)
        {
            // Add current layout to the next actions
            AddLayout("Next");
            // Get layout data from the previous list
            currentObjects = JsonUtility.FromJson<ObjectData>(PreviousActions[PreviousActions.Count - 1]);
            // Remove the item from the list
            PreviousActions.RemoveAt(PreviousActions.Count - 1);
            // Set the objects according to the data
            SetSceneObjects(currentObjects);
        }
    }

    // Redo an action
    public void Redo()
    {
        // Check if the list contains data
        if (NextActions.Count > 0)
        {
            // Add current layout to previous list
            AddLayout();
            // Get layout data from the next list
            currentObjects = JsonUtility.FromJson<ObjectData>(NextActions[NextActions.Count - 1]);
            // Remove the item from the list
            NextActions.RemoveAt(NextActions.Count - 1);
            // Set the scene objects according to the data
            SetSceneObjects(currentObjects);
        }
    }

    // Set the layout according to the given data
    public void SetSceneObjects(ObjectData objectData)
    {
        // Destroy all current objects
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Selectable");
        foreach (var g in gameObjects)
        {
            Destroy(g);
        }
        // Check if the field exists and has values
        if (objectData.objectTypes != null && objectData.objectTypes.Length > 0)
        {
            // Add the objects to the layout
            for (int i = 0; i < objectData.objectTypes.Length; i++)
            {
                GameObject G = Instantiate(ObjectPrefabs[objectData.objectTypes[i]], objectData.positions[i], objectData.rotations[i]);
                G.GetComponentInChildren<MeshRenderer>().material.color = objectData.colors[i];
                G.name = ObjectPrefabs[objectData.objectTypes[i]].name;
            }
        }
        // Set the plane colors
        for (int i = 0; i < Planes.Length; i++)
        {
            Planes[i].material.color = objectData.planeColors[i];
        }
    }

    // Check if user has moved an object
    private void CheckForObjectMovement()
    {
        // Add layout to previous actions if the movement state changes to 0
        if (M.MoveDir != 0)
        {
            movementListener = true;
        }
        else if (movementListener)
        {
            AddLayout();
            movementListener = false;
        }
    }

    // Class for storing layout data
    [Serializable]
    public class ObjectData {
        public int[] objectTypes;
        public Vector3[] positions;
        public Quaternion[] rotations;
        public Color[] colors;
        public Color[] planeColors;
    }

}