using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Blurift.UI
{
    [AddComponentMenu("Blurift/UI/Toggle List Item")]
    public class ToggleListItem : MonoBehaviour
    {
        public delegate void ClickEvent(int i);

        public event ClickEvent OnClick;

        public Text text;
        public Toggle Toggle;

        public string Value;
        public int Index;

        public void SetText(string value)
        {
            text.text = value;
        }

        public void Change()
        {
            if (OnClick != null && Toggle.isOn)
                OnClick(Index);
        }
    }
}
