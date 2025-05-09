using System;
using System.Collections.Generic;
using System.Linq; // Necessário para OfType<T>(), FirstOrDefault(), Any()
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A classe principal para a área visual do editor de grafos de diálogo.
/// Lida com a exibição de nós, conexões, manipulação e callbacks de interação.
/// </summary>
public class DialogueGraphView : GraphView
{
    /// <summary>
    /// Tamanho padrão para novos nós criados.
    /// </summary>
    public readonly Vector2 DefaultNodeSize = new Vector2(200, 150);

    // Referências
    private DialogueDataSO _currentDialogue; // O asset de diálogo sendo editado
    private readonly DialogueGraphEditorWindow _editorWindow; // A janela do editor que contém esta view

    // Cache para acesso rápido às NodeViews pelo GUID do NodeData
    private readonly Dictionary<string, DialogueNodeView> _nodeViews = new Dictionary<string, DialogueNodeView>();

    // Provider para a janela de busca de criação de nós
    private NodeCreationSearchWindow _searchWindowProvider;

    // --- Variáveis de Controle de Navegação ---
    private const float PAN_SPEED = 0.3f;       // Sensibilidade do Pan com Scroll
    // ZOOM_SPEED_SCROLL não é mais usado para o scroll
    private const float ZOOM_SPEED_BUTTON = 0.1f; // Fator de Zoom para os botões +/-
    private const float MIN_SCALE = 0.1f;      // Zoom mínimo
    private const float MAX_SCALE = 2.0f;      // Zoom máximo
    // ------------------------------------------

    /// <summary>
    /// Construtor da DialogueGraphView.
    /// </summary>
    /// <param name="editorWindow">A janela do editor pai.</param>
    public DialogueGraphView(DialogueGraphEditorWindow editorWindow)
    {
        _editorWindow = editorWindow ?? throw new ArgumentNullException(nameof(editorWindow));

        // --- Configuração Visual e de Interação ---

        // !! IMPORTANTE: Garanta que ContentZoomer NÃO seja adicionado !!
        // this.AddManipulator(new ContentZoomer()); // <-- COMENTADO OU REMOVIDO

        // Mantenha ContentDragger se quiser pan com botão do meio
        this.AddManipulator(new ContentDragger()); // <-- MANTIDO (opcional)

        // Manter manipuladores de seleção
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        // Adiciona um fundo de grade
        var grid = new GridBackground();
        Insert(0, grid); // Insere atrás de todos os outros elementos
        grid.StretchToParentSize(); // Faz a grade preencher toda a área

        // Adicionar Minimapa
        var miniMap = new MiniMap { anchored = true };
        miniMap.style.position = Position.Absolute;
        miniMap.style.right = 15; miniMap.style.top = 35; // Posição abaixo da toolbar da janela
        miniMap.style.width = 200; miniMap.style.height = 140;
        Add(miniMap);

        // --- Callbacks ---
        graphViewChanged = OnGraphViewChanged; // Callback para mudanças estruturais
        SetupNodeCreationRequest(); // Configura menu de contexto para criar nós
        RegisterCallback<DetachFromPanelEvent>(evt => TeardownSearchWindow()); // Limpeza

        // Callback para scroll personalizado (APENAS Pan com StopImmediatePropagation)
        RegisterCallback<WheelEvent>(OnWheelEvent);

        // Configuração inicial de zoom e posição
        SetupZoom(MIN_SCALE, MAX_SCALE); // Define limites no GraphView
        this.viewTransform.scale = Vector3.one; // Garante zoom inicial 1x
        this.viewTransform.position = Vector3.zero; // Garante posição inicial (0,0)
    }

    /// <summary>
    /// Configura a requisição de criação de nó (menu de contexto).
    /// </summary>
    private void SetupNodeCreationRequest()
    {
        // Cria e inicializa o provider da janela de busca UMA VEZ
        _searchWindowProvider = ScriptableObject.CreateInstance<NodeCreationSearchWindow>();
        _searchWindowProvider.Initialize(this);

        // Define a ação para quando o usuário clica com o botão direito (ou outro gatilho)
        nodeCreationRequest = (context) => {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindowProvider);
        };
    }

    /// <summary>
    /// Limpa a instância do ScriptableObject SearchWindowProvider quando a view é descartada.
    /// </summary>
    private void TeardownSearchWindow()
    {
        if (_searchWindowProvider != null)
        {
            UnityEngine.Object.DestroyImmediate(_searchWindowProvider);
            _searchWindowProvider = null;
        }
    }

    /// <summary>
    /// Callback para lidar com eventos da roda do mouse (scroll).
    /// Implementa APENAS pan (scroll normal). O zoom via scroll foi removido.
    /// Usa StopImmediatePropagation para garantir que nenhum outro handler processe o evento.
    /// </summary>
    private void OnWheelEvent(WheelEvent evt)
    {
        // Ignorar eventos sem delta significativo pode previnir jitter
        if (Mathf.Abs(evt.delta.x) < 0.01f && Mathf.Abs(evt.delta.y) < 0.01f)
        {
            return;
        }

        // --- PAN (Scroll Normal) ---
        // Calcula a mudança de posição baseada no delta do scroll
        // O sinal negativo é porque o delta geralmente é positivo para scroll "para baixo/direita"
        // e queremos mover o conteúdo na direção oposta ao scroll.
        Vector3 positionChange = new Vector3(evt.delta.x, evt.delta.y, 0) * -PAN_SPEED;
        // Ajusta a sensibilidade - pode precisar de ajuste fino
        // float sensitivity = 1.0f; // Ajuste este valor se o pan estiver muito rápido/lento
        // positionChange *= sensitivity;

        // Calcula a nova posição da view
        Vector3 newPosition = viewTransform.position + positionChange;

        // Atualiza a posição da view (mantendo a escala atual)
        // Usamos UpdateViewTransform para que o GraphView gerencie corretamente
        UpdateViewTransform(newPosition, viewTransform.scale);

        // !! PONTO CRÍTICO: Impede que QUALQUER outro manipulador
        // !! no mesmo nível ou acima processe este evento WheelEvent.
        // !! Isso deve desativar qualquer zoom padrão remanescente.
        evt.StopImmediatePropagation();

        // Debug Log (opcional, remova depois de confirmar que funciona)
        // Debug.Log($"Pan applied: {positionChange}, New Position: {newPosition}");
    }

    /// <summary>
    /// Aplica zoom in, centralizado na view atual. Chamado pelo botão "+".
    /// </summary>
    public void ZoomIn()
    {
        ZoomWithFactor(1.0f + ZOOM_SPEED_BUTTON);
    }

    /// <summary>
    /// Aplica zoom out, centralizado na view atual. Chamado pelo botão "-".
    /// </summary>
    public void ZoomOut()
    {
        ZoomWithFactor(1.0f / (1.0f + ZOOM_SPEED_BUTTON)); // Usa o inverso para zoom out
    }

    /// <summary>
    /// Função auxiliar para aplicar zoom com um fator multiplicativo,
    /// centralizado no meio da view atual.
    /// </summary>
    /// <param name="factor">Fator multiplicativo da escala (ex: 1.1 para zoom in, 0.9 para zoom out).</param>
    private void ZoomWithFactor(float factor)
    {
        Vector3 oldScale = viewTransform.scale;
        float newScale = Mathf.Clamp(oldScale.x * factor, MIN_SCALE, MAX_SCALE);

        if (Mathf.Approximately(newScale, oldScale.x)) return;

        // Centro da view visível atual em coordenadas locais da GraphView
        Vector2 viewCenterLocal = layout.center;
        // Converte o centro da view para coordenadas do container de conteúdo
        Vector2 contentCenter = this.ChangeCoordinatesTo(contentViewContainer, viewCenterLocal);

        // Calcula a nova posição para manter o centro da view fixo
        Vector3 oldPosition = viewTransform.position;
        Vector3 newPosition = contentCenter - (contentCenter - (Vector2)oldPosition) * (newScale / oldScale.x);

        UpdateViewTransform(newPosition, new Vector3(newScale, newScale, 1f));
    }


    /// <summary>
    /// Atualiza o estilo visual dos nós para indicar qual é o nó de entrada.
    /// </summary>
    private void UpdateEntryNodeVisuals()
    {
        // Remove a classe de todos primeiro
        foreach (var nodeView in _nodeViews.Values)
        {
            nodeView?.RemoveFromClassList("entry-node");
        }
        // Adiciona a classe ao nó de entrada atual, se existir
        if (_currentDialogue?.EntryNode != null && _nodeViews.TryGetValue(_currentDialogue.EntryNode.Guid, out var entryNodeView))
        {
            entryNodeView?.AddToClassList("entry-node");
        }
    }

    /// <summary>
    /// Retorna o asset DialogueDataSO atualmente carregado nesta view.
    /// </summary>
    public DialogueDataSO GetCurrentDialogue() => _currentDialogue;

    /// <summary>
    /// Limpa a view atual e a popula com os nós e conexões do DialogueDataSO fornecido.
    /// </summary>
    /// <param name="dialogue">O asset de diálogo a ser carregado, ou null para limpar a view.</param>
    public void PopulateView(DialogueDataSO dialogue)
    {
        // Salva a posição/escala atual para tentar restaurar depois
        Vector3 previousPosition = viewTransform.position;
        Vector3 previousScale = viewTransform.scale;

        _currentDialogue = dialogue;

        // Desativa temporariamente o callback para evitar processamento durante a limpeza
        graphViewChanged -= OnGraphViewChanged;

        // Remove todos os elementos visuais existentes (nós, arestas)
        DeleteElements(graphElements.ToList());
        _nodeViews.Clear(); // Limpa o cache de views

        // Reativa o callback
        graphViewChanged += OnGraphViewChanged;

        // Se nenhum diálogo foi fornecido, reseta a view e termina
        if (_currentDialogue == null)
        {
            UpdateViewTransform(Vector3.zero, Vector3.one); // Reset view
            return;
        }

        // ---- Validação e Limpeza dos Dados ----
        // Garante que a lista de nós não seja nula
        if (_currentDialogue.AllNodes == null)
        {
            _currentDialogue.AllNodes = new List<DialogueNode>();
        }
        // Remove entradas nulas da lista (podem ocorrer por erros anteriores)
        int removedCount = _currentDialogue.AllNodes.RemoveAll(node => node == null);
        if (removedCount > 0)
        {
            Debug.LogWarning($"Removed {removedCount} null nodes from '{_currentDialogue.name}'. Saving the asset is recommended.");
            EditorUtility.SetDirty(_currentDialogue); // Marca para salvar a lista limpa
        }
        // ---------------------------------------

        // 1ª Passagem: Criar Views para cada Nó de Dados
        foreach (var nodeData in _currentDialogue.AllNodes)
        {
            // CreateNodeView já tem validação interna para nodeData nulo
            CreateNodeView(nodeData);
        }

        // 2ª Passagem: Criar Arestas (Edges) para as Conexões
        foreach (var nodeData in _currentDialogue.AllNodes)
        {
            // Se não existe view para este nó (ou nó é nulo), pula
            if (nodeData == null || !_nodeViews.TryGetValue(nodeData.Guid, out var parentView)) continue;

            // Obtém as informações de conexão que saem deste nó
            var connections = GetNodeConnections(nodeData);

            foreach (var connection in connections)
            {
                // Valida se o GUID de destino não é nulo/vazio e se existe uma view para ele
                if (!string.IsNullOrEmpty(connection.TargetGuid) && _nodeViews.TryGetValue(connection.TargetGuid, out var childView))
                {
                    // Obtém as portas de saída e entrada correspondentes
                    Port outputPort = parentView.GetOutputPort(connection.SourcePortName);
                    Port inputPort = childView?.InputPort; // Nó filho pode não ter porta de entrada (ex: EntryNode customizado)

                    // Se ambas as portas existem, cria e adiciona a aresta visual
                    if (outputPort != null && inputPort != null)
                    {
                        Edge edge = outputPort.ConnectTo(inputPort);
                        AddElement(edge);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not create edge. Missing port for connection: {parentView.title}({connection.SourcePortName}) -> {childView?.title}(Input).");
                    }
                }
                else if (!string.IsNullOrEmpty(connection.TargetGuid))
                {
                    // Log de erro se o GUID existe mas a view do nó de destino não foi encontrada
                    Debug.LogWarning($"Could not create edge. Target node view not found for GUID: {connection.TargetGuid} (from {parentView.title})");
                }
            }
        }

        // ---- Finalização ----
        UpdateEntryNodeVisuals(); // Garante que o nó de entrada esteja estilizado
        UpdateViewTransform(previousPosition, previousScale); // Tenta restaurar a posição/zoom anterior
        // FrameAll(); // Comentado - Enquadrar automaticamente pode ser irritante ao trocar de asset
    }

    /// <summary>
    /// Obtém as informações de conexão de saída para um nó de dados específico.
    /// </summary>
    /// <param name="nodeData">O nó de dados a ser analisado.</param>
    /// <returns>Uma lista de informações de conexão.</returns>
    private List<NodeConnectionInfo> GetNodeConnections(DialogueNode nodeData)
    {
        List<NodeConnectionInfo> connections = new List<NodeConnectionInfo>();
        if (nodeData is SentenceNode sn && !string.IsNullOrEmpty(sn.NextNodeGuid)) { connections.Add(new NodeConnectionInfo { SourcePortName = "Next", TargetGuid = sn.NextNodeGuid }); }
        else if (nodeData is ChoiceNode cn && cn.Choices != null) { for (int i = 0; i < cn.Choices.Count; i++) { if (cn.Choices[i] != null && !string.IsNullOrEmpty(cn.Choices[i].TargetNodeGuid)) { connections.Add(new NodeConnectionInfo { SourcePortName = $"Choice {i}", TargetGuid = cn.Choices[i].TargetNodeGuid }); } } }
        else if (nodeData is ConditionNode condN) { if (!string.IsNullOrEmpty(condN.TrueNodeGuid)) { connections.Add(new NodeConnectionInfo { SourcePortName = "True", TargetGuid = condN.TrueNodeGuid }); } if (!string.IsNullOrEmpty(condN.FalseNodeGuid)) { connections.Add(new NodeConnectionInfo { SourcePortName = "False", TargetGuid = condN.FalseNodeGuid }); } }
        // Adicionar mais 'else if' para outros tipos de nós, se houver
        return connections;
    }

    /// <summary>
    /// Estrutura auxiliar para armazenar informações de uma conexão ao popular a view.
    /// </summary>
    private struct NodeConnectionInfo { public string SourcePortName; public string TargetGuid; }

    /// <summary>
    /// Cria a instância da View (representação visual) apropriada para um nó de dados.
    /// </summary>
    /// <param name="nodeData">O nó de dados ScriptableObject.</param>
    private void CreateNodeView(DialogueNode nodeData)
    {
        // Valida se o nó de dados é válido e se já não existe uma view para ele
        if (nodeData == null || string.IsNullOrEmpty(nodeData.Guid) || _nodeViews.ContainsKey(nodeData.Guid))
        {
            if (nodeData != null && _nodeViews.ContainsKey(nodeData.Guid)) { /* Nó já existe, OK */ }
            else { Debug.LogError($"Cannot create node view. Invalid NodeData or duplicate GUID: {(nodeData?.Guid ?? "NULL")}"); }
            return;
        }

        DialogueNodeView nodeView = null;
        // Cria a view específica baseada no tipo de dado
        if (nodeData is SentenceNode sn) { nodeView = new SentenceNodeView(sn, this); }
        else if (nodeData is ChoiceNode cn) { nodeView = new ChoiceNodeView(cn, this); }
        else if (nodeData is ConditionNode condN) { nodeView = new ConditionNodeView(condN, this); }
        else
        {
            Debug.LogError($"No view implemented for DialogueNode type: {nodeData.GetType()}");
            return;
        }

        // Define a posição visual e adiciona ao grafo e ao cache
        nodeView.SetPosition(new Rect(nodeData.GraphPosition, DefaultNodeSize)); // Usa DefaultNodeSize, layout ajustará
        AddElement(nodeView);
        _nodeViews.Add(nodeData.Guid, nodeView);
    }

    /// <summary>
    /// Cria um novo nó de dados (ScriptableObject) e sua representação visual (View).
    /// </summary>
    /// <param name="type">O tipo de DialogueNode a ser criado.</param>
    /// <param name="screenMousePosition">A posição do mouse na tela onde o nó deve ser criado.</param>
    public void CreateNewNode(Type type, Vector2 screenMousePosition)
    {
        // Validações
        if (_currentDialogue == null) { EditorUtility.DisplayDialog("Error", "No Dialogue Asset loaded.", "OK"); return; }
        if (!typeof(DialogueNode).IsAssignableFrom(type)) { Debug.LogError($"Type {type} is not assignable from DialogueNode."); return; }

        // Agrupa operações para um único Undo/Redo
        Undo.SetCurrentGroupName($"Create {type.Name}");
        int undoGroup = Undo.GetCurrentGroup();

        // Cria o nó de dados usando o método do SO principal (que deve lidar com Undo e sub-assets)
        DialogueNode nodeData = _currentDialogue.CreateNode(type);
        if (nodeData == null)
        {
            Debug.LogError($"Failed to create NodeData of type {type}. CreateNode returned null.");
            Undo.CollapseUndoOperations(undoGroup); // Cancela o grupo se falhar
            return;
        }

        // Registra a adição do nó à lista do SO principal (se CreateNode não fizer)
        // Undo.RecordObject(_currentDialogue, "Add Node to Dialogue List");

        // Converte a posição do mouse para coordenadas locais do grafo
        Vector2 graphPosition = contentViewContainer.WorldToLocal(screenMousePosition);
        // Define posição e nome padrão nos dados (registra para Undo)
        Undo.RecordObject(nodeData, "Set Initial Node Properties");
        nodeData.GraphPosition = graphPosition;
        // Gera um nome inicial baseado no tipo e parte do GUID para unicidade
        nodeData.name = $"{type.Name.Replace("Node", "")}_{nodeData.Guid.Substring(0, 4)}";

        // Cria a representação visual para o novo nó de dados
        CreateNodeView(nodeData);

        // Marca os assets como modificados (geralmente já feito pelos RecordObject)
        EditorUtility.SetDirty(_currentDialogue);
        EditorUtility.SetDirty(nodeData);

        // Finaliza o grupo de Undo
        Undo.CollapseUndoOperations(undoGroup);
    }

    /// <summary>
    /// Define o nó de entrada (ponto de partida) do diálogo.
    /// </summary>
    /// <param name="nodeView">A view do nó a ser definido como entrada.</param>
    public void SetEntryPoint(DialogueNodeView nodeView)
    {
        // Validações
        if (_currentDialogue == null || nodeView?.NodeData == null)
        {
            Debug.LogError("Cannot set entry point: No dialogue loaded or invalid node view.");
            return;
        }
        if (_currentDialogue.EntryNode == nodeView.NodeData) return; // Já é o nó de entrada

        // Registra a mudança no asset principal para Undo
        Undo.RecordObject(_currentDialogue, "Set Dialogue Entry Point");
        // Define o novo nó de entrada nos dados
        _currentDialogue.EntryNode = nodeView.NodeData;
        // Marca o asset como modificado (Undo já faz)
        EditorUtility.SetDirty(_currentDialogue);
        // Atualiza a UI para refletir a mudança visualmente
        UpdateEntryNodeVisuals();
        Debug.Log($"Node '{nodeView.NodeData.name}' set as Entry Point for '{_currentDialogue.name}'.");
    }

    /// <summary>
    /// Callback principal para mudanças na estrutura do grafo (deleções, conexões).
    /// É responsável por sincronizar as mudanças visuais com os dados ScriptableObject, usando Undo.
    /// </summary>
    /// <param name="graphViewChange">O objeto contendo as informações sobre a mudança.</param>
    /// <returns>O mesmo objeto graphViewChange (ou modificado, se necessário).</returns>
    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        // --- Deleção de Elementos ---
        if (graphViewChange.elementsToRemove != null)
        {
            Undo.SetCurrentGroupName("Remove Dialogue Elements");
            int undoGroup = Undo.GetCurrentGroup();

            // Processa Arestas (Edges) Removidas Primeiro
            foreach (var element in graphViewChange.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    // Notifica os nós conectados sobre a desconexão ANTES de remover a view
                    (edge.output?.node as DialogueNodeView)?.DisconnectChild(edge); // Nó pai desconecta filho
                    (edge.input?.node as DialogueNodeView)?.OnInputDisconnected(edge); // Nó filho notificado da desconexão de entrada
                }
            }

            // Processa Nós (Nodes) Removidos
            foreach (var element in graphViewChange.elementsToRemove)
            {
                if (element is DialogueNodeView nodeView)
                {
                    if (_currentDialogue != null && nodeView.NodeData != null)
                    {
                        string nodeGuid = nodeView.NodeData.Guid; // Guarda GUID antes de deletar

                        // Remove o nó da lista do asset principal (registra para Undo)
                        Undo.RecordObject(_currentDialogue, "Remove Node from List");
                        bool removed = _currentDialogue.AllNodes.Remove(nodeView.NodeData);

                        if (removed)
                        {
                            // Destrói o asset ScriptableObject do nó (importante para sub-assets)
                            Undo.DestroyObjectImmediate(nodeView.NodeData);
                            // Remove a view do cache
                            _nodeViews.Remove(nodeGuid);
                            // Marca o asset principal como modificado
                            EditorUtility.SetDirty(_currentDialogue);
                        }
                        else
                        {
                            Debug.LogWarning($"Tried to remove node '{nodeView.title}' ({nodeGuid}) but it wasn't found in the DialogueDataSO list.");
                        }
                    }
                }
            }
            Undo.CollapseUndoOperations(undoGroup);
        }

        // --- Criação de Arestas (Edges) ---
        if (graphViewChange.edgesToCreate != null)
        {
            // A lógica de salvar a conexão nos dados é tratada pelo ConnectChild/OnInputConnected
            // das NodeViews, que já usam Undo.
            foreach (var edge in graphViewChange.edgesToCreate)
            {
                (edge.output?.node as DialogueNodeView)?.ConnectChild(edge); // Nó pai conecta filho
                (edge.input?.node as DialogueNodeView)?.OnInputConnected(edge); // Nó filho notificado da conexão de entrada
            }
        }

        // --- Movimentação de Elementos ---
        if (graphViewChange.movedElements != null)
        {
            // A lógica de salvar a nova posição é tratada pelo SetPosition
            // da DialogueNodeView, que já usa Undo.
            // Não precisamos fazer nada extra aqui para a posição.
        }

        return graphViewChange;
    }

    /// <summary>
    /// Define quais portas são compatíveis para iniciar uma conexão.
    /// Usado pelo GraphView para determinar onde uma nova aresta pode terminar.
    /// </summary>
    /// <param name="startPort">A porta de onde a conexão está sendo iniciada.</param>
    /// <param name="nodeAdapter">Adaptador de nó (geralmente não usado aqui).</param>
    /// <returns>Uma lista de portas compatíveis.</returns>
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        var startNodeView = startPort.node as DialogueNodeView; // Nó de origem

        // Itera por todas as portas na view
        ports.ForEach(port =>
        {
            var portNodeView = port.node as DialogueNodeView; // Nó da porta atual

            // Condições para compatibilidade:
            // 1. Não pode conectar uma porta a ela mesma.
            if (startPort == port) return;
            // 2. Não pode conectar portas do mesmo nó.
            if (startNodeView == portNodeView) return;
            // 3. Não pode conectar Input com Input ou Output com Output.
            if (startPort.direction == port.direction) return;
            // 4. Porta de destino não pode estar cheia (se for Single capacity).
            if (port.capacity == Port.Capacity.Single && port.connected) return;

            // Se passou por todas as verificações, a porta é compatível
            compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }

    /// <summary>
    /// Encontra uma NodeView (representação visual) pelo GUID do seu NodeData.
    /// </summary>
    /// <param name="guid">O GUID do nó a ser encontrado.</param>
    /// <returns>A DialogueNodeView correspondente, ou null se não encontrada.</returns>
    public DialogueNodeView FindNodeViewByGuid(string guid)
    {
        _nodeViews.TryGetValue(guid, out var view);
        return view;
    }
}


// --- Definição de NodeCreationSearchWindow (Provider para o menu de criação de nós) ---
public class NodeCreationSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialogueGraphView _graphView; // Referência à GraphView para criar o nó
    public void Initialize(DialogueGraphView graphView) { _graphView = graphView; }

    /// <summary>
    /// Cria a estrutura de árvore para a janela de busca.
    /// </summary>
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry> {
            new SearchTreeGroupEntry(new GUIContent("Create Dialogue Node"), 0), // Título do grupo principal
            // Adiciona entradas para cada tipo de nó, passando o Type como userData
            // Verifica se _graphView não é nulo para segurança
            _graphView != null ? new SearchTreeEntry(new GUIContent("Sentence Node")) { userData = typeof(SentenceNode), level = 1 } : null,
            _graphView != null ? new SearchTreeEntry(new GUIContent("Choice Node")) { userData = typeof(ChoiceNode), level = 1 } : null,
            _graphView != null ? new SearchTreeEntry(new GUIContent("Condition Node")) { userData = typeof(ConditionNode), level = 1 } : null,
            // Adicione mais tipos de nós aqui se necessário
        };
        // Remove entradas nulas (caso _graphView seja null)
        tree.RemoveAll(item => item == null);
        return tree;
    }

    /// <summary>
    /// Chamado quando um item na janela de busca é selecionado.
    /// </summary>
    /// <param name="entry">A entrada selecionada.</param>
    /// <param name="context">O contexto da janela de busca (inclui posição do mouse).</param>
    /// <returns>True se a seleção foi tratada, false caso contrário.</returns>
    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        // Obtém o tipo de nó do userData da entrada
        var type = entry.userData as Type;
        // Valida se o tipo é válido e se a GraphView existe
        if (type != null && _graphView != null && typeof(DialogueNode).IsAssignableFrom(type))
        {
            // Chama o método na GraphView para criar o nó na posição do mouse
            _graphView.CreateNewNode(type, context.screenMousePosition);
            return true; // Indica que a seleção foi tratada
        }
        return false; // Seleção não tratada (ex: grupo)
    }
}
