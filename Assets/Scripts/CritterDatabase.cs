using System;
using System.Collections.Generic;
using UnityEngine;

public class CritterDatabase : MonoBehaviour {

	public static CritterDatabase Main { get; private set; }

	[Serializable]
	public struct CritterInfo {
		[SerializeField] private string creatureName;
		[TextArea(3, 10)]
		[SerializeField] private string descriptionLive;
		[TextArea(3, 10)]
		[SerializeField] private string descriptionDead;

		public string CreatureName => creatureName;
		public string DescriptionLive => descriptionLive;
		public string DescriptionDead => descriptionDead;

		public static bool operator==(CritterInfo a, CritterInfo b) {
			return a.creatureName == b.creatureName
				&& a.descriptionLive == b.descriptionLive
				&& a.descriptionDead == b.descriptionDead;
		}

		public static bool operator!=(CritterInfo a, CritterInfo b) {
			return !(a == b);
		}

		public override bool Equals(object obj) {
			if (!(obj is CritterInfo)) {
				return false;
			}

			var info = (CritterInfo)obj;
			return this == info;
		}

		public override int GetHashCode() {
			var hashCode = 1430997281;
			hashCode = hashCode * -1521134295 + creatureName.GetHashCode();
			hashCode = hashCode * -1521134295 + descriptionLive.GetHashCode();
			hashCode = hashCode * -1521134295 + descriptionDead.GetHashCode();
			return hashCode;
		}
	}

	private class CritterData {
		public CritterInfo info;
		public int foundDead;
		public int foundAlive;

		public bool Found => foundDead > 0 || foundAlive > 0;

		public CritterData(CritterInfo info) {
			this.info = info;
			this.foundDead = 0;
			this.foundAlive = 0;
		}
	}

	private Dictionary<string, CritterData> infoMap = new Dictionary<string, CritterData>();

	void Awake() {
		if (!Main) Main = this;
	}

	public void Register(string id, CritterInfo info) {
		CritterData existing;
		if (!infoMap.TryGetValue(id, out existing)) {
			infoMap.Add(id, new CritterData(info));
		} else if (info != existing.info) {
			Debug.LogErrorFormat("Tried to register two different info blocks to the same ID ({0})", id);
		}
	}

	public bool WasCaptured(string id) {
		CritterData existing;
		if (!infoMap.TryGetValue(id, out existing)) {
			Debug.LogErrorFormat("Tried to check capture for an unregistered ID ({0})", id);
			return false;
		}

		return existing.Found;
	}

	public bool Captured(string id, bool alive) {
		CritterData existing;
		if (!infoMap.TryGetValue(id, out existing)) {
			Debug.LogErrorFormat("Tried to capture an unregistered ID ({0})", id);
			return false;
		}

		bool wasFound = existing.Found;

		if (alive) {
			existing.foundAlive++;
		} else {
			existing.foundDead++;
		}

		return !wasFound;
	}
}
