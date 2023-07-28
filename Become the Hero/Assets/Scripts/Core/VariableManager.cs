using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;


/// <summary>
/// Manages and contains all variables utilized throughout the entire game.
/// 
/// To add a new variable, create the variable in its respective folder,
/// add it to this scriptable objects' respective list via inspector,
/// then add its name as a contstant string in <see cref="VariableConstants"/>.
/// </summary>

[CreateAssetMenu(fileName = "Variable Manager", menuName = "System/VariableManager")]
public class VariableManager : ScriptableObject
{
    #region Variable Fields
    [SerializeField]
    [Header("Booleans")]
    [Tooltip("Add all bools to this list.")]
    private List<BoolVariable> bools;

    [SerializeField]
    [Header("Floats")]
    [Tooltip("Add all floats to this list.")]
    private List<FloatVariable> floats;

    [SerializeField]
    [Header("Vector2s")]
    [Tooltip("Add all Vector2 to this list.")]
    private List<Vector2Variable> vector2s;

    public static VariableManager Instance;
    #endregion

    #region Variable Maps - For String & Var Lookup
    private Dictionary<string, BoolVariable> boolMap;
    private Dictionary<string, FloatVariable> floatMap;
    private Dictionary<string, Vector2Variable> vector2Map;
    #endregion

    #region Get Variable Values
    /// <summary>
    /// Gets a <see cref="BoolVariable"/> from our <see cref="boolMap"/>.
    /// If no variable by string name exists, returns null.
    /// </summary>
    /// <param name="varName">The string name of the variable.</param>
    /// <returns>The value of the variable associated with the passed in string, if any.</returns>
    public bool GetBoolVariableValue(string varName)
    {
        BoolVariable value;

        //searches our dictionary for the variable, outputs it, and returns it
        if (boolMap.TryGetValue(varName.ToLower(), out value))
        {
            return value.Value;
        }

        //Debug.LogError("ERROR: No variable of name " + varName + " exists. Returning false.");
        return false;
    }

    /// <summary>
    /// Gets a <see cref="FloatVariable"/> from our <see cref="floatMap"/>.
    /// If no variable by string name exists, returns null.
    /// </summary>
    /// <param name="varName">The string name of the variable.</param>
    /// <returns>The value of the variable associated with the passed in string, if any.</returns>
    public float GetFloatVariableValue(string varName)
    {
        FloatVariable value;

        //searches our dictionary for the variable, outputs it, and returns it
        if (floatMap.TryGetValue(varName.ToLower(), out value))
        {
            return value.Value;
        }

        //Debug.LogError("ERROR: No variable of name " + varName + " exists. Returning false.");
        return 0;
    }


    /// <summary>
    /// Gets a <see cref="Vector2Variable"/> from our <see cref="vector2Map"/>.
    /// If no variable by string name exists, returns null.
    /// </summary>
    /// <param name="varName">The string name of the variable.</param>
    /// <returns>The value of the variable associated with the passed in string, if any.</returns>
    public Vector2 GetVector2VariableValue(string varName)
    {
        Vector2Variable value;

        //searches our dictionary for the variable, outputs it, and returns it
        if (vector2Map.TryGetValue(varName.ToLower(), out value))
        {
            return value.Value;
        }

        //Debug.LogError("ERROR: No variable of name " + varName + " exists. Returning false.");
        return Vector2.zero;
    }
    #endregion

    #region Set Variable Values
    /// <summary>
    /// Sets the value of a <see cref="BoolVariable"/> in our <see cref="boolMap"/>.
    /// </summary>
    /// <param name="varName">The string name of the variable.</param>
    /// <param name="val">The value to set.</param>
    public void SetBoolVariableValue(string varName, bool val)
    {
        BoolVariable value;

        //searches our dictionary for the variable, outputs it, and returns it
        if (boolMap.TryGetValue(varName.ToLower(), out value))
        {
            value.Value = val;
            return;
        }

        //Debug.LogError("ERROR: No variable of name " + varName + " exists.");
    }


    /// <summary>
    /// Sets the value of a <see cref="FloatVariable"/> in our <see cref="floatMap"/>.
    /// </summary>
    /// <param name="varName">The string name of the variable.</param>
    /// <param name="val">The value to set.</param>
    public void SetFloatVariableValue(string varName, float val)
    {
        FloatVariable value;

        //searches our dictionary for the variable, outputs it, and returns it
        if (floatMap.TryGetValue(varName.ToLower(), out value))
        {
            value.Value = val;
            return;
        }

        //Debug.LogError("ERROR: No variable of name " + varName + " exists.");
    }


    /// <summary>
    /// Sets the value of a <see cref="Vector2Variable"/> in our <see cref="vector2Map"/>.
    /// </summary>
    /// <param name="varName">The string name of the variable.</param>
    /// <param name="val">The value to set.</param>
    public void SetVector2VariableValue(string varName, Vector2 val)
    {
        Vector2Variable value;

        //searches our dictionary for the variable, outputs it, and returns it
        if (vector2Map.TryGetValue(varName.ToLower(), out value))
        {
            value.Value = val;
            return;
        }

        //Debug.LogError("ERROR: No variable of name " + varName + " exists.");
    }
    #endregion

    #region Populate Variable Maps
    /// <summary>
    /// Maps each bool variables's name to the bool itself, after 
    /// filling <see cref="boolMap"/> via inspector.
    /// </summary>
    private void PopulateBoolMap()
    {
        boolMap = new Dictionary<string, BoolVariable>();
        foreach (BoolVariable boolVar in this.bools)
        {
            boolMap.Add(boolVar.name.ToLower(), boolVar);
        }
    }

    /// <summary>
    /// Maps each float variables's name to the float itself, after 
    /// filling <see cref="floatMap"/> via inspector.
    /// </summary>
    private void PopulateFloatMap()
    {
        floatMap = new Dictionary<string, FloatVariable>();
        foreach (FloatVariable floatVar in this.floats)
        {
            floatMap.Add(floatVar.name.ToLower(), floatVar);
        }
    }

    /// <summary>
    /// Maps each Vector2 variables's name to the Vector2 itself, after 
    /// filling <see cref="vector2Map"/> via inspector.
    /// </summary>
    private void PopulateVector2Map()
    {
        vector2Map = new Dictionary<string, Vector2Variable>();
        foreach (Vector2Variable vector2Var in this.vector2s)
        {
            vector2Map.Add(vector2Var.name.ToLower(), vector2Var);
        }
    }
    #endregion

    #region On Enable
    /// <summary>
    /// Populate all our maps upon startup.
    /// </summary>
    private void OnEnable()
    {
        this.PopulateBoolMap();
        this.PopulateFloatMap();
        this.PopulateVector2Map();

        if (!Instance)
        {
            Instance = this;
        }
    }
    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void E()
    {
        Resources.Load<VariableManager>("Variable Manager");
    }
}