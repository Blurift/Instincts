using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[AddComponentMenu("Entity/HUD Craft Item")]
public class HUDCraftItem : MonoBehaviour {
    public Image Image;
    public Text Text;
    public CraftableItem Item;
    public Button Button;
    public Button.ButtonClickedEvent ButtonEvent;

    public HUDN HUD;

    public void Click()
    {
        HUD.CraftSetItem(Item);
    }
}
