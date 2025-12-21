using UnityEngine;
using UnityEngine.UI;


// класс для отрисовки иконок с наградами
public class UIController : MonoBehaviour
{

    [SerializeField]
    private GameObject[] iconReward;

    [SerializeField]
    private HouseManager rewardManager; 
    [SerializeField, Tooltip("Data")]
     private BuildingDatabaseSO buildingDatabase;

    private void OnEnable()
    {
        HouseManager.CanTakeReward += SpawnRewardButton;
    }

    private void SpawnRewardButton(HouseData houseData)
    {
        Button b = Instantiate(iconReward[(int)buildingDatabase.GetById(houseData.buildingId).resourceType],
            houseData.transform.position + (Vector3.up * 5), transform.rotation, transform)
            .GetComponent<Button>();
        b.onClick.AddListener(() => rewardManager.TryClaimHouse(houseData.id));
        b.onClick.AddListener(() => houseData.iRedy = false);
        b.onClick.AddListener(() => Destroy(b.gameObject));
    }

}
