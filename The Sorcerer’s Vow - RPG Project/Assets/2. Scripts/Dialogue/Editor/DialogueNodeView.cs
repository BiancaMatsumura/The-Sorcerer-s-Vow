using System;
using System.Linq; // Necessário para OfType<T>() e FirstOrDefault()
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Classe base para a representação visual de qualquer nó no grafo de diálogo.
/// Lida com a estrutura básica, portas de entrada/saída, posição, GUID, menu de contexto e renomeação.
/// </summary>
public abstract class DialogueNodeView : Node // Tornada abstrata para forçar implementação de métodos específicos
{
    /// <summary>
    /// Os dados ScriptableObject que este nó representa.
    /// </summary>
    public DialogueNode NodeData { get; protected set; }

    /// <summary>
    /// A porta de entrada padrão para a maioria dos nós.
    /// </summary>
    public Port InputPort { get; protected set; }

    /// <summary>
    /// Referência à GraphView que contém este nó.
    /// </summary>
    protected DialogueGraphView GraphView { get; private set; }

    // --- Campos para Renomeação ---
    protected Label _titleLabel;          // Referência ao Label padrão do título
    protected TextField _titleTextField;    // TextField usado para edição
    protected bool _isRenaming = false;   // Flag para controlar o estado de renomeação
    // ------------------------------

    /// <summary>
    /// Construtor base para todas as views de nó.
    /// </summary>
    /// <param name="nodeData">O ScriptableObject DialogueNode a ser representado.</param>
    /// <param name="graphView">A GraphView pai.</param>
    protected DialogueNodeView(DialogueNode nodeData, DialogueGraphView graphView) : base()
    {
        // Validações essenciais
        if (nodeData == null)
        {
            throw new ArgumentNullException(nameof(nodeData), "DialogueNodeView cannot be created with null NodeData!");
        }
        if (graphView == null)
        {
            throw new ArgumentNullException(nameof(graphView), "DialogueNodeView cannot be created with null GraphView!");
        }

        // Garante que todo nó tenha um GUID (essencial para salvar/carregar conexões)
        if (string.IsNullOrEmpty(nodeData.Guid))
        {
            Debug.LogWarning($"NodeData of type {nodeData.GetType().Name} (Name: {nodeData.name}) had a null or empty GUID. Assigning a new one. Ensure nodes are created via DialogueDataSO.CreateNode().");
            nodeData.Guid = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(nodeData); // Marca para salvar o novo GUID
        }

        this.NodeData = nodeData;
        this.GraphView = graphView;

        // Define o título do nó.
        this.title = string.IsNullOrWhiteSpace(nodeData.name)
                     ? nodeData.GetType().Name.Replace("Node", "")
                     : nodeData.name;
        this.viewDataKey = nodeData.Guid;

        // Define a posição inicial baseada nos dados salvos
        style.left = nodeData.GraphPosition.x;
        style.top = nodeData.GraphPosition.y;

        // Chama métodos virtuais/abstratos para configurar partes específicas do nó
        CreateInputPorts();
        CreateOutputPorts();
        SetupClasses();
        SetupDataBinding();

        // --- Configuração da Renomeação ---
        _titleLabel = this.Q<Label>(name: "title-label");
        if (_titleLabel == null) { _titleLabel = titleContainer.Q<Label>(); }

        // Mantém LogError se não encontrar o label
        if (_titleLabel == null) { Debug.LogError($"[NodeView] Node '{this.title}': Could not find title Label!"); }


        if (titleContainer != null)
        {
            titleContainer.RegisterCallback<MouseDownEvent>(OnTitleMouseDown);
        }
        else
        {
            // Mantém LogError se não encontrar o container
            Debug.LogError($"[NodeView] Node '{this.title}': titleContainer is NULL!");
        }
        // ---------------------------------
    }

    /// <summary>
    /// Cria a porta de entrada padrão. Pode ser sobrescrito se um nó não tiver entrada ou tiver múltiplas.
    /// </summary>
    protected virtual void CreateInputPorts()
    {
        InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        if (InputPort != null)
        {
            InputPort.portName = "Input";
            inputContainer.Add(InputPort);
        }
        else
        {
            Debug.LogError($"Failed to instantiate Input Port for node {title} ({NodeData.Guid})");
        }
    }

    /// <summary>
    /// Método ABSTRATO para criar portas de saída. DEVE ser implementado por subclasses.
    /// </summary>
    protected abstract void CreateOutputPorts();

    /// <summary>
    /// Método virtual para adicionar classes de estilo USS ao nó. Pode ser sobrescrito.
    /// </summary>
    protected virtual void SetupClasses() { }

    /// <summary>
    /// Método ABSTRATO para adicionar controles de UI específicos (campos de texto, etc.). DEVE ser implementado por subclasses.
    /// </summary>
    protected abstract void SetupDataBinding();

    /// <summary>
    /// Sobrescreve o método padrão para construir o menu de contexto (clique direito) do nó.
    /// </summary>
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        if (GraphView != null && GraphView.GetCurrentDialogue() != null && NodeData != null)
        {
            var currentDialogue = GraphView.GetCurrentDialogue();
            if (currentDialogue.EntryNode != NodeData)
            {
                evt.menu.AppendAction("Set as Entry Point", action => { GraphView.SetEntryPoint(this); }, DropdownMenuAction.AlwaysEnabled);
            }
            else
            {
                evt.menu.AppendAction("Set as Entry Point", action => { }, DropdownMenuAction.Status.Checked);
            }
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Delete Node", action => { GraphView.DeleteElements(new[] { this }); }, DropdownMenuAction.AlwaysEnabled);
        }
    }

    /// <summary>
    /// Chamado quando a posição do nó é alterada na interface gráfica.
    /// Atualiza a posição nos dados (NodeData) usando Undo.
    /// </summary>
    public override void SetPosition(Rect newPos)
    {
        if (NodeData == null) return;
        base.SetPosition(newPos);
        Vector2 newGraphPosition = new Vector2(newPos.xMin, newPos.yMin);
        if (Vector2.Distance(NodeData.GraphPosition, newGraphPosition) > 0.01f)
        {
            Undo.RecordObject(NodeData, "Move Dialogue Node");
            NodeData.GraphPosition = newGraphPosition;
        }
    }


    /// <summary>
    /// Callback para quando o container do título é clicado.
    /// Detecta duplo clique para iniciar a renomeação.
    /// </summary>
    private void OnTitleMouseDown(MouseDownEvent evt)
    {
        if (evt.button == 0 && !_isRenaming && evt.clickCount == 2)
        {
            StartRenaming();
            evt.StopPropagation();
        }
    }

    /// <summary>
    /// Inicia o processo de renomeação do nó.
    /// </summary>
    protected virtual void StartRenaming()
    {
        if (_isRenaming || _titleLabel == null || NodeData == null) return;

        _isRenaming = true;
        _titleLabel.style.display = DisplayStyle.None;

        if (_titleTextField == null)
        {
            _titleTextField = new TextField { name = "title-textfield", isDelayed = false };
            _titleTextField.RegisterCallback<KeyDownEvent>(OnTitleRenameKeyDown);
            _titleTextField.RegisterCallback<FocusOutEvent>(OnTitleRenameFocusOut);
            _titleTextField.AddToClassList("title-textfield");
        }

        _titleTextField.value = NodeData.name;
        titleContainer.Insert(0, _titleTextField);

        schedule.Execute(() => {
            if (_titleTextField != null && _isRenaming)
            {
                _titleTextField.Focus();
                _titleTextField.SelectAll();
            }
        }).StartingIn(10);
    }

    /// <summary>
    /// Finaliza o processo de renomeação, aplicando as mudanças.
    /// </summary>
    protected virtual void CommitRename()
    {
        if (!_isRenaming || _titleTextField == null || NodeData == null)
        {
            // Ainda chama StopRenaming para garantir limpeza da UI se algo deu errado
            StopRenaming();
            return;
        }

        string oldName = NodeData.name;
        string newName = _titleTextField.value.Trim();

        if (string.IsNullOrWhiteSpace(newName))
        {
            // Reverte silenciosamente ou pode adicionar um LogWarning se preferir
            newName = oldName;
        }

        if (oldName != newName)
        {
            Undo.RecordObject(NodeData, $"Rename Node from '{oldName}' to '{newName}'");
            NodeData.name = newName;
            this.title = newName;
            EditorUtility.SetDirty(NodeData);
        }
        else
        {
            this.title = NodeData.name; // Garante que o título visual esteja sincronizado
        }

        StopRenaming();
    }

    /// <summary>
    /// Para o modo de renomeação, removendo o TextField e mostrando o Label.
    /// </summary>
    private void StopRenaming()
    {
        if (!_isRenaming) return;

        if (_titleTextField != null && _titleTextField.parent == titleContainer)
        {
            titleContainer.Remove(_titleTextField);
        }
        if (_titleLabel != null)
        {
            _titleLabel.style.display = DisplayStyle.Flex;
        }
        _isRenaming = false;
    }


    /// <summary>
    /// Callback para teclas pressionadas enquanto o TextField do título está focado.
    /// Finaliza a edição com Enter, cancela com Escape.
    /// </summary>
    private void OnTitleRenameKeyDown(KeyDownEvent evt)
    {
        if (!_isRenaming) return;

        switch (evt.keyCode)
        {
            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                CommitRename();
                evt.StopPropagation();
                break;
            case KeyCode.Escape:
                this.title = NodeData.name; // Restaura título visual antes de parar
                StopRenaming();
                evt.StopPropagation();
                break;
        }
    }

    /// <summary>
    /// Callback para quando o TextField do título perde o foco.
    /// Finaliza a edição.
    /// </summary>
    private void OnTitleRenameFocusOut(FocusOutEvent evt)
    {
        if (_isRenaming)
        {
            CommitRename();
        }
    }


    /// <summary>
    /// Retorna uma porta de saída pelo seu nome. Usado ao recriar conexões.
    /// </summary>
    public virtual Port GetOutputPort(string portName)
    {
        return outputContainer.Children()
                              .OfType<Port>()
                              .FirstOrDefault(p => p.portName == portName);
    }

    /// <summary>
    /// Método virtual chamado quando uma aresta é conectada a uma porta de SAÍDA deste nó.
    /// </summary>
    public abstract void ConnectChild(Edge edge);

    /// <summary>
    /// Método virtual chamado quando uma aresta saindo de uma porta de SAÍDA deste nó é desconectada.
    /// </summary>
    public abstract void DisconnectChild(Edge edge);

    /// <summary>
    /// Método virtual (opcional) chamado quando uma aresta é conectada à porta de ENTRADA deste nó.
    /// </summary>
    public virtual void OnInputConnected(Edge edge) { }

    /// <summary>
    /// Método virtual (opcional) chamado quando uma aresta é desconectada da porta de ENTRADA deste nó.
    /// </summary>
    public virtual void OnInputDisconnected(Edge edge) { }

    /// <summary>
    /// Método auxiliar para criar e adicionar uma porta de saída ao container `outputContainer`.
    /// </summary>
    protected Port AddOutputPort(string portName, Port.Capacity capacity = Port.Capacity.Single, string portLabel = null)
    {
        Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, typeof(bool));
        if (outputPort != null)
        {
            outputPort.portName = portName;
            if (!string.IsNullOrEmpty(portLabel))
            {
                Label labelElement = new Label(portLabel);
                labelElement.AddToClassList("unity-label"); // Usa classe padrão para estilo USS
                outputPort.contentContainer.Add(labelElement);
            }
            outputContainer.Add(outputPort);
        }
        else
        {
            Debug.LogError($"Failed to instantiate output port '{portName}' for node {title} ({NodeData?.Guid}).");
        }
        return outputPort;
    }
}
