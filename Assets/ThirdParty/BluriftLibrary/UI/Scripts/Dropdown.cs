using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Blurift.UI
{

    [AddComponentMenu("Blurift/UI/Dropdown Item")]
    public class Dropdown : MonoBehaviour
    {
        public delegate void Press(int index);

        public event Press OnPress;

        public int Index;

        public Button Button;
        public Text Text;

        public void OnClick()
        {
            if (OnPress != null)
                OnPress(Index);
        }
    }

}
