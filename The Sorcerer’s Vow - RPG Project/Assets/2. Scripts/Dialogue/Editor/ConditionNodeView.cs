using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements; // Necessário para ObjectField

/// <summary>
/// Representação visual para um nó de diálogo do tipo Condição.
/// </summary>
public class ConditionNodeView : DialogueNodeView
{
    // Acesso seguro e tipado aos dados
    private ConditionNode _conditionNodeData => NodeData as ConditionNode;

    // Referências às portas de saída específicas
    private Port _trueOutputPort;
    private Port _falseOutputPort;

    /// <summary>
    /// Construtor para a view do nó de condição.
    /// </summary>
    public ConditionNodeView(ConditionNode nodeData, DialogueGraphView graphView)
        : base(nodeData, graphView) // Chama o construtor da classe base
    {
        // Título definido na base
    }

    /// <summary>
    /// Adiciona a classe USS específica para este tipo de nó.
    /// </summary>
    protected override void SetupClasses()
    {
        // base.SetupClasses(); // Chama se a base tiver lógica
        AddToClassList("condition-node");
    }

    /// <summary>
    /// Cria as duas portas de saída fixas: "True" e "False".
    /// Implementação do método abstrato da classe base.
    /// </summary>
    protected override void CreateOutputPorts()
    {
        // Usa o método auxiliar da classe base
        _trueOutputPort = AddOutputPort("True", Port.Capacity.Single, "✔️ True ");
        _falseOutputPort = AddOutputPort("False", Port.Capacity.Single, "❌ False");

        if (_trueOutputPort == null || _falseOutputPort == null)
        {
            Debug.LogError($"Failed to create True/False ports for ConditionNodeView ({NodeData?.Guid}).");
        }
    }

    /// <summary>
    /// Cria o campo de UI para selecionar o asset ConditionSO a ser verificado.
    /// Implementação do método abstrato da classe base.
    /// </summary>
    protected override void SetupDataBinding()
    {
        if (_conditionNodeData == null)
        {
            Debug.LogError($"ConditionNodeView ({NodeData?.Guid}) has invalid NodeData (not a ConditionNode). Cannot bind data.");
            mainContainer.Add(new Label("Error: Node data is invalid!") { style = { color = Color.red } });
            return;
        }

        // --- Campo Condição (ConditionSO Asset) ---
        var conditionField = new ObjectField("Condition Asset:")
        {
            objectType = typeof(ConditionSO),
            allowSceneObjects = false,
            value = _conditionNodeData.ConditionToCheck,
            tooltip = "Drag the Condition ScriptableObject Asset to evaluate here."
        };

        conditionField.RegisterValueChangedCallback(evt =>
        {
            // Só registra Undo se o valor mudou
            ConditionSO newValue = evt.newValue as ConditionSO;
            if (_conditionNodeData.ConditionToCheck != newValue)
            {
                Undo.RecordObject(_conditionNodeData, "Change Condition Asset");
                _conditionNodeData.ConditionToCheck = newValue;
                EditorUtility.SetDirty(_conditionNodeData);
            }
        });

        mainContainer.Add(conditionField);

        RefreshExpandedState();
        RefreshPorts();
    }

    /// <summary>
    /// Retorna a porta de saída ("True" ou "False") correspondente ao nome.
    /// </summary>
    public override Port GetOutputPort(string portName)
    {
        switch (portName)
        {
            case "True": return _trueOutputPort;
            case "False": return _falseOutputPort;
            default:
                // Debug.LogWarning($"Requested unknown output port '{portName}' from ConditionNodeView ({NodeData?.Guid}).");
                return base.GetOutputPort(portName); // Chama base como fallback
        }
    }

    /// <summary>
    /// Chamado quando uma aresta é conectada às portas "True" ou "False".
    /// Atualiza o TrueNodeGuid ou FalseNodeGuid nos dados.
    /// Implementação do método abstrato da classe base.
    /// </summary>
    public override void ConnectChild(Edge edge)
    {
        Port outputPort = edge.output;
        DialogueNodeView childView = edge.input?.node as DialogueNodeView;

        if (outputPort != null && childView?.NodeData != null)
        {
            string childGuid = childView.NodeData.Guid;
            bool changed = false;

            // Determina qual GUID atualizar e se ele realmente mudou
            if (outputPort == _trueOutputPort && _conditionNodeData.TrueNodeGuid != childGuid)
            {
                _conditionNodeData.TrueNodeGuid = childGuid;
                changed = true;
            }
            else if (outputPort == _falseOutputPort && _conditionNodeData.FalseNodeGuid != childGuid)
            {
                _conditionNodeData.FalseNodeGuid = childGuid;
                changed = true;
            }
            // else { Debug.LogWarning($"ConditionNodeView ({NodeData?.Guid}) connected via unexpected port: {outputPort.portName}"); }

            // Só registra Undo e marca dirty se houve mudança
            if (changed)
            {
                Undo.RecordObject(_conditionNodeData, "Connect Condition Node");
                EditorUtility.SetDirty(_conditionNodeData);
            }
        }
        else { Debug.LogWarning($"ConnectChild failed in ConditionNodeView: Invalid port or child view/data. Port: {outputPort?.portName}"); }
    }

    /// <summary>
    /// Chamado quando uma aresta é desconectada das portas "True" ou "False".
    /// Limpa o TrueNodeGuid ou FalseNodeGuid nos dados.
    /// Implementação do método abstrato da classe base.
    /// </summary>
    public override void DisconnectChild(Edge edge)
    {
        Port outputPort = edge.output;

        if (outputPort != null)
        {
            bool changed = false;

            // Determina qual GUID limpar e se ele não estava nulo
            if (outputPort == _trueOutputPort && !string.IsNullOrEmpty(_conditionNodeData.TrueNodeGuid))
            {
                _conditionNodeData.TrueNodeGuid = null;
                changed = true;
            }
            else if (outputPort == _falseOutputPort && !string.IsNullOrEmpty(_conditionNodeData.FalseNodeGuid))
            {
                _conditionNodeData.FalseNodeGuid = null;
                changed = true;
            }
            // else { Debug.LogWarning($"ConditionNodeView ({NodeData?.Guid}) disconnected via unexpected port: {outputPort.portName}"); }

            // Só registra Undo e marca dirty se houve mudança
            if (changed)
            {
                Undo.RecordObject(_conditionNodeData, "Disconnect Condition Node");
                EditorUtility.SetDirty(_conditionNodeData);
            }
        }
        else { Debug.LogWarning($"DisconnectChild failed in ConditionNodeView: Invalid port."); }
    }
}
