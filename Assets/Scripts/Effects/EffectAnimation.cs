using UnityEngine;
using System.Collections;

[AddComponentMenu ("EffectsSystem/Animation")]
public class EffectAnimation : MonoBehaviour {

	SpriteRenderer SpriteRenderer;

	void Start ()
	{
		SpriteRenderer = GetComponent<SpriteRenderer> ();
		animation.Play ();
	}

	// Update is called once per frame
	void Update () {
		if (!animation.isPlaying)
			Destroy (gameObject);
	}

	public void SetNextSprite(Sprite sprite)
	{
		SpriteRenderer.sprite = sprite;
	}
}
