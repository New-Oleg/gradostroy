using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    [SerializeField]
    private GameObject iconReward;

    [SerializeField]
    private RewardManager rewardManager;

    private void OnEnable()
    {
        RewardManager.CanTackeRevard += SpawnRewardButton;
    }

    private void SpawnRewardButton(HouseData houseData)
    {
        Button b = Instantiate(iconReward, houseData.transform.position + (Vector3.up * 5), transform.rotation, transform)
        .GetComponent<Button>();

        b.onClick.AddListener(() => rewardManager.TryClaimHouse(houseData.id));
        b.onClick.AddListener(() => houseData.iRedy = false);
        b.onClick.AddListener(() => Destroy(b.gameObject));
    }

}
