using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class HUDInvSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler {

    public Item Item;
    public Image Image;
    public Image Background;
    public Text StackText;
    public int Slot;
    public string SlotType = "I";

    public static int TargetIndex = 0;
    public static string TargetType = "I";
    public static int SourceIndex = 0;
    public static string SourceType = "I";

    public void Setup(Item item)
    {
        Item = item;

        if(item == null)
        {
            StackText.gameObject.SetActive(false);
            Image.gameObject.SetActive(false);
            return;
        }
        if (item.Stackable)
        {
            StackText.text = item.StackAmount.ToString();
            StackText.gameObject.SetActive(true);
        }
        else
            StackText.gameObject.SetActive(false);
        Image.sprite = Sprite.Create(item.Icon, new Rect(0, 0, item.Icon.width, item.Icon.height), Vector2.zero);
        Image.gameObject.SetActive(true);
        
    }

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
