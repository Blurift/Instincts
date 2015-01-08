using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Entity/Character/HUD")]
public class HUDN : MonoBehaviour {

    public static HUDN Instance;
    public PlayerController Controller;
    public Inventory Inventory;

    #region Variables
    
    //Panels
    public RectTransform CanvasTransform;
    public GameObject InventoryPanel;
    public GameObject CraftingPanel;
    
    public GameObject MenuPanel;

    //Vitals
    public RectTransform VitalsHealth;
    public RectTransform VitalsStamina;
    public RectTransform VitalsHunger;

    private float healthTarget = 1;
    private float staminaTarget = 1;
    private float hungerTarget = 1;
    private float healthCurrent = 1;
    private float staminaCurrent = 1;
    private float hungerCurrent = 1;

    //Prefabs
    public GameObject CraftItemPrefab;

    //Crafting
    public RectTransform CraftingContent;
    public Text CraftingText;
    private CraftableItem craftingCurrent;

    //Inventory
    private HUDInvSlot[] invSlots;
    private HUDInvSlot[] equipSlots;
    public RectTransform[] equipPlaceHolders;
    public RectTransform InvPanel;
    public GameObject InvSlotPrefab;
    public Sprite EquippedNormal;
    public Sprite EquippedSelected;

    //OnScreen
    public GameObject Tooltip;
    public GameObject DraggedItemGameObject;
    public Item DraggedItem;
    public bool DraggingItem = false;

    //HelperText
    public RectTransform HelperTextContainer;
    public Text HelperTextContainerText;
    private float helperTextOffsetCurrent = 0;
    private float helperTextOffsetTarget = 0;
    private float helperTextStart = 0;
    private float helperTextDuration = 5;

    //Effects
    public Image BloodEffect;
    public AnimationCurve BloodCurve;
    private float bloodStart = 0;
    private float bloodDuration = 0.5f;

    #endregion

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    #region Initialize

    // Use this for initialization
	void Start () {
        

        //Populate The Inventory List
        if(InvPanel != null)
        {
            invSlots = new HUDInvSlot[20];
            equipSlots = new HUDInvSlot[6];

            int y = 0;
            int x = 0;
            for (int i = 0; i < invSlots.Length; i++)
            {
                GameObject slot = Instantiate(InvSlotPrefab) as GameObject;

                slot.name = "Slot" + x + "." + y;

                invSlots[i] = slot.GetComponent<HUDInvSlot>();
                invSlots[i].Slot = i;
                invSlots[i].SlotType = "I";

                slot.transform.SetParent(InvPanel);
                RectTransform rt = slot.GetComponent<RectTransform>();

                rt.anchorMin = new Vector2(x * 0.2f, 1 - (y * 0.25f + 0.25f));
                rt.anchorMax = new Vector2(x * 0.2f + 0.2f, 1 - (y * 0.25f));

                rt.offsetMax = new Vector2(-2, -2);
                rt.offsetMin = new Vector2(2, 2);

                rt.localRotation = InvPanel.localRotation;

                x++;
                if(x == 5)
                {
                    x = 0;
                    y++;
                }
            }

            for (int i = 0; i < equipSlots.Length; i++)
            {
                GameObject slot = Instantiate(InvSlotPrefab) as GameObject;

                slot.name = "Slot" + i;

                equipSlots[i] = slot.GetComponent<HUDInvSlot>();
                equipSlots[i].Slot = i;
                equipSlots[i].SlotType = "E";

                slot.transform.SetParent(equipPlaceHolders[i]);
                RectTransform rt = slot.GetComponent<RectTransform>();

                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);

                rt.offsetMax = new Vector2(-2, -2);
                rt.offsetMin = new Vector2(2, 2);
            }

        }

        HelperTextSetup();
	}

    #endregion

    // Update is called once per frame
	void Update ()
    {

        if(DraggingItem)
        {
            Vector3 mp = Input.mousePosition - CanvasTransform.localPosition;
            DraggedItemGameObject.GetComponent<RectTransform>().localPosition = mp;
        }

        HelperTextUpdate();
        HelperInputUpdate();
        SetVitals();
        BloodUpdate();
        InvPopulate();
        

        
	}

    #region States

    public static bool IsInventory()
    {
        return Instance.InventoryPanel.activeInHierarchy;
    }

    #endregion

    #region Toggle

    public static void ToggleInventory()
    {
        Instance.InventoryPanel.SetActive(!Instance.InventoryPanel.activeInHierarchy);
        Instance.CraftingPanel.SetActive(Instance.InventoryPanel.activeInHierarchy);
        Instance.CraftingPopulate();
    }

    public static void ToggleMenu()
    {
        Instance.MenuPanel.SetActive(!Instance.MenuPanel.activeInHierarchy);
    }

    public void Quit()
    {
        NetworkManager.Disconnect();
    }

    public void ShowDraggedItem(Item item)
    {
        DraggingItem = item;
        DraggedItemGameObject.SetActive(true);
        DraggedItemGameObject.GetComponent<Image>().sprite = Sprite.Create(item.Icon, new Rect(0, 0, item.Icon.width, item.Icon.height), Vector2.zero);
        DraggingItem = true;
    }

    public void HideDraggedItem()
    {
        DraggedItemGameObject.SetActive(false);
        DraggingItem = false;
    }

    #endregion

    #region Vitals

    private void SetVitals()
    {
        if (Controller != null)
        {
            float ht = healthTarget;
            healthTarget = (float)Controller.Health.Health / (float)Controller.Health.HealthMax;
            if (healthTarget < ht) bloodStart = Time.time;
            staminaTarget = (float)Controller.Health.Stamina / (float)Controller.Health.StaminaMax;
            hungerTarget = (float)Controller.Health.Hunger / (float)Controller.Health.HungerMax;
        }
        if (healthCurrent != healthTarget) healthCurrent = Mathf.Lerp(healthCurrent, healthTarget, Time.deltaTime * 2);
        if (staminaCurrent != staminaTarget) staminaCurrent = Mathf.Lerp(staminaCurrent, staminaTarget, Time.deltaTime * 2);
        if (hungerCurrent != hungerTarget) hungerCurrent = Mathf.Lerp(hungerCurrent, hungerTarget, Time.deltaTime * 2);


        VitalsHealth.anchorMax = new Vector2(1, healthCurrent);
        VitalsHealth.offsetMax = Vector2.zero;
        VitalsHealth.offsetMin = Vector2.zero;

        VitalsStamina.anchorMax = new Vector2(1, staminaCurrent);
        VitalsStamina.offsetMax = Vector2.zero;
        VitalsStamina.offsetMin = Vector2.zero;

        VitalsHunger.anchorMax = new Vector2(1, hungerCurrent);
        VitalsHunger.offsetMax = Vector2.zero;
        VitalsHunger.offsetMin = Vector2.zero;
    }

    private void BloodUpdate()
    {
        if (BloodEffect == null) return;

        float time = Time.time - bloodStart;

        if(time > bloodDuration)
        {
            BloodEffect.color = new Color(1, 1, 1, BloodCurve.Evaluate(time));
        }
        else
        {
            BloodEffect.color = new Color(1, 1, 1, 0);
        }
    }

    #endregion

    #region Crafting Window

    private void CraftingPopulate()
    {
        if(Inventory != null)
        {
            CraftableItem[] craftItems = CraftingManager.AvailableCrafts(Inventory);

            for (int i = CraftingContent.childCount - 1; i >= 0; i--)
            {
                GameObject c = CraftingContent.GetChild(i).gameObject;
                c.transform.SetParent(null);
                Destroy(c);
            }

            for (int i = 0; i < craftItems.Length; i++)
            {
                GameObject craftGO = (GameObject)Instantiate(CraftItemPrefab);
                HUDCraftItem craft = craftGO.GetComponent<HUDCraftItem>();
                craft.Image.sprite = Sprite.Create(craftItems[i].Item.Icon, new Rect(0, 0, craftItems[i].Item.Icon.width, craftItems[i].Item.Icon.height), Vector2.zero);
                craft.Text.text = craftItems[i].Item.Name;
                craft.HUD = this;
                craft.Item = craftItems[i];
                craft.Button.onClick = craft.ButtonEvent;
                craftGO.transform.SetParent(CraftingContent);
                craftGO.transform.localRotation = CraftingContent.transform.localRotation;
            }

            CraftingText.text = "";
        }
    }

    public void CraftSetItem(CraftableItem item)
    {
        craftingCurrent = item;
        CraftingText.text = item.Item.Name;
    }

    public void CraftItem()
    {
        if(craftingCurrent != null)
        {
            bool craftIt = true;
            for (int j = 0; j < craftingCurrent.ItemNeeded.Length; j++)
            {
                if (!Inventory.IsStackAvailable(craftingCurrent.ItemNeeded[j].name, craftingCurrent.ItemQNeeded[j]))
                {
                    craftIt = false;
                }
            }

            if (craftIt)
            {
                for (int j = 0; j < craftingCurrent.ItemNeeded.Length; j++)
                {
                    Inventory.RetrieveFromStack(craftingCurrent.ItemNeeded[j].name, craftingCurrent.ItemQNeeded[j]);
                }
                Inventory.AddToInventory(craftingCurrent.Item.name, craftingCurrent.Item.StackAmount);
            }

            CraftingPopulate();
        }
    }

    #endregion

    #region Inventory

    private void InvPopulate()
    {
        if(Inventory != null)
        {
            for (int i = 0; i < Inventory.Items.Length; i++)
            {
                Item item = Inventory.Items[i];
                invSlots[i].Setup(item);
            }

            for (int i = 0; i < Inventory.Equipment.Length; i++)
            {
                Item item = Inventory.Equipment[i];
                equipSlots[i].Setup(item);
                if (i == Inventory.SelectedIndex)
                {
                    equipSlots[i].Background.sprite = EquippedSelected;
                    equipSlots[i].Background.color = new Color(1, 1, 1, 1);
                }
                else
                {
                    equipSlots[i].Background.sprite = null;
                    equipSlots[i].Background.color = new Color(0, 0, 0, 0);
                }
            }
        }
    }
    #endregion

    #region Helper Text

    private void HelperTextSetup()
    {
        helperTextOffsetTarget = -HelperTextContainer.rect.width;
        helperTextOffsetCurrent = helperTextOffsetTarget;
        HelperTextContainer.localPosition = new Vector3(helperTextOffsetCurrent, HelperTextContainer.localPosition.y, 0);
    }

    private void HelperTextUpdate()
    {
        //Debug.Log(helperTextOffsetTarget);
        if (helperTextOffsetCurrent != helperTextOffsetTarget)
        {
            helperTextOffsetCurrent = Mathf.Lerp(helperTextOffsetCurrent, helperTextOffsetTarget, Time.deltaTime * 3);
            HelperTextContainer.localPosition = new Vector3(helperTextOffsetCurrent, HelperTextContainer.localPosition.y, 0);
        }

        if (Time.time - helperTextStart > helperTextDuration)
            helperTextOffsetTarget = -HelperTextContainer.rect.width;
    }

    public static void HelperText(string text)
    {
        Instance.helperTextStart = Time.time;
        Instance.helperTextOffsetTarget = 0;
        Instance.HelperTextContainerText.text = text;
    }

    public Text HelperInputText;
    private float helperInputStart;
    private float helperInputDuration;

    private void HelperInputUpdate()
    {
        if (Time.time - helperInputStart > helperInputDuration)
            HelperInputText.color = new Color(1, 1, 1, 0);
    }

    public static void HelperInput(string text,string key, float time)
    {
        Instance.HelperInputText.text = "Press (" + key + ") " + text;
        Instance.helperInputStart = Time.time;
        Instance.helperInputDuration = time;
        Instance.HelperInputText.color = new Color(1, 1, 1, 1);
    }

    #endregion
}
