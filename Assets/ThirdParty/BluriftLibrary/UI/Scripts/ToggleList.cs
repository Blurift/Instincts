using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace Blurift.UI
{
    [AddComponentMenu("Blurift/UI/Toggle List")]
    public class ToggleList : MonoBehaviour
    {
        public RectTransform ListPanel;
        public GameObject ListItemPrefab;
        public ToggleGroup ListGroup;

        private List<string> values = new List<string>();
        private int selectedIndex = -1;

        public Button.ButtonClickedEvent OnValueChanged;

        private void Redraw()
        {
            for(int i = ListPanel.childCount-1; i > -1; i--)
            {
                GameObject child = ListPanel.GetChild(i).gameObject;
                child.transform.SetParent(null);
                Destroy(child);
            }

            if (values.Count > selectedIndex)
            {
                selectedIndex = -1;
            }

            for(int i = 0; i < values.Count; i++)
            {
                GameObject obj = Instantiate(ListItemPrefab) as GameObject;

                ToggleListItem listItem = obj.GetComponent<ToggleListItem>();

                listItem.Index = i;
                listItem.Value = values[i];
                listItem.Toggle.group = ListGroup;
                if (i == selectedIndex)
                    listItem.Toggle.isOn = true;
                listItem.OnClick += OnChange;
                listItem.SetText(values[i]);

                obj.transform.SetParent(ListPanel);
            }

            
        }


        #region Manipulation;

        public void AddItem(string value)
        {
            values.Add(value);
            Redraw();
        }

        public void DeleteCurrentItem()
        {
            if (selectedIndex > -1 && selectedIndex < values.Count)
            {
                values.RemoveAt(selectedIndex);
                selectedIndex = -1;
                Redraw();
            }
        }

        public void DeleteItemAt(int index)
        {
            if (index > -1 && index < values.Count)
            {
                values.RemoveAt(index);
                Redraw();
            }
        }

        /// <summary>
        /// Value of the currently selected item in the list;
        /// </summary>
        /// <returns>The current value (Blank string if no item selected).</returns>
        public string GetCurrentValue()
        {
            if(selectedIndex > -1)
                return values[selectedIndex];
            return "";
        }

        /// <summary>
        /// Index of the currently selected item in the list;
        /// </summary>
        /// <returns>The current index (-1 if no item selected).</returns>
        public int GetCurrentIndex()
        {
            return selectedIndex;
        }

        public string[] GetValues()
        {
            return values.ToArray();
        }

        /// <summary>
        /// Clear all items from the list.
        /// </summary>
        public void ClearItems()
        {
            values.Clear();
        }

        #endregion

        private void OnChange(int index)
        {
            selectedIndex = index;

            OnValueChanged.Invoke();
        }
    }
}