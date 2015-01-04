using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("EffectsSystem/EffectDetacher")]
public class EffectDetacher : MonoBehaviour {

    public List<GameObject> ChildrenToDetach;

    void OnDisable()
    {
        for (int i = 0; i < ChildrenToDetach.Count; i++)
        {
            GameObject child = ChildrenToDetach[i];
            if (child.transform.IsChildOf(transform))
            {
                child.transform.parent = null;
            }

        }
    }

    void OnDestroy()
    {
        
    }
}
