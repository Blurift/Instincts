using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class HUDInvSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler {

    public Item Item;
    public Image Image;
    public int Slot;
    public string SlotType = "I";

    public static int TargetIndex = 0;
    public static string TargetType = "I";
    public static int SourceIndex = 0;
    public static string SourceType = "I";

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Item != null)
        {
            HUDN.Instance.ShowDraggedItem(Item);
            SourceIndex = Slot;
            SourceType = SlotType;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("We Released");
        if(HUDN.Instance.DraggingItem)
        {
            if (HUDN.Instance.Inventory != null)
            {
                HUDN.Instance.Inventory.MoveItem(TargetIndex,TargetType,SourceIndex,SourceType);
            }
            HUDN.Instance.HideDraggedItem();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TargetIndex = Slot;
        TargetType = SlotType;
    }
}
