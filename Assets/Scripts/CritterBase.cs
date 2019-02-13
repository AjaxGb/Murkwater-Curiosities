using UnityEngine;

[SelectionBase]
public class CritterBase : MonoBehaviour {

	[SerializeField] private CritterDatabase critterDB;
	[SerializeField] private string critterID;
	[SerializeField] private CritterDatabase.CritterInfo info;

	[SerializeField] protected ParticleSystem deathExplosion;
	[SerializeField] protected float health;

	public int deadMoney = 50;
	public int liveMoney = 200;

	public float damage = 5;

	public bool IsAlive => health > 0;
	public bool IsStunned { get; private set; }

	protected Rigidbody2D rb;

	protected virtual void Start() {
		if (!critterDB) critterDB = CritterDatabase.Main;

		critterDB.Register(critterID, info);

		if (!rb) rb = GetComponent<Rigidbody2D>();
	}

	protected virtual void Update() {
		if (!IsAlive || IsStunned) {
			ParticleSystem death = Instantiate(deathExplosion, transform.position, Quaternion.identity);
			Destroy(death.gameObject, death.main.duration);
			
			if (CritterDatabase.Main.Captured(critterID, IsAlive)) {
				PaperManager.Main.Open(info.CreatureName, IsAlive ? info.DescriptionLive : info.DescriptionDead);
			}

			SubController.Main.CurrMoney += (IsAlive ? liveMoney : deadMoney);

			Destroy(this.gameObject);
		}
	}

	protected virtual void OnTriggerEnter2D(Collider2D collider) {
		SubController player = collider.GetComponentInParent<SubController>();
		if (player) {
			player.CurrHealth -= damage;
		}
	}

	public virtual void Hurt(float damage) {
		health -= damage;
	}

	public virtual void Stun() {
		IsStunned = true;
	}
}
