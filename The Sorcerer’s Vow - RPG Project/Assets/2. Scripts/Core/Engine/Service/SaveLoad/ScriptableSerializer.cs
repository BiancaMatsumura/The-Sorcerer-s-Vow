using System;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableSerializer
{
    /// <summary>
    /// Serializa uma lista de ScriptableObjects em um JSON contendo as informações importantes (ex. nomes).
    /// </summary>
    public static string SerializeList<T>(List<T> scriptableObjects) where T : ScriptableObject
    {
        List<string> objectNames = new List<string>();

        // Adiciona o nome de cada ScriptableObject na lista
        foreach (var obj in scriptableObjects)
        {
            if (obj != null)
            {
                objectNames.Add(obj.name); // Pega o nome do asset
            }
        }

        // Retorna o JSON da lista de nomes
        return JsonUtility.ToJson(new SerializationContainer<string> { items = objectNames }, true);
    }

    /// <summary>
    /// Deserializa uma lista de nomes JSON em uma lista de ScriptableObjects.
    /// </summary>
    public static List<T> DeserializeList<T>(string json) where T : ScriptableObject
    {
        // Desserializa o JSON para pegar os nomes que estavam salvos
        SerializationContainer<string> container = JsonUtility.FromJson<SerializationContainer<string>>(json);

        List<T> scriptableObjects = new List<T>();

        // Procura os ScriptableObjects pelo nome usando Resources.Load
        foreach (var name in container.items)
        {
            T obj = Resources.Load<T>(name); // Carrega o ScriptableObject pelo nome
            if (obj != null)
            {
                scriptableObjects.Add(obj);
            }
            else
            {
                Debug.LogWarning($"ScriptableObject do tipo {typeof(T).Name} com o nome '{name}' não foi encontrado em Resources.");
            }
        }

        return scriptableObjects;
    }

    /// <summary>
    /// Classe intermediária para serializar listas em JSON.
    /// </summary>
    [Serializable]
    private class SerializationContainer<T>
    {
        public List<T> items;
    }
}