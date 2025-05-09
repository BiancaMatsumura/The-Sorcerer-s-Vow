// DialogueDataSO.cs
using System;
using System.Collections.Generic;
using System.Linq; // Needed for FirstOrDefault
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // Needed for Editor-only code
#endif

/// <summary>
/// The main ScriptableObject asset representing a complete dialogue tree or sequence.
/// Contains either a node-based structure (EntryNode + AllNodes) or a legacy linear list (Sentences).
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueTree", menuName = "Scriptable Objects/Dialogue Tree")]
public class DialogueDataSO : ScriptableObject
{
    [Header("Node-Based Dialogue (Prioritized)")]
    [Tooltip("The first node to execute in this dialogue. If null, the system will attempt to use the legacy 'Sentences' list.")]
    public DialogueNode EntryNode;

    [HideInInspector] // Managed by the custom editor, not meant for direct Inspector modification
    public List<DialogueNode> AllNodes = new(); // Stores all nodes within this dialogue asset

    [Header("Legacy Linear Dialogue (Fallback)")]
    [Tooltip("Linear list of sentences (Old format). Used ONLY if 'EntryNode' is null.")]
    public List<DialogSentence> Sentences = new(); // Kept for backward compatibility

#if UNITY_EDITOR
    /// <summary>
    /// [Editor-Only] Creates a new DialogueNode of the specified type,
    /// adds it as a sub-asset, registers for Undo, and adds it to the AllNodes list.
    /// </summary>
    /// <param name="type">The Type of DialogueNode to create (must inherit from DialogueNode).</param>
    /// <returns>The newly created DialogueNode, or null if creation failed.</returns>
    public DialogueNode CreateNode(Type type)
    {
        // Validate that the requested type is actually a DialogueNode
        if (!typeof(DialogueNode).IsAssignableFrom(type))
        {
            Debug.LogError($"[DialogueDataSO] Invalid type for node creation: {type.Name}. Must inherit from DialogueNode.");
            return null;
        }

        DialogueNode node = (DialogueNode)ScriptableObject.CreateInstance(type);
        // Use a more descriptive default name if possible, maybe later based on content
        node.name = type.Name;
        // Use Unity's robust GUID generation for assets
        node.Guid = GUID.Generate().ToString();

        // Register the object creation for the Undo system
        Undo.RegisterCreatedObjectUndo(node, "Create Dialogue Node");

        // IMPORTANT: Add the new node as a sub-asset of this DialogueDataSO asset
        AssetDatabase.AddObjectToAsset(node, this);

        // Register the change to the AllNodes list for the Undo system
        Undo.RecordObject(this, "Add Node To Dialogue List");
        AllNodes.Add(node);

        // Mark the main asset as dirty so the changes (like adding to AllNodes) are saved
        EditorUtility.SetDirty(this);

        // Optional: Force save assets immediately. Can be slow if done frequently.
        // AssetDatabase.SaveAssets();

        return node;
    }

    /// <summary>
    /// [Editor-Only] Deletes a specified DialogueNode, removes it from the list,
    /// destroys the sub-asset, and registers operations for Undo.
    /// </summary>
    /// <param name="nodeToDelete">The DialogueNode sub-asset to delete.</param>
    public void DeleteNode(DialogueNode nodeToDelete)
    {
        if (nodeToDelete != null && AllNodes.Contains(nodeToDelete))
        {
            // Register the list modification for Undo
            Undo.RecordObject(this, "Delete Node From Dialogue List");
            AllNodes.Remove(nodeToDelete);

            // IMPORTANT: Use Undo.DestroyObjectImmediate to properly handle
            // the destruction of the sub-asset and register it for Undo.
            Undo.DestroyObjectImmediate(nodeToDelete);

            // Mark the main asset as dirty
            EditorUtility.SetDirty(this);

            // Optional: Force save assets immediately.
            // AssetDatabase.SaveAssets();
        }
        else if (nodeToDelete != null)
        {
            Debug.LogWarning($"[DialogueDataSO] Tried to delete node '{nodeToDelete.name}' but it wasn't found in the AllNodes list.");
        }
    }
#endif

    /// <summary>
    /// Finds a DialogueNode within this asset by its unique GUID.
    /// Used by the runtime DialogueManager to navigate the graph.
    /// </summary>
    /// <param name="guid">The GUID of the node to find.</param>
    /// <returns>The DialogueNode if found, otherwise null.</returns>
    public DialogueNode FindNodeByGuid(string guid)
    {
        // Use FirstOrDefault which safely returns null if no match is found or if the list is empty
        // Also includes a null check for the node itself in case the list somehow contains null entries (defensive programming)
        return AllNodes.FirstOrDefault(node => node != null && node.Guid == guid);
    }
}


// =======================================================================
//          DEFINITIONS FOR NODES, CONDITIONS, AND LEGACY STRUCTURES
// =======================================================================
// Note: These could also be in their own separate .cs files.

// ---- Base Class for All Dialogue Nodes ----
[Serializable] // Important if nested, though less so now they are SOs
public abstract class DialogueNode : ScriptableObject
{
    [HideInInspector] // Internal ID, not meant for manual editing
    public string Guid; // Unique identifier for saving/loading connections

    public Vector2 GraphPosition; // Stores position in the visual editor
}

// ---- Sentence Node ----
[Serializable]
public class SentenceNode : DialogueNode
{
    public CharacterDataSO ActorData; // Who is speaking? (Optional)
    [TextArea(3, 5)]
    public string Content; // The dialogue text
    public string NextNodeGuid; // GUID of the node to go to after this sentence
}

// ---- Choice Node ----
[Serializable]
public class ChoiceNode : DialogueNode
{
    public CharacterDataSO ActorData; // Who is presenting the choices? (Optional)
    [TextArea(3, 5)]
    public string PromptContent; // Text shown before the choices (e.g., "What do you do?")
    public List<PlayerChoice> Choices = new(); // List of choices available at this node
}

// ---- Structure for a Single Player Choice ----
[Serializable]
public class PlayerChoice
{
    [TextArea(1, 3)]
    public string ChoiceText; // Text displayed on the choice button
    public ConditionSO AvailabilityCondition; // Condition to check if this choice should be shown (Optional)
    public string TargetNodeGuid; // GUID of the node this choice leads to
}

// ---- Condition Node ----
[Serializable]
public class ConditionNode : DialogueNode
{
    public ConditionSO ConditionToCheck; // The Condition Asset to evaluate
    public string TrueNodeGuid;      // GUID of the node to go to if the condition is TRUE
    public string FalseNodeGuid;     // GUID of the node to go to if the condition is FALSE
}

// ---- Base Class for Condition Assets ----
// Allows creating different types of reusable conditions as ScriptableObjects.
public abstract class ConditionSO : ScriptableObject
{
    /// <summary>
    /// Evaluates the condition. The runtime DialogueManager will call this.
    /// IMPORTANT: You will likely need to modify this to accept parameters
    /// representing the current game state (e.g., player stats, inventory, quest flags).
    /// </summary>
    /// <returns>True if the condition is met, false otherwise.</returns>
    public abstract bool Evaluate(/* Pass GameState, PlayerData, etc. here */);
}

// ---- Example Concrete Condition: Flag Check ----
[CreateAssetMenu(fileName = "NewFlagCondition", menuName = "Scriptable Objects/Dialogue Conditions/Flag Condition")]
public class FlagConditionSO : ConditionSO
{
    [Tooltip("The exact name of the flag to check in your game's state system.")]
    public string FlagName;
    [Tooltip("The value the flag must have for the condition to be true.")]
    public bool RequiredValue = true;

    public override bool Evaluate(/* GameState gameState */)
    {
        Debug.LogWarning($"Condition '{this.name}' evaluating flag '{FlagName}'. IMPLEMENT ACTUAL GAME STATE CHECK!");
        // --- !!! Placeholder - Replace with your actual game logic !!! ---
        // Example:
        // if (GameStateManager.Instance != null)
        // {
        //     return GameStateManager.Instance.GetFlag(FlagName) == RequiredValue;
        // }
        // return false; // Default if game state is unavailable
        // --- End Placeholder ---

        // Return true for now for testing purposes, **BEWARE**
        return true;
    }
}

// ---- Legacy Structure for Linear Dialogues ----
[Serializable]
public class DialogSentence
{
    public CharacterDataSO ActorData; // Reference to the speaking character's data
    [TextArea(3, 5)]
    public string Content; // The line of dialogue
}
