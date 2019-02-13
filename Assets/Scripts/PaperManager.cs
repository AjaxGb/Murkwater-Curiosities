using TMPro;
using UnityEngine;

public class PaperManager : MonoBehaviour {

	public static PaperManager Main { get; private set; }

	public RectTransform lightbox;

	public TextMeshProUGUI startText;
	public TextMeshProUGUI titleText;
	public TextMeshProUGUI bodyText;

	private bool open = true;
	
	void Start() {
		if (!Main) Main = this;
		OpenStartText();
	}
	
	void Update() {
		if (open && Input.GetMouseButtonDown(0)) {
			Close();
		}
	}

	private void _Open() {
		lightbox.gameObject.SetActive(true);
		open = true;
		Time.timeScale = 0;
	}

	public void OpenStartText() {
		Close();
		startText.gameObject.SetActive(true);

		_Open();
	}

	public void Open(string title, string body) {
		Close();

		titleText.text = title;
		bodyText.text = body;

		titleText.gameObject.SetActive(true);
		bodyText.gameObject.SetActive(true);

		_Open();
	}

	public void Close() {
		lightbox.gameObject.SetActive(false);
		startText.gameObject.SetActive(false);
		titleText.gameObject.SetActive(false);
		bodyText.gameObject.SetActive(false);
		open = false;
		Time.timeScale = 1;
	}
}
