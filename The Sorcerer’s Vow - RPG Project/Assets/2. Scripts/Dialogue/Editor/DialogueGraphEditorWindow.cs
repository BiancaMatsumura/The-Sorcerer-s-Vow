using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView; // Namespace for GraphView
using UnityEditor.UIElements; // Namespace for Editor-specific UI elements like ObjectField
using System;
using System.Linq; // Namespace for LINQ methods like Any(), Where(), FirstOrDefault()
using System.IO; // Namespace for Path and File operations

/// <summary>
/// The main Editor Window for the Dialogue Graph system.
/// Hosts the DialogueGraphView and provides toolbar controls for asset management and view manipulation.
/// </summary>
public class DialogueGraphEditorWindow : EditorWindow
{
    // The visual graph area
    private DialogueGraphView _graphView;
    // The currently loaded DialogueDataSO asset
    private DialogueDataSO _currentAsset;
    // Reference to the ObjectField in the toolbar for direct asset selection
    private ObjectField _assetObjectField;

    // Path to the USS stylesheet. ADJUST THIS to match your project structure.
    private const string USS_PATH = "Assets/2. Scripts/Dialogue/Editor/DialogueGraphStyle.uss";

    /// <summary>
    /// Unity menu item to open this editor window.
    /// </summary>
    [MenuItem("Graph/Dialogue Graph Editor")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraphEditorWindow>();
        window.titleContent = new GUIContent("Dialogue Graph");
        window.minSize = new Vector2(600, 400); // Set a minimum reasonable size
    }

    /// <summary>
    /// Called when the window is enabled (opened or re-focused).
    /// Sets up the window's contents.
    /// </summary>
    private void OnEnable()
    {
        ConstructGraphView();   // Create the graph view element
        GenerateToolbar();      // Create the toolbar element
        LoadStyles();           // Load and apply USS styles
        OnSelectionChange();    // Load initially selected asset (if any)
    }

    /// <summary>
    /// Called when the window is disabled (closed).
    /// Cleans up by removing the graph view.
    /// </summary>
    private void OnDisable()
    {
        // Remove graph view from the root to prevent potential memory leaks
        if (rootVisualElement != null && _graphView != null)
        {
            rootVisualElement.Remove(_graphView);
        }
    }

    /// <summary>
    /// Creates and adds the DialogueGraphView to the window.
    /// </summary>
    private void ConstructGraphView()
    {
        // Instantiate the graph view, passing a reference to this window
        _graphView = new DialogueGraphView(this) { name = "Dialogue Graph" };
        // Make the graph view fill the entire window area
        _graphView.StretchToParentSize();
        // Add the graph view to the window's root visual element
        rootVisualElement.Add(_graphView);
    }

    /// <summary>
    /// Loads the USS stylesheet from the defined path and applies it to the GraphView.
    /// </summary>
    private void LoadStyles()
    {
        if (_graphView == null) // Safety check
        {
            Debug.LogError("Cannot load styles: GraphView is null.");
            return;
        }

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);
        if (styleSheet != null)
        {
            _graphView.styleSheets.Add(styleSheet); // Apply styles to the graph view
            // Debug.Log($"Dialogue Graph StyleSheet loaded successfully from: {USS_PATH}");
        }
        else
        {
            Debug.LogError($"Failed to load StyleSheet at path: '{USS_PATH}'. Ensure the path is correct and the file exists.");
        }
    }

    /// <summary>
    /// Creates and configures the toolbar at the top of the window.
    /// </summary>
    private void GenerateToolbar()
    {
        var toolbar = new UnityEditor.UIElements.Toolbar();

        // --- Dialogue Asset Selection Field ---
        _assetObjectField = new ObjectField("Dialogue Asset:")
        {
            name = "DialogueAssetField", // Assign a name for easier querying
            objectType = typeof(DialogueDataSO),
            allowSceneObjects = false,
            style = { flexGrow = 1, maxWidth = 350 } // Allow growth but limit max width
        };
        // Callback when the value in the ObjectField changes
        _assetObjectField.RegisterValueChangedCallback(evt =>
        {
            DialogueDataSO selectedValue = evt.newValue as DialogueDataSO;
            if (_currentAsset != selectedValue) // Only act if the value actually changed
            {
                _currentAsset = selectedValue; // Update internal reference

                if (_currentAsset != null)
                {
                    // Update Project window selection for consistency, avoiding loops
                    if (Selection.activeObject != _currentAsset)
                    {
                        Selection.activeObject = _currentAsset;
                        // OnSelectionChange will handle populating the view
                    }
                    else
                    {
                        // If it was already selected, force populate
                        _graphView?.PopulateView(_currentAsset);
                    }
                    EditorGUIUtility.PingObject(_currentAsset); // Highlight in Project window
                }
                else // ObjectField was cleared
                {
                    // Clear Project window selection if it was a dialogue asset
                    if (Selection.activeObject is DialogueDataSO) { Selection.activeObject = null; }
                    _graphView?.PopulateView(null); // Clear the graph view
                }
                UpdateToolbarState(); // Update button enable states
            }
        });
        toolbar.Add(_assetObjectField);

        toolbar.Add(new Label(" | ")); // Visual separator

        // --- Save Button ---
        var saveButton = new Button(() => RequestDataOperation(true)) { text = "Save", name = "SaveButton", tooltip = "Save changes (Ctrl+S)" };
        toolbar.Add(saveButton);

        // --- New Button ---
        var createButton = new Button(() => CreateNewDialogueAsset()) { text = "New", name = "CreateButton", tooltip = "Create New Dialogue Tree Asset" };
        toolbar.Add(createButton);

        toolbar.Add(new Label(" | ")); // Visual separator

        // --- Frame Buttons ---
        var frameAllButton = new Button(() => _graphView?.FrameAll()) { text = "Fit All", name = "FitAllButton", tooltip = "Fit entire graph" };
        toolbar.Add(frameAllButton);
        var frameSelectedButton = new Button(() => _graphView?.FrameSelection()) { text = "Fit Sel.", name = "FitSelectedButton", tooltip = "Fit selected nodes" };
        toolbar.Add(frameSelectedButton);

        // --- Zoom Buttons ---
        toolbar.Add(new Label(" | ")); // Visual separator
        var zoomInButton = new Button(() => _graphView?.ZoomIn()) { text = "+", name = "ZoomInButton", tooltip = "Zoom In" };
        toolbar.Add(zoomInButton);
        var zoomOutButton = new Button(() => _graphView?.ZoomOut()) { text = "-", name = "ZoomOutButton", tooltip = "Zoom Out" };
        toolbar.Add(zoomOutButton);

        // --- Toolbar State Update Logic ---
        // Periodically check and update the enabled state of toolbar buttons
        rootVisualElement.schedule.Execute(() => UpdateToolbarState(saveButton, createButton, frameAllButton, frameSelectedButton, zoomInButton, zoomOutButton)).Every(150);

        // Add the configured toolbar to the window's root
        rootVisualElement.Add(toolbar);
    }

    /// <summary>
    /// Updates the enabled/disabled state of the toolbar buttons based on the current context.
    /// </summary>
    private void UpdateToolbarState(Button saveButton = null, Button createButton = null,
                                    Button frameAllButton = null, Button frameSelectedButton = null,
                                    Button zoomInButton = null, Button zoomOutButton = null)
    {
        // Find buttons by name if references weren't passed (e.g., first scheduled call)
        saveButton ??= rootVisualElement?.Q<Button>("SaveButton");
        createButton ??= rootVisualElement?.Q<Button>("CreateButton");
        frameAllButton ??= rootVisualElement?.Q<Button>("FitAllButton");
        frameSelectedButton ??= rootVisualElement?.Q<Button>("FitSelectedButton");
        zoomInButton ??= rootVisualElement?.Q<Button>("ZoomInButton");
        zoomOutButton ??= rootVisualElement?.Q<Button>("ZoomOutButton");

        // Fallback to searching by text if names weren't found (less reliable)
        if (frameAllButton == null) frameAllButton = rootVisualElement?.Query<Button>().Where(b => b.text == "Fit All").ToList().FirstOrDefault();
        if (frameSelectedButton == null) frameSelectedButton = rootVisualElement?.Query<Button>().Where(b => b.text == "Fit Sel.").ToList().FirstOrDefault();
        if (zoomInButton == null) zoomInButton = rootVisualElement?.Query<Button>().Where(b => b.text == "+").ToList().FirstOrDefault();
        if (zoomOutButton == null) zoomOutButton = rootVisualElement?.Query<Button>().Where(b => b.text == "-").ToList().FirstOrDefault();

        // Determine current state
        bool isAssetLoaded = _currentAsset != null;
        bool graphExists = _graphView != null;
        bool hasSelection = graphExists && _graphView.selection.Any(); // Check selection only if graph exists

        // Update enabled state of each button
        saveButton?.SetEnabled(isAssetLoaded);                  // Can save only if asset is loaded
        createButton?.SetEnabled(!isAssetLoaded);               // Can create only if no asset is loaded
        frameAllButton?.SetEnabled(graphExists);                // Can frame all if graph exists
        frameSelectedButton?.SetEnabled(graphExists && hasSelection); // Can frame selection if graph exists AND has selection
        zoomInButton?.SetEnabled(graphExists);                  // Can zoom if graph exists
        zoomOutButton?.SetEnabled(graphExists);                 // Can zoom if graph exists

        // Ensure the ObjectField displays the correct current asset
        // Use SetValueWithoutNotify to prevent triggering its own value changed callback.
        _assetObjectField?.SetValueWithoutNotify(_currentAsset);
    }

    /// <summary>
    /// Handles the creation of a new DialogueDataSO asset.
    /// </summary>
    private void CreateNewDialogueAsset()
    {
        // Prevent creation if an asset is already loaded/selected in the toolbar
        if (_currentAsset != null)
        {
            EditorUtility.DisplayDialog("Action Required", "Please clear the 'Dialogue Asset' field in the toolbar before creating a new asset.", "OK");
            return;
        }

        // Prompt user for file path and name
        string path = EditorUtility.SaveFilePanelInProject("Create New Dialogue Tree", "NewDialogueTree", "asset", "Please enter a file name");

        if (string.IsNullOrEmpty(path)) return; // User cancelled

        // Create the main ScriptableObject asset
        DialogueDataSO newDialogue = ScriptableObject.CreateInstance<DialogueDataSO>();
        AssetDatabase.CreateAsset(newDialogue, path);

        // Optionally, create a default starting node
        if (typeof(SentenceNode).IsSubclassOf(typeof(DialogueNode)))
        {
            SentenceNode entryNode = (SentenceNode)newDialogue.CreateNode(typeof(SentenceNode)); // Assumes CreateNode handles Undo/sub-asset
            if (entryNode != null)
            {
                entryNode.name = "Start Node"; // Name the sub-asset
                entryNode.Content = "Dialogue Start...";
                entryNode.GraphPosition = new Vector2(150, 150); // Initial position
                newDialogue.EntryNode = entryNode; // Set as entry point
                EditorUtility.SetDirty(entryNode); // Mark node as dirty
            }
        }

        EditorUtility.SetDirty(newDialogue); // Mark main asset as dirty
        AssetDatabase.SaveAssets(); // Save asset and sub-assets
        AssetDatabase.Refresh(); // Refresh project view

        // Update the editor state to reflect the newly created asset
        _currentAsset = newDialogue;
        _assetObjectField?.SetValueWithoutNotify(_currentAsset); // Update toolbar field
        Selection.activeObject = _currentAsset; // Select in Project window
        EditorGUIUtility.PingObject(_currentAsset); // Highlight in Project window

        _graphView?.PopulateView(_currentAsset); // Load into graph view
        UpdateToolbarState(); // Update button states
    }

    /// <summary>
    /// Handles the save operation for the currently loaded dialogue asset.
    /// </summary>
    private void RequestDataOperation(bool save)
    {
        if (!save) return; // Only handles saving currently

        if (_currentAsset == null)
        {
            EditorUtility.DisplayDialog("No Asset Selected", "Cannot save. Please select a DialogueDataSO asset.", "OK");
            return;
        }
        if (_graphView == null) return; // Safety check

        // Mark the main asset and all its nodes (sub-assets) as dirty
        // This ensures all changes (positions, connections, content) are saved.
        // Note: Proper Undo usage in NodeViews should already handle this. This is extra safety.
        EditorUtility.SetDirty(_currentAsset);
        if (_currentAsset.AllNodes != null)
        {
            foreach (var node in _currentAsset.AllNodes)
            {
                if (node != null) EditorUtility.SetDirty(node);
            }
        }

        // Save all modified assets to disk
        AssetDatabase.SaveAssets();
        Debug.Log($"Dialogue '{_currentAsset.name}' saved.");
    }

    /// <summary>
    /// Gets the currently loaded DialogueDataSO asset.
    /// </summary>
    public DialogueDataSO GetCurrentDialogue() => _currentAsset;

    /// <summary>
    /// Called by Unity when the selection changes in the Project window.
    /// Updates the toolbar's ObjectField and the GraphView if a DialogueDataSO is selected.
    /// </summary>
    private void OnSelectionChange()
    {
        DialogueDataSO selectedAsset = Selection.activeObject as DialogueDataSO;

        // Check if the selected object is a valid, main DialogueDataSO asset
        if (selectedAsset != null && AssetDatabase.IsMainAsset(selectedAsset))
        {
            // If the selected asset is different from the currently loaded one
            if (_currentAsset != selectedAsset)
            {
                _currentAsset = selectedAsset; // Update internal reference
                _assetObjectField?.SetValueWithoutNotify(_currentAsset); // Update toolbar field
                _graphView?.PopulateView(_currentAsset); // Load into graph view
                UpdateToolbarState(); // Update button states
            }
        }
        // If the selection changed TO something else (or nothing) FROM a dialogue asset
        else if (Selection.activeObject != _currentAsset)
        {
            // Clear the view only if there was an asset previously loaded or shown in the field
            if (_currentAsset != null || (_assetObjectField != null && _assetObjectField.value != null))
            {
                _currentAsset = null;
                _assetObjectField?.SetValueWithoutNotify(null); // Clear toolbar field
                _graphView?.PopulateView(null); // Clear graph view
                UpdateToolbarState(); // Update button states
            }
        }
        // If selection didn't change or wasn't relevant, do nothing more
    }
}
