using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements; // Necessário para ObjectField

/// <summary>
/// Representação visual para um nó de diálogo do tipo Sentença/Fala.
/// </summary>
public class SentenceNodeView : DialogueNodeView
{
    // Propriedade para acesso seguro e tipado aos dados específicos deste nó
    private SentenceNode _sentenceNodeData => NodeData as SentenceNode;

    // Referência à porta de saída única deste nó
    private Port _nextOutputPort;

    /// <summary>
    /// Construtor para a view do nó de sentença.
    /// </summary>
    public SentenceNodeView(SentenceNode nodeData, DialogueGraphView graphView)
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
        AddToClassList("sentence-node");
    }

    /// <summary>
    /// Cria a única porta de saída "Next".
    /// Implementação do método abstrato da classe base.
    /// </summary>
    protected override void CreateOutputPorts()
    {
        // Usa o método auxiliar da classe base
        _nextOutputPort = AddOutputPort("Next", Port.Capacity.Single, "Next ");
        if (_nextOutputPort == null)
        {
            Debug.LogError($"Failed to create 'Next' output port for SentenceNodeView ({NodeData?.Guid}).");
        }
    }

    /// <summary>
    /// Cria os campos de UI para editar o Ator (CharacterDataSO) e o Conteúdo (texto da fala).
    /// Implementação do método abstrato da classe base.
    /// </summary>
    protected override void SetupDataBinding()
    {
        if (_sentenceNodeData == null)
        {
            Debug.LogError($"SentenceNodeView ({NodeData?.Guid}) has invalid NodeData (not a SentenceNode). Cannot bind data.");
            mainContainer.Add(new Label("Error: Node data is invalid!") { style = { color = Color.red } });
            return;
        }

        // --- Campo Ator (CharacterDataSO) ---
        var actorField = new ObjectField("Actor:")
        {
            objectType = typeof(CharacterDataSO),
            allowSceneObjects = false,
            value = _sentenceNodeData.ActorData,
            tooltip = "The character speaking this line."
        };
        actorField.RegisterValueChangedCallback(evt =>
        {
            Undo.RecordObject(_sentenceNodeData, "Change Dialogue Actor");
            _sentenceNodeData.ActorData = evt.newValue as CharacterDataSO;
            EditorUtility.SetDirty(_sentenceNodeData); // Marcar dirty é bom aqui
        });
        mainContainer.Add(actorField);

        // --- Campo Conteúdo (Texto da Fala) ---
        var contentField = new TextField("Content:")
        {
            multiline = true,
            value = _sentenceNodeData.Content,
            tooltip = "The dialogue text to be displayed."
        };
        contentField.AddToClassList("dialogue-text-field"); // Para estilo USS
        contentField.style.whiteSpace = WhiteSpace.Normal;
        contentField.style.minWidth = 250;
        contentField.style.minHeight = 60;

        contentField.RegisterValueChangedCallback(evt =>
        {
            // Só registra Undo e marca dirty se o valor realmente mudou
            if (_sentenceNodeData.Content != evt.newValue)
            {
                Undo.RecordObject(_sentenceNodeData, "Change Dialogue Content");
                _sentenceNodeData.Content = evt.newValue;
                EditorUtility.SetDirty(_sentenceNodeData);
            }
        });
        mainContainer.Add(contentField);

        RefreshExpandedState();
        RefreshPorts();
    }

    /// <summary>
    /// Retorna a porta de saída "Next" quando solicitada pelo nome.
    /// </summary>
    public override Port GetOutputPort(string portName)
    {
        return portName == "Next" ? _nextOutputPort : base.GetOutputPort(portName);
    }

    /// <summary>
    /// Chamado quando uma aresta é conectada à porta "Next".
    /// Atualiza o NextNodeGuid nos dados.
    /// Implementação do método abstrato da classe base.
    /// </summary>
    public override void ConnectChild(Edge edge)
    {
        if (edge.output == _nextOutputPort)
        {
            DialogueNodeView childView = edge.input?.node as DialogueNodeView;
            if (childView?.NodeData != null)
            {
                // Só registra Undo se o GUID realmente mudou
                if (_sentenceNodeData.NextNodeGuid != childView.NodeData.Guid)
                {
                    Undo.RecordObject(_sentenceNodeData, "Connect Dialogue Node (Sentence)");
                    _sentenceNodeData.NextNodeGuid = childView.NodeData.Guid;
                    EditorUtility.SetDirty(_sentenceNodeData);
                }
            }
            else { Debug.LogWarning($"ConnectChild failed in SentenceNodeView: Child view or data is null."); }
        }
        else { Debug.LogWarning($"SentenceNodeView ({NodeData?.Guid}) ConnectChild called with unexpected output port: {edge.output?.portName}"); }
    }

    /// <summary>
    /// Chamado quando uma aresta é desconectada da porta "Next".
    /// Limpa o NextNodeGuid nos dados.
    /// Implementação do método abstrato da classe base.
    /// </summary>
    public override void DisconnectChild(Edge edge)
    {
        if (edge.output == _nextOutputPort)
        {
            // Só registra Undo se o GUID não estava nulo
            if (!string.IsNullOrEmpty(_sentenceNodeData.NextNodeGuid))
            {
                Undo.RecordObject(_sentenceNodeData, "Disconnect Dialogue Node (Sentence)");
                _sentenceNodeData.NextNodeGuid = null;
                EditorUtility.SetDirty(_sentenceNodeData);
            }
        }
        else { Debug.LogWarning($"SentenceNodeView ({NodeData?.Guid}) DisconnectChild called with unexpected output port: {edge.output?.portName}"); }
    }
}
