using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private float offset = 50f;   // насколько сдвигаем влево

    [SerializeField]
    private float duration = 0.25f; // время анимации


    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private GameObject DirPanel;

    private bool IsFix;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DirPanel) DirPanel.SetActive(false);
        DirPanel = eventData.hovered[0].GetComponentsInChildren<RectTransform>(true).First(o => o.tag == "Panel").gameObject;

        rectTransform.DOAnchorPos(originalPosition + Vector2.up * offset, duration)
            .SetEase(Ease.OutQuad);

        DirPanel.SetActive(true);

    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsFix)
        {
            rectTransform.DOAnchorPos(originalPosition, duration)
                .SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        IsFix = !IsFix;
    }
}