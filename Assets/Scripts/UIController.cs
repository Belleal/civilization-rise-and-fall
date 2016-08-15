using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Civilization {

	public class UIController : MonoBehaviour {
		/* UI Controller singleton */
		public static UIController instance { get; private set; }


		public Canvas gameMenu;
		public Canvas settlementMenu;
		public CanvasRenderer centralPopulationArea;
		public GameObject populationUnitInfoPrefab;

		/* System function. */
		void Awake() {
			// make the UI controller a singleton:
			instance = this;
		}

		/* Used to exit the game application. */
		public void ExitGame() {
			Application.Quit();
		}

		/* Used to open and populate hte settlement panel. */
		public void OpenSettlementMenu( Settlement settlement ) {
			gameMenu.gameObject.SetActive( false );
			settlementMenu.gameObject.SetActive( true );

			foreach ( KeyValuePair<string,PopulationGroup> populationGroup in settlement.populationGroups ) {
				foreach ( KeyValuePair<string,Population> population in populationGroup.Value.population ) {
					GameObject infoPanel = Instantiate<GameObject>( populationUnitInfoPrefab );
					infoPanel.transform.SetParent( centralPopulationArea.transform, false );

					//GameObject infoPanel = centralPopulationArea.gameObject.AddComponent
				}
			}
		}

		/* Used to close settlement panel and return to game map. */
		public void CloseSettlementMenu() {
			settlementMenu.gameObject.SetActive( false );
			gameMenu.gameObject.SetActive( true );
		}


		public void TogglePopulationPanel( GameObject panel ) {
			panel.SetActive( !panel.activeSelf );
		}
	}

}
