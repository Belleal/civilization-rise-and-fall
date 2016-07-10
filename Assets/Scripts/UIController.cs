using UnityEngine;
using System.Collections;

public class UIController : MonoBehaviour {
	/* UI Controller singleton */
	public static UIController instance { get; private set; }

	/* System function. */
	void Awake() {
		// make the UI controller a singleton:
		instance = this;
	}

	/* Used to exit the game application. */
	public void ExitGame() {
		Application.Quit();
	}
}
