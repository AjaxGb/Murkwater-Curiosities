using UnityEngine;

public class CritterBase : MonoBehaviour {

	[SerializeField] private CritterDatabase critterDB;
	[SerializeField] private string critterID;
	[SerializeField] private CritterDatabase.CritterInfo info;

	void Start() {
		if (!critterDB) critterDB = CritterDatabase.Main;

		critterDB.Register(critterID, info);
	}
}
