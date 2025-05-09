using System.Collections.Generic;
using System.Linq; // Necessário para OfType<T>, FirstOrDefault, IndexOf etc.
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements; // Necessário para ObjectField

/// <summary>
/// Representação visual para um nó de diálogo do tipo Escolha do Jogador.
/// Lida com a exibição e edição das opções de escolha e suas conexões.
/// </summary>
public class ChoiceNodeView : DialogueNodeView
{
    // Acesso seguro e tipado aos dados específicos deste nó
    private ChoiceNode _choiceNodeData => NodeData as ChoiceNode;

    // Mapeia o NOME DA PORTA (ex: "Choice 0") para a INSTÂNCIA DA PORTA visual
    // Usado para recuperar a porta correta ao carregar/desconectar arestas.
    private readonly Dictionary<string, Port> _outputPorts = new Dictionary<string, Port>();

    // Container visual específico para os elementos das escolhas (campos de texto, botões)
    private VisualElement _choicesContainer;

    /// <summary>
    /// Construtor para a view do nó de escolha.
    /// </summary>
    public ChoiceNodeView(ChoiceNode nodeData, DialogueGraphView graphView)
        : base(nodeData, graphView) // Chama o construtor da classe base
    {
        // O construtor base já chama CreateOutputPorts e SetupDataBinding
        // O título também é definido na classe base
    }

    /// <summary>
    /// Adiciona a classe USS específica para este tipo de nó para estilização.
    /// </summary>
    protected override void SetupClasses()
    {
        // base.SetupClasses(); // Chama se a base tiver lógica de classe genérica
        AddToClassList("choice-node");
    }

    /// <summary>
    /// Cria/Recria as portas de saída visuais, uma para cada escolha nos dados,
    /// e atualiza o dicionário `_outputPorts`.
    /// Implementação do método abstrato da classe base.
    /// </summary>
    protected override void CreateOutputPorts()
    {
        // Validações iniciais
        if (_choiceNodeData == null)
        {
            Debug.LogError($"[ChoiceNodeView CreateOutputPorts] NodeData is null for node GUID: {NodeData?.Guid}");
            return;
        }
        // Garante que a lista de escolhas exista (importante se o SO foi criado sem inicializar)
        if (_choiceNodeData.Choices == null)
        {
            _choiceNodeData.Choices = new List<PlayerChoice>();
            Debug.LogWarning($"ChoiceNode ({_choiceNodeData.name}) had a null Choices list. Initialized.");
            // Não marcar dirty aqui, pois pode ser chamado antes de SetupDataBinding
        }

        // Limpa o container visual das portas e o dicionário de referências
        outputContainer.Clear();
        _outputPorts.Clear();

        // Cria uma porta para cada escolha válida existente nos dados
        for (int i = 0; i < _choiceNodeData.Choices.Count; i++)
        {
            if (_choiceNodeData.Choices[i] != null)
            {
                string portName = $"Choice {i}";
                // Usa o método auxiliar AddOutputPort da classe base para criar e adicionar a porta
                Port port = AddOutputPort(portName, Port.Capacity.Single, $"{i + 1}) "); // Label visual ex: "1) "
                if (port != null)
                {
                    // Armazena a REFERÊNCIA da porta no dicionário para acesso rápido
                    _outputPorts[portName] = port;
                }
            }
            else
            {
                Debug.LogWarning($"[ChoiceNodeView CreateOutputPorts] Null choice data found at index {i} in ChoiceNode ({_choiceNodeData.name}). Skipping port creation for this index.");
            }
        }
        // Garante que a UI do nó se atualize para refletir as mudanças nas portas
        // (útil se chamado fora do fluxo normal de construção)
        RefreshPorts();
    }

    /// <summary>
    /// Cria os campos de UI para editar o Ator (opcional), o Prompt (texto antes das opções),
    /// o botão "+ Add Choice", e inicializa a área visual das escolhas.
    /// Implementação do método abstrato da classe base.
    /// </summary>
    protected override void SetupDataBinding()
    {
        // Validações
        if (_choiceNodeData == null)
        {
            Debug.LogError($"[ChoiceNodeView SetupDataBinding] NodeData is invalid (not a ChoiceNode) for GUID: {NodeData?.Guid}. Cannot bind data.");
            mainContainer.Add(new Label("Error: Node data is invalid!") { style = { color = Color.red } });
            return;
        }
        // Garante novamente que a lista exista
        if (_choiceNodeData.Choices == null)
        {
            _choiceNodeData.Choices = new List<PlayerChoice>();
            EditorUtility.SetDirty(_choiceNodeData); // Marca dirty aqui pois é uma correção de dados
        }

        // --- Campo Ator (Opcional) ---
        var actorField = new ObjectField("Actor:")
        {
            objectType = typeof(CharacterDataSO),
            allowSceneObjects = false,
            value = _choiceNodeData.ActorData,
            tooltip = "(Optional) Character presenting the choices."
        };
        actorField.RegisterValueChangedCallback(evt =>
        {
            if (_choiceNodeData.ActorData != (evt.newValue as CharacterDataSO))
            {
                Undo.RecordObject(_choiceNodeData, "Change Choice Actor");
                _choiceNodeData.ActorData = evt.newValue as CharacterDataSO;
                EditorUtility.SetDirty(_choiceNodeData);
            }
        });
        mainContainer.Add(actorField); // Adiciona ao corpo principal do nó

        // --- Campo Prompt (Texto antes das opções) ---
        var promptField = new TextField("Prompt:")
        {
            multiline = true,
            value = _choiceNodeData.PromptContent,
            tooltip = "Text displayed before the choices appear (e.g., 'What do you do?')."
        };
        promptField.AddToClassList("dialogue-text-field"); // Para estilo USS
        promptField.style.whiteSpace = WhiteSpace.Normal;
        promptField.style.minWidth = 250; // Garante largura mínima
        promptField.style.minHeight = 40; // Altura mínima para multiline
        promptField.RegisterValueChangedCallback(evt =>
        {
            if (_choiceNodeData.PromptContent != evt.newValue)
            {
                Undo.RecordObject(_choiceNodeData, "Change Choice Prompt");
                _choiceNodeData.PromptContent = evt.newValue;
                EditorUtility.SetDirty(_choiceNodeData);
            }
        });
        mainContainer.Add(promptField);

        // --- Container para a UI das Escolhas ---
        // Usar um container dedicado facilita limpar e recriar apenas esta seção
        _choicesContainer = new VisualElement { name = "choices-container" };
        mainContainer.Add(_choicesContainer);

        // --- Botão Adicionar Escolha ---
        var addButton = new Button(AddChoice)
        {
            text = "+ Add Choice",
            tooltip = "Add a new choice option to this node."
        };
        // Adiciona o botão ao container no canto superior direito do título (titleButtonContainer)
        titleButtonContainer.Add(addButton);

        // --- Desenha a UI Inicial para as Escolhas Existentes ---
        // As portas já devem ter sido criadas pelo CreateOutputPorts chamado no construtor base
        DrawChoicesUI();

        // Garante que o nó tenha o tamanho correto inicialmente após adicionar todos os elementos
        RefreshExpandedState();
        // RefreshPorts(); // Não é estritamente necessário aqui, pois CreateOutputPorts já fez isso.
    }

    /// <summary>
    /// Limpa e recria APENAS a UI dos campos de edição para todas as escolhas
    /// atualmente nos dados. Não mexe nas portas diretamente aqui.
    /// </summary>
    private void DrawChoicesUI()
    {
        // Validações
        if (_choiceNodeData == null || _choicesContainer == null) return;
        if (_choiceNodeData.Choices == null) _choiceNodeData.Choices = new List<PlayerChoice>();

        // Limpa a UI antiga das escolhas do seu container dedicado
        _choicesContainer.Clear();

        // Recria a UI para cada escolha válida na lista de dados
        for (int i = 0; i < _choiceNodeData.Choices.Count; i++)
        {
            if (_choiceNodeData.Choices[i] != null)
            {
                // Passa o índice para saber qual escolha desenhar
                DrawSingleChoiceUI(i);
            }
            else
            {
                Debug.LogError($"[ChoiceNodeView DrawChoicesUI] Null choice data found at index {i} in ChoiceNode ({_choiceNodeData.name}). Skipping UI draw.");
                // Opcional: Remover o item nulo da lista aqui com Undo?
                // Undo.RecordObject(_choiceNodeData, "Remove Null Choice Entry");
                // _choiceNodeData.Choices.RemoveAt(i);
                // EditorUtility.SetDirty(_choiceNodeData);
                // i--; // Ajusta o índice após remoção
            }
        }

        // Atualiza o layout do nó para ajustar ao novo conteúdo da UI
        RefreshExpandedState();
    }

    /// <summary>
    /// Cria a UI (campos de texto/condição, botão delete) para UMA escolha específica,
    /// identificada pelo seu índice na lista de dados.
    /// </summary>
    /// <param name="index">O índice da escolha na lista _choiceNodeData.Choices.</param>
    private void DrawSingleChoiceUI(int index)
    {
        // Validações robustas
        if (_choiceNodeData == null || _choiceNodeData.Choices == null || index < 0 || index >= _choiceNodeData.Choices.Count || _choiceNodeData.Choices[index] == null)
        {
            Debug.LogError($"[ChoiceNodeView DrawSingleChoiceUI] Aborted due to invalid index ({index}) or null choice data.");
            return;
        }

        // Obtém a referência ao objeto de dados da escolha ATUALMENTE neste índice
        // É crucial usar essa referência nos callbacks, pois o índice pode mudar se itens forem removidos/reordenados
        PlayerChoice choiceData = _choiceNodeData.Choices[index];

        // --- Container da Linha da Escolha (para agrupar campos horizontalmente) ---
        var choiceRowContainer = new VisualElement();
        choiceRowContainer.name = $"choice-row-{index}"; // Nome para possível estilização USS específica
        choiceRowContainer.AddToClassList("choice-row"); // Classe genérica para estilo USS
        choiceRowContainer.style.flexDirection = FlexDirection.Row;
        choiceRowContainer.style.alignItems = Align.Center; // Alinha itens verticalmente
        choiceRowContainer.style.marginLeft = 5; // Pequena indentação

        // --- Campo Texto da Escolha ---
        var textField = new TextField()
        {
            value = choiceData.ChoiceText, // Valor inicial
            multiline = false, // Apenas uma linha para o texto da escolha
            tooltip = "Text of the choice option displayed to the player."
        };
        textField.style.flexGrow = 1; // Permite que o campo expanda na largura
        textField.style.minWidth = 100; // Largura mínima
        // Callback para atualizar dados quando o texto muda
        textField.RegisterValueChangedCallback(evt => {
            // Usa a referência 'choiceData' capturada no escopo para garantir que atualize o objeto correto
            if (choiceData.ChoiceText != evt.newValue)
            {
                Undo.RecordObject(_choiceNodeData, "Change Choice Text"); // Registra mudança no SO pai (ChoiceNode)
                choiceData.ChoiceText = evt.newValue; // Atualiza o dado capturado
                EditorUtility.SetDirty(_choiceNodeData); // Marca pai como dirty
            }
        });

        // --- Campo Condição da Escolha (Opcional) ---
        var conditionField = new ObjectField()
        {
            objectType = typeof(ConditionSO), // Filtra assets do tipo ConditionSO
            allowSceneObjects = false,
            value = choiceData.AvailabilityCondition, // Valor inicial
            tooltip = "(Optional) Condition Asset. Choice only available if condition is met."
        };
        conditionField.style.width = 70; // Largura fixa compacta
        conditionField.style.marginLeft = 5; // Espaço antes do campo
        // Callback para atualizar dados quando a condição muda
        conditionField.RegisterValueChangedCallback(evt => {
            ConditionSO newCondition = evt.newValue as ConditionSO;
            if (choiceData.AvailabilityCondition != newCondition)
            {
                Undo.RecordObject(_choiceNodeData, "Change Choice Condition"); // Registra no pai
                choiceData.AvailabilityCondition = newCondition; // Atualiza o dado capturado
                EditorUtility.SetDirty(_choiceNodeData); // Marca pai como dirty
            }
        });

        // --- Botão Remover Escolha ---
        // A action lambda captura o índice ATUAL 'i' para passar ao método RemoveChoice
        int currentIndex = index; // Captura o valor atual do índice para a lambda
        var deleteButton = new Button(() => RemoveChoice(currentIndex))
        {
            text = "X",
            tooltip = "Remove this choice option."
        };
        deleteButton.AddToClassList("delete-button"); // Classe para estilo USS
        // Aplica estilo compacto ao botão de remoção
        deleteButton.style.width = 20; deleteButton.style.height = 20;
        deleteButton.style.marginLeft = 5; deleteButton.style.paddingLeft = 4; deleteButton.style.paddingRight = 4;

        // --- Montagem da UI da Linha ---
        choiceRowContainer.Add(textField);          // Adiciona campo de texto à linha
        choiceRowContainer.Add(conditionField);     // Adiciona campo de condição à linha
        choiceRowContainer.Add(deleteButton);       // Adiciona botão de deletar à linha
        _choicesContainer.Add(choiceRowContainer); // Adiciona a linha completa ao container de escolhas
    }

    /// <summary>
    /// Adiciona uma nova escolha padrão aos dados e atualiza a UI e as portas.
    /// </summary>
    private void AddChoice()
    {
        if (_choiceNodeData == null) return;
        if (_choiceNodeData.Choices == null) _choiceNodeData.Choices = new List<PlayerChoice>(); // Segurança

        // Registra a modificação na lista para Undo
        Undo.RecordObject(_choiceNodeData, "Add Dialogue Choice");

        // Cria e adiciona a nova escolha
        PlayerChoice newChoice = new PlayerChoice { ChoiceText = $"New Choice {_choiceNodeData.Choices.Count + 1}" };
        _choiceNodeData.Choices.Add(newChoice);

        // Marca o asset como modificado (Undo já faz, mas reforça)
        EditorUtility.SetDirty(_choiceNodeData);

        // Ordem: Recria as portas PRIMEIRO, depois redesenha a UI das escolhas
        CreateOutputPorts(); // Atualiza o dicionário _outputPorts e adiciona a nova porta visual
        DrawChoicesUI();     // Recria a UI para todas as escolhas, incluindo a nova
    }

    /// <summary>
    /// Remove uma escolha (identificada pelo seu índice) dos dados, desconecta sua porta
    /// e atualiza a UI e as portas restantes.
    /// </summary>
    /// <param name="indexToRemove">O índice da escolha a ser removida na lista `_choiceNodeData.Choices`.</param>
    private void RemoveChoice(int indexToRemove)
    {
        // Validação robusta do índice
        if (_choiceNodeData == null || _choiceNodeData.Choices == null || indexToRemove < 0 || indexToRemove >= _choiceNodeData.Choices.Count)
        {
            Debug.LogError($"[ChoiceNodeView RemoveChoice] Cannot remove choice: Invalid index {indexToRemove} (List Count: {_choiceNodeData.Choices?.Count ?? -1}).");
            return;
        }

        // Nome da porta correspondente ao índice que será removido
        string portNameToRemove = $"Choice {indexToRemove}";

        // 1. Desconectar Aresta Visualmente ANTES de remover dados/porta
        // Usa o dicionário _outputPorts para pegar a referência da porta visual
        if (_outputPorts.TryGetValue(portNameToRemove, out Port portToRemove) && (portToRemove?.connected ?? false))
        {
            // Pega a primeira (e única, pois a capacidade é Single) conexão
            Edge edge = portToRemove.connections.FirstOrDefault();
            if (edge != null)
            {
                // Deletar a aresta da GraphView (isso acionará DisconnectChild no nó pai via OnGraphViewChanged)
                GraphView.DeleteElements(new List<GraphElement> { edge });
            }
        }

        // 2. Registrar a Remoção do Item da Lista para Undo
        Undo.RecordObject(_choiceNodeData, "Remove Dialogue Choice");

        // 3. Remover a Escolha da Lista de Dados usando o índice validado
        _choiceNodeData.Choices.RemoveAt(indexToRemove);

        // 4. Marcar Dirty (feito pelo Undo, mas reforça)
        EditorUtility.SetDirty(_choiceNodeData);

        // 5. Recriar as Portas e Redesenhar a UI das Escolhas
        // É crucial recriar as portas DEPOIS de remover o item dos dados,
        // pois isso ajustará os nomes ("Choice 0", "Choice 1", etc.) e o dicionário _outputPorts.
        CreateOutputPorts(); // Recria todas as portas com os nomes/índices corretos
        DrawChoicesUI();     // Redesenha a UI das escolhas restantes
    }

    /// <summary>
    /// Retorna a instância da porta de saída específica de uma escolha, buscando pelo nome no dicionário.
    /// Método sobrescrito da classe base.
    /// </summary>
    /// <param name="portName">O nome da porta (ex: "Choice 0").</param>
    /// <returns>A instância da `Port`, ou `null` se não encontrada.</returns>
    public override Port GetOutputPort(string portName)
    {
        _outputPorts.TryGetValue(portName, out Port port);
        // Log opcional para depuração se a porta não for encontrada
        // if (port == null) Debug.LogWarning($"[ChoiceNodeView GetOutputPort] Node '{NodeData?.name}' FAILED to find port '{portName}' in dictionary.");
        return port; // Retorna a porta encontrada ou null
    }

    /// <summary>
    /// Chamado quando uma aresta é conectada a uma das portas de saída de escolha.
    /// Atualiza o `TargetNodeGuid` na escolha correspondente nos dados.
    /// Implementação do método abstrato da classe base.
    /// </summary>
    /// <param name="edge">A aresta que foi criada.</param>
    public override void ConnectChild(Edge edge)
    {
        Port outputPort = edge.output; // Porta de saída DESTE nó (ChoiceNodeView)
        DialogueNodeView childView = edge.input?.node as DialogueNodeView; // Nó de destino

        // Valida se a porta de saída pertence a este nó (verificando no dicionário) e se o nó filho é válido
        if (outputPort != null && _outputPorts.ContainsValue(outputPort) && childView?.NodeData != null)
        {
            string portName = outputPort.portName;
            // Tenta encontrar o índice correspondente ao nome da porta conectada
            int choiceIndex = -1;
            // O nome da porta é "Choice N", então podemos extrair N
            if (portName.StartsWith("Choice ") && int.TryParse(portName.Substring("Choice ".Length), out int index))
            {
                choiceIndex = index;
            }

            // Valida se o índice encontrado está dentro dos limites da lista de escolhas atual
            if (choiceIndex != -1 && choiceIndex >= 0 && choiceIndex < _choiceNodeData.Choices.Count)
            {
                // Verifica se a conexão realmente mudou antes de registrar Undo
                if (_choiceNodeData.Choices[choiceIndex].TargetNodeGuid != childView.NodeData.Guid)
                {
                    Undo.RecordObject(_choiceNodeData, "Connect Choice Node");
                    // Atualiza o GUID de destino na escolha correta
                    _choiceNodeData.Choices[choiceIndex].TargetNodeGuid = childView.NodeData.Guid;
                    EditorUtility.SetDirty(_choiceNodeData);
                }
            }
            else { Debug.LogError($"[ChoiceNodeView ConnectChild] Error: Could not find valid index ({choiceIndex}) for connected port '{portName}'. Choices count: {_choiceNodeData.Choices.Count}"); }
        }
        else { Debug.LogWarning($"[ChoiceNodeView ConnectChild] Failed: Invalid output port reference, child view/data, or port not in dictionary. Port: {outputPort?.portName}"); }
    }

    /// <summary>
    /// Chamado quando uma aresta saindo de uma porta de escolha é desconectada.
    /// Limpa o `TargetNodeGuid` na escolha correspondente nos dados.
    /// Implementação do método abstrato da classe base.
    /// </summary>
    /// <param name="edge">A aresta que foi removida.</param>
    public override void DisconnectChild(Edge edge)
    {
        Port outputPort = edge.output; // Porta de saída DESTE nó

        // Valida se a porta de saída pertence a este nó (verificando no dicionário)
        if (outputPort != null && _outputPorts.ContainsValue(outputPort))
        {
            string portName = outputPort.portName;
            // Tenta encontrar o índice correspondente ao nome da porta desconectada
            int choiceIndex = -1;
            if (portName.StartsWith("Choice ") && int.TryParse(portName.Substring("Choice ".Length), out int index))
            {
                choiceIndex = index;
            }

            // Valida o índice e se a entrada de dados para essa escolha existe
            if (choiceIndex != -1 && choiceIndex >= 0 && choiceIndex < _choiceNodeData.Choices.Count && _choiceNodeData.Choices[choiceIndex] != null)
            {
                // Verifica se já não está nulo para evitar Undo desnecessário
                if (!string.IsNullOrEmpty(_choiceNodeData.Choices[choiceIndex].TargetNodeGuid))
                {
                    Undo.RecordObject(_choiceNodeData, "Disconnect Choice Node");
                    // Limpa o GUID de destino na escolha correta
                    _choiceNodeData.Choices[choiceIndex].TargetNodeGuid = null;
                    EditorUtility.SetDirty(_choiceNodeData);
                }
            }
            else { Debug.LogError($"[ChoiceNodeView DisconnectChild] Error: Could not find valid index ({choiceIndex}) for disconnected port '{portName}' or choice data is null. Choices count: {_choiceNodeData.Choices.Count}"); }
        }
        else { Debug.LogWarning($"[ChoiceNodeView DisconnectChild] Failed: Invalid output port reference or port not in dictionary. Port: {outputPort?.portName}"); }
    }
}
