using UnityEngine;
using System.Collections;

public class ObjectHider : MonoBehaviour {

    public bool On = true;

    public float TransitionTime = 0.5f;

    private float tranStart = 0;
    private float alpha = 1;

    private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            Destroy(this);
        this.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {

        float t = Time.time - tranStart;
        float p = t / TransitionTime;

        if (p > 1)
        {
            p = 1;
            this.enabled = false;
        }

        Color old = spriteRenderer.color;

        if(On)
        {
            spriteRenderer.color = new Color(old.r, old.g, old.b, p);
        }
        else
        {
            spriteRenderer.color = new Color(old.r, old.g, old.b, 1 -p);
        }
	}

    public void Set(bool on)
    {
        if (On == on)
            return;

        On = on;
        this.enabled = true;
        tranStart = Time.time;
    }
}
