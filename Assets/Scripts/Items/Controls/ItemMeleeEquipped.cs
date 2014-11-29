using UnityEngine;
using System;
using System.Collections;

public class ItemMeleeEquipped : MonoBehaviour {

	public LayerMask Layers;
	public DamageType Damage;
	public string UseEffect;
	public string HitEffect;
    public string MissEffect;

	public GameObject EffectUse;

	public Animator AnimatorControl;

	public GameObject Owner;

	[RPC]
	public void Use(Vector3 aim)
	{
		if(Network.isServer)
		{
            Vector3 origin = transform.position;
			Vector3 point = new Vector3(aim.x,aim.y,0) - origin;

			GameObject player = NetworkManager.GetPlayer(networkView.owner);
			origin = player.transform.position;
			float charRadius = player.GetComponent<CircleCollider2D>().radius + 0.1f;

			Vector2 direction = new Vector2 (point.x, point.y).normalized;
			origin += new Vector3(direction.x,direction.y,0) * charRadius;

			RaycastHit2D hit = Physics2D.Raycast (origin, direction, 1, Layers);
			//GameManager.WriteMessage("Player: " + origin.ToString() + " : " + direction.ToString());
			if(hit != false)
			{
				if(HitEffect != null && HitEffect != "")
				{
					EffectManager.CreateNetworkEffect(transform.position, HitEffect);
				}

				HealthSystem health = hit.collider.GetComponent<HealthSystem>();

				if(Owner == null)
					Owner = transform.parent.gameObject;
                if(health != null)
                    health.TakeDamage(Damage, Owner);
			}
            else
            {
                if (MissEffect != null && MissEffect != "")
                {
                    EffectManager.CreateNetworkEffect(transform.position, MissEffect);
                }
            }
			
		}
        //else
        //{
        //    networkView.RPC("Use", RPCMode.Server, origin, aim);
        //}
	}

	[RPC]
	public void UseEffects()
	{
		if(EffectUse != null)
			Instantiate(EffectUse, transform.position, transform.rotation);

		if(AnimatorControl != null)
			AnimatorControl.SetTrigger("Used");
	}

	public void SetDamage(string t, int d, int ad)
	{
		DamageEffect effect = DamageEffect.None;
		try
		{
			effect = (DamageEffect)Enum.Parse(typeof(DamageEffect), t);
		} catch (Exception ex) {
			
		}
		Damage = new DamageType (effect, d, ad);
	}
}
