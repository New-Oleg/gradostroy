using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingDatabaseSO", menuName = "Scriptable Objects/BuildingDatabaseSO")]
public class BuildingDatabaseSO : ScriptableObject
{
    public List<BuildingSO> buildings = new List<BuildingSO>();

    // Быстрый поиск по id
    private Dictionary<string, BuildingSO> map;

    // Ручной вызов при редактировании / загрузке данных
    public void Refresh()
    {
        map = buildings
            .Where(b => b != null && !string.IsNullOrEmpty(b.id))
            .GroupBy(b => b.id)
            .ToDictionary(g => g.Key, g => g.First());
    }

    // Получить BuildingSO по id (или null)
    public BuildingSO GetById(string id)
    {
        if (map == null) Refresh();
        if (string.IsNullOrEmpty(id)) return null;
        map.TryGetValue(id, out BuildingSO so);
        return so;
    }
}
