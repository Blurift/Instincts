/* Name: Health System
 * Desc: Stores and manipulates vitals for a game object
 * Author: Keirron Stach
 * Version: 1.0
 * Created: 1/04/2014
 * Edited: 22/04/2014
 */ 

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Health System/Health Attributes")]
public class HealthSystem : MonoBehaviour {

	#region Fields

	[SerializeField]
	private int health = 0;
	[SerializeField]
	private int healthMax = 100;

	[SerializeField]
	private int hunger = 0;
	[SerializeField]
	private int hungerMax = 500;

	[SerializeField]
	private int stamina = 0;
	[SerializeField]
	private int staminaMax = 100;

	private float lastHungerSync;
	private float lastStaminaSync;

	public bool HungerEnabled = true;
	public bool StaminaEnabled = true;
	public bool HealthRegenerates = true;

	public GameObject[] HitEffects;
	public GameObject[] HitDropEffects;

    public GameObject DeathEffectPrefab;
    public GameObject DeathRemainsPrefab;

    private float useStaminaUntil = 0;
    private bool exhausted = false;

    private bool isDead = false;

	#endregion

	#region Properties

	public int Health
	{
		get { return health; }
		set
		{
			health = value;
			if(health > healthMax)
				health = healthMax;
			if(Network.isServer)
			{
				if(networkView != null)
					networkView.RPC("ChangeHealth", RPCMode.Others, health);
				if(health <= 0)
					Die();
			}
		}
	}


	public int Hunger
	{
		get { return hunger; }
		set
		{
			hunger = value;
			if(hunger > HungerMax)
				hunger = HungerMax;
			if(hunger < 0)
				hunger = 0;
		}
	}

	public int Stamina
	{
		get { return stamina; }
		set
		{
			stamina = value;			
			if(stamina > staminaMax)
				stamina = staminaMax;
			else if(stamina < 0)
				stamina = 0;
            if (stamina < 10)
                exhausted = true;
            else if (stamina > 25)
                exhausted = false;
		}
	}

	public int HealthMax
	{
		get { return healthMax; }
		set { healthMax = value; }
	}

	public int HungerMax
	{
		get { return hungerMax; }
		set { hungerMax = value; }
	}

	public int StaminaMax
	{
		get { return staminaMax; }
		set { staminaMax = value; }
	}

	#endregion

	#region Events

	public delegate void EventActionInt (int val);
	public delegate void EventAction ();
	//public delegate void HitAction (NetworkPlayer player, int damage);
	public delegate void HitAction (GameObject source, int damage);

	public event EventActionInt StaminaChange;
	public event EventAction Death;
	public event HitAction Hit;

	#endregion

	private List<HealthEffect> healthEffects = new List<HealthEffect>();

    void Awake()
    {
        Health = HealthMax;
        Hunger = HungerMax;
        Stamina = staminaMax;
    }

	void Start()
	{
		
	}

    public void Respawn()
    {
        Health = HealthMax;
        Hunger = HungerMax;
        Stamina = staminaMax;
        healthEffects = new List<HealthEffect>();
    }


	public void TakeDamage(DamageType damage, GameObject source)
	{

		int convertedDamage = damage.Damage + UnityEngine.Random.Range(0,damage.AltDamage);


		//If the even is not null raise the hit event with the converted Damage received
		if (Hit != null)
			Hit (source, convertedDamage);

		if(convertedDamage > 0)
		{
			Health -= convertedDamage;
			lastDamage = Time.time;
		}

		if(damage.Effect == DamageEffect.Bleeding)
			BleedingRPC(true);
		
		if(HitEffects != null)
		{
			if(HitEffects.Length > 0)
			{
				int effect = UnityEngine.Random.Range(0,HitEffects.Length);
				EffectManager.CreateNetworkEffect(transform.position, HitEffects[effect].name);
			}
		}
		
		if(HitDropEffects != null)
		{
			if(HitDropEffects.Length > 0)
			{
				int effect = UnityEngine.Random.Range(0,HitDropEffects.Length);
				EffectManager.CreateNetworkEffect(transform.position, HitDropEffects[effect].name);
			}
		}
	}

	public float RegenDelay = 1;
	private float lastDamage = 0;
	private float lastHealthRegen = 0;

	void Update()
	{
		if(Network.isServer)
		{
			if(Health < HealthMax && !ContainsHealthEffect(typeof(HealthBleeding)) && Hunger > HungerMax/2 && HealthRegenerates)
			{
				if(Time.time - lastDamage > RegenDelay && Time.time - lastHealthRegen > 0.5f)
				{
					Health += 1;
					lastHealthRegen = Time.time;
				}
			}

			if(Time.time - lastHungerSync > 2.5f && HungerEnabled)
			{
                ChangeHunger(Hunger - 1);

                if (Hunger <= 0)
                    Health--;

				lastHungerSync = Time.time;
			}

			if(Time.time - lastStaminaSync > 1 && StaminaEnabled)
			{

                if (useStaminaUntil > Time.time)
                    ChangeStamina(Stamina -8);
                else
                    ChangeStamina(Stamina+1);

				lastStaminaSync = Time.time;
			}
		}

		for(int i = 0; i < healthEffects.Count; i++)
		{
			healthEffects[i].Update(this);
		}
	}

	[RPC]
	void ChangeHealth(int health)
	{
		Health = health;
	}

	public void ChangeHunger(int hunger)
	{
        if (Network.isServer)
        {
            networkView.RPC("ChangeHungerRPC", RPCMode.Others, hunger);
            ChangeHungerRPC(hunger);
        }
        else
            networkView.RPC("ChangeHungerRPC", RPCMode.Server, hunger);
	}

    [RPC]
    void ChangeHungerRPC(int hunger)
    {
        Hunger = hunger;
    }

	public void ChangeStamina(int stamina)
	{
        if (Network.isClient)
            networkView.RPC("ChangeStaminaRPC", RPCMode.Server, stamina);
        else
        {
            networkView.RPC("ChangeStaminaRPC", RPCMode.Others, stamina);
            ChangeStaminaRPC(stamina);
        }
	}

    [RPC]
    void ChangeStaminaRPC(int stamina)
    {
        Stamina = stamina;
    }

    [RPC]
    public void UseStamina()
    {
        if (Network.isClient)
            networkView.RPC("UseStamina", RPCMode.Server);
        useStaminaUntil = Time.time + 1f;
    }

    public void ChangeBleeding(bool bleed)
    {
        if (Network.isServer)
            BleedingRPC(bleed);
        else
            networkView.RPC("BleedingRPC", RPCMode.Server, bleed);
    }

    public bool Exhausted()
    {
        return exhausted;
    }

	[RPC]
	public void BleedingRPC(bool bleed)
	{
		if(Network.isServer && networkView != null)
			networkView.RPC("BleedingRPC", RPCMode.Others, bleed);
		if(bleed)
			AddHealthEffect (new HealthBleeding ());
		else
			RemoveHealthEffect(typeof(HealthBleeding));
	}

	[RPC]
	void Die()
	{
		if(Network.isServer)
		{
            if (!isDead)
            {
                if (DeathEffectPrefab != null)
                {
                    EffectManager.CreateNetworkEffect(transform.position, DeathEffectPrefab.name);
                }

                if (DeathRemainsPrefab != null)
                {
                    EffectManager.CreateNetworkEffect(transform.position, DeathRemainsPrefab.name, transform.rotation);
                }
            }

			if(Death == null)
			{
				NetworkManager.RemoveNetworkBuffer(networkView.viewID);
				Network.Destroy(gameObject);
			}
			else
			{
				Death();
			}

            isDead = true;
		}
	}

	public void RemoveHealthEffect(Type t)
	{
		for (int i = 0; i < healthEffects.Count; i++)
		{
			if (healthEffects [i].GetType () == t)
			{
				healthEffects.RemoveAt(i);
			};
		}
	}

	public void AddHealthEffect(HealthEffect effect)
	{
		if(!ContainsHealthEffect(effect.GetType()))
		{
			healthEffects.Add(effect);
		}
	}

	public bool ContainsHealthEffect(Type t)
	{
		for(int i = 0; i < healthEffects.Count; i++)
			if(healthEffects[i].GetType() == t)
				return true;

		return false;
	}

	public void SetHealthState(HealthState state)
	{
		Health = state.Health;
		Hunger = state.Hunger;
	}

	public HealthState GetHealthState()
	{
		HealthState state = new HealthState ();
		state.Health = Health;
		state.Hunger = Hunger;
		return state;
	}

	[System.Serializable]
	public class HealthState
	{
		public int Health;
		public int Hunger;
	}
}

public enum DamageEffect
{
	None,
	Bleeding,
	Acid,
	Crippled,
}

[System.Serializable]
public class DamageType
{
	public DamageEffect Effect = DamageEffect.None;
	public int Damage = 1;
	public int AltDamage = 0;

	public DamageType(DamageEffect effect, int damage, int altDamage)
	{
		Effect = effect;
		Damage = damage;
		AltDamage = altDamage;
	}

	public DamageType()
	{
	}
}

public class HealthEffect
{
	public virtual void Update(HealthSystem sys){}
}

public class HealthBleeding : HealthEffect
{
	private float lastBleed = 0;

	public override void Update (HealthSystem sys)
	{
		if(Time.time - lastBleed > 1.5f)
		{
			EffectManager.CreateEffect(sys.transform.position, "Blood Drop 1");
			lastBleed = Time.time;
		}
	}
}
