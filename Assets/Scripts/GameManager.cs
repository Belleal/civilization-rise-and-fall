using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Civilization {
	
	#region Enumerations

	public enum PopulationCenterSize {
		tiny = 1,
		small = 2,
		medium = 3,
		large = 4,
		huge = 5,
		enourmous = 6
	}

	public enum BiomeType {
		iceSheet = 1,
		tundra = 2,
		taiga = 3,
		temperateForest = 4,
		temperateSteppe = 5,
		moistForest = 6,
		mediterraneanVegetation = 7,
		monsoonForest = 8,
		aridDesert = 9,
		xericShrubland = 10,
		drySteppe = 11,
		semiaridDesert = 12,
		grassSavanna = 13,
		treeSavanna = 14,
		dryForest = 15,
		rainForest = 16,
		alpineTundra = 17,
		mountainForest = 18,
		freshWater = 19,
		salineWater = 20
	}

	public enum BiomePotentialType {
		l = 0,
		f = 1,
		c = 2
	}

	public enum ElevationType {
		lowland = 1,
		upland = 2,
		highland = 3,
		alpine = 4
	}

	public enum Assignment {
		unassigned = 0,
		worker = 1
	}

	public enum PopulationGroupType {
		district = 1,
		village = 2
	}

	#endregion

	#region Game Entity Objects

	public class Location : IEquatable<Location> {
		public int row { get; private set; }
		public int col { get; private set; }

		private Location() {
		}

		public Location( int row, int col ) {
			this.row = row;
			this.col = col;
		}

		public override bool Equals( object obj ) { 
			return Equals( obj as Location );
		}

		public bool Equals( Location other ) { 
			return this.row == other.row && this.col == other.col;
		}

		public override int GetHashCode() {
			return this.row.GetHashCode() + this.col.GetHashCode();
		}
	};

	public struct Storrage {
		public int current;
		public int maxCapacity;
	};

	public class PopulationCenterArea {
		public List<Vector3> coordinates { get; set; }
		public List<Location> tiles { get; set; }
		public List<GameObject> visuals { get; set; }
	};

	#region Modifiers

	public abstract class Modifier {
		public float value { get; set; }
	};

	public class StaticModifier : Modifier {
		public bool isPermanent { get; set; }
	};

	public class TimedModifier : Modifier {
		public int turnsRemaining { get; set; }
	};

	public class DiminishingModifier : Modifier {
		public float diminishStep { get; set; }
	};

	#endregion

	public class Population {
		public string uid { get; private set; }
		public string name { get; set; }
		public float happiness { get; set; }
		public int bornOn { get; private set; }
		public int units { get; set; }
		public float socialClass { get; set; }
		public float birthFactor { get; set; }
		public float deathFactor { get; set; }
		public Assignment assignment { get; set; }
		public Location workplace { get; set; }
		public List<Modifier> happinessModifiers { get; set; }

		private Population() {
			uid = GameManager.instance.GenerateUID();
			happinessModifiers = new List<Modifier>();
			bornOn = 0;
		}

		public Population( int currentTurn ) : this() {
			bornOn = currentTurn;
		}
	};

	public class PopulationGroup {
		public string uid { get; private set; }
		public string name { get; set; }
		public int establishedOn { get; private set; }
		public PopulationGroupType type { get; set; }
		public Location location { get; set; }
		public PopulationCenterArea borderArea { get; set; }
		public Dictionary<string,Population> population { get; set; }
		public Dictionary<Location,GameObject> visuals { get; set; }

		private PopulationGroup() {
			uid = GameManager.instance.GenerateUID();
			borderArea = new PopulationCenterArea();
			population = new Dictionary<string,Population>();
			visuals = new Dictionary<Location, GameObject>();
			establishedOn = 0;
		}

		public PopulationGroup( int currentTurn ) : this() {
			establishedOn = currentTurn;
		}
	};

	public class PopulationCenter {
		public string uid { get; private set; }
		public string name { get; set; }
		public PopulationCenterSize size { get; set; }
		public int populationMargin { get; set; }
		public Location centerLocation { get; set; }
		public PopulationCenterArea borders { get; set; }
		public PopulationCenterArea outskirts { get; set; }
		public Dictionary<string,PopulationGroup> populationGroups { get; set; }
		public Dictionary<string,Population> homeless { get; set; }
		public Storrage tools { get; set; }
		public Storrage food { get; set; }
		public Storrage goods { get; set; }
		public Storrage materials { get; set; }

		public PopulationCenter() {
			uid = GameManager.instance.GenerateUID();
			borders = new PopulationCenterArea();
			outskirts = new PopulationCenterArea();
			populationGroups = new Dictionary<string,PopulationGroup>();
			homeless = new Dictionary<string,Population>();
			tools = new Storrage();
			food = new Storrage();
			goods = new Storrage();
			materials = new Storrage();
		}
	};

	public class GameData {
		public GameTile[,] worldTiles { get; set; }
		public int turn { get; set; }
		public Dictionary<string,PopulationCenter> populationCenters { get; set; }

		public GameData() {
			populationCenters = new Dictionary<string,PopulationCenter>();
		}
	};

	#endregion

	public class GameManager : MonoBehaviour {
		/* Game Manager singleton */
		public static GameManager instance { get; private set; }

		/* References to outside objects */
		public GameObject sunController;
		public GameObject gameTilesContainer;
		public GameObject propPlaceholder;
		public GameTile tilePrefab;
		public Material[] tileBiomes = new Material[20];
		public GameObject tileBorders;
		public Canvas populationCenterShield;

		/* Would and tile measurments */
		public int worldHeightInTiles { get; private set; }
		public int worldWidthInTiles { get; private set; }
		public int tileSide { get; private set; }
		public float worldRadius { get; private set; }
		public float worldHeight { get; private set; }
		public float angleBetweeenTiles { get; private set; }
		public float tileSideHalf { get; private set; }
		public float tileHeight { get; private set; }
		public float tileBisector { get; private set; }
		public float tileDelta { get; private set; }
		public float tileRotationAngle { get; private set; }
		public float tileHalfBisector { get; private set; }
		public float propAnchorOffsetX { get; private set; }
		public float propAnchorDeltaZ { get; private set; }
		public float propAnchorOffsetZ { get; private set; }

		private float sunDistance = 250f;

		/* Game parameters */
		private GameData currentGame;
		private Dictionary<BiomeType,int[,]> biomePotentialMapping = new Dictionary<BiomeType,int[,]>();
		private int maxPopulationPerTile = 3;

		void Awake() {
			// make the game manager a singleton:
			instance = this;

			/// execute initialization procedures:
			InitializeGameProperties();

			// TODO: replace this with new game creation
			CreateNewGame( 64, 1024 );

			// adjust the Sun poition and direction based on the new world size:
			sunController.transform.Translate( new Vector3( 0, GameManager.instance.worldHeight / 2, GameManager.instance.worldRadius + GameManager.instance.tileHeight + sunDistance ) );
		}

		void LateUpdate() {
			// TODO: later we will change the sun rotation to be based on the game turns or even more complex factors
			sunController.transform.RotateAround( gameTilesContainer.transform.position, Vector3.up, 10 * Time.deltaTime );
		}

		/* Used to initialize the world dimensions and angles. */
		private void InitializeWorldDimensions() {
			int halfWidthInTiles = worldWidthInTiles / 2;
			angleBetweeenTiles = ( 180f * ( halfWidthInTiles - 2 ) ) / halfWidthInTiles;
			worldRadius = tileSide / ( 2 * Mathf.Tan( ( 180f * Mathf.Deg2Rad ) / halfWidthInTiles ) );

			Debug.Log( "World Radius: " + worldRadius );
			Debug.Log( "Angle Between Tiles: " + angleBetweeenTiles );

			// precalcualte the tile dimensions to reuse them later (especially square root operation is expensive one):
			tileSideHalf = tileSide / 2;
			tileHeight = ( Mathf.Sqrt( 6 ) * tileSide ) / 3;
			tileBisector = ( Mathf.Sqrt( 3 ) * tileSide ) / 2;
			tileDelta = ( Mathf.Sqrt( 3 ) * tileSide ) / 6;
			tileRotationAngle = ( 180f - angleBetweeenTiles ) / 2;
			tileHalfBisector = tileBisector / 2;

			// precalculate the prop anchor offsets:
			propAnchorOffsetX = tileSide / 10;
			propAnchorDeltaZ = Mathf.Tan( 30f * Mathf.Deg2Rad ) * propAnchorOffsetX;
			propAnchorOffsetZ = ( Mathf.Tan( 30f * Mathf.Deg2Rad ) * ( propAnchorOffsetX * 2.5f ) ) - propAnchorDeltaZ;

			// calculate the world actual height:
			worldHeight = worldHeightInTiles * tileBisector;

			Debug.Log( "World Height: " + worldHeight );
			Debug.Log( "Tile Rotation Angle: " + tileRotationAngle );
		}

		/* Used to initialize the common game properties. These are independent from actual game data. */
		private void InitializeGameProperties() {
			// the tile side in units will determine a lot of other measurments and distances; increase as necessary for more detailed scenery:
			tileSide = 100;

			/**
	    	 * Game Rule: Biome Potential Base Values
	    	 * The mapping follows the pattern of 1-2-3 workers and then for each worker L-F-C as per the enum 'BiomePotentialType'
			 */
			biomePotentialMapping.Add( BiomeType.iceSheet, new int[,] { { 1, 0, 0 }, { 1, 1, 0 }, { 1, 0, 1 } } );
			biomePotentialMapping.Add( BiomeType.tundra, new int[,] { { 1, 1, 0 }, { 2, 1, 0 }, { 1, 1, 1 } } );
			biomePotentialMapping.Add( BiomeType.taiga, new int[,] { { 2, 1, 0 }, { 1, 2, 1 }, { 1, 0, 2 } } );
			biomePotentialMapping.Add( BiomeType.temperateForest, new int[,] { { 2, 1, 0 }, { 1, 2, 1 }, { 1, 1, 1 } } );
			biomePotentialMapping.Add( BiomeType.temperateSteppe, new int[,] { { 1, 2, 0 }, { 1, 2, 1 }, { 1, 2, 0 } } );
			biomePotentialMapping.Add( BiomeType.moistForest, new int[,] { { 1, 1, 0 }, { 1, 3, 0 }, { 2, 1, 0 } } );
			biomePotentialMapping.Add( BiomeType.mediterraneanVegetation, new int[,] { { 1, 1, 1 }, { 1, 2, 1 }, { 2, 1, 0 } } );
			biomePotentialMapping.Add( BiomeType.monsoonForest, new int[,] { { 1, 1, 0 }, { 1, 2, 1 }, { 2, 1, 0 } } );
			biomePotentialMapping.Add( BiomeType.aridDesert, new int[,] { { 0, 0, 1 }, { 1, 0, 0 }, { 1, 0, 1 } } );
			biomePotentialMapping.Add( BiomeType.xericShrubland, new int[,] { { 1, 0, 1 }, { 1, 1, 0 }, { 1, 0, 1 } } );
			biomePotentialMapping.Add( BiomeType.drySteppe, new int[,] { { 2, 1, 0 }, { 2, 1, 1 }, { 1, 1, 1 } } );
			biomePotentialMapping.Add( BiomeType.semiaridDesert, new int[,] { { 0, 0, 1 }, { 1, 0, 1 }, { 0, 1, 1 } } );
			biomePotentialMapping.Add( BiomeType.grassSavanna, new int[,] { { 0, 3, 0 }, { 0, 3, 1 }, { 0, 2, 1 } } );
			biomePotentialMapping.Add( BiomeType.treeSavanna, new int[,] { { 1, 2, 0 }, { 0, 3, 1 }, { 1, 1, 1 } } );
			biomePotentialMapping.Add( BiomeType.dryForest, new int[,] { { 2, 0, 1 }, { 2, 1, 1 }, { 1, 1, 1 } } );
			biomePotentialMapping.Add( BiomeType.rainForest, new int[,] { { 0, 2, 0 }, { 2, 1, 1 }, { 1, 2, 0 } } );
			biomePotentialMapping.Add( BiomeType.alpineTundra, new int[,] { { 1, 0, 0 }, { 2, 0, 0 }, { 1, 1, 0 } } );
			biomePotentialMapping.Add( BiomeType.mountainForest, new int[,] { { 2, 0, 0 }, { 2, 1, 0 }, { 1, 0, 1 } } );
			biomePotentialMapping.Add( BiomeType.freshWater, new int[,] { { 0, 1, 1 }, { 0, 2, 1 }, { 0, 2, 0 } } );
			biomePotentialMapping.Add( BiomeType.salineWater, new int[,] { { 0, 2, 1 }, { 1, 1, 1 }, { 0, 1, 2 } } );

		}

		/* Used to generate all the game map tiles and fill in their properties. */
		private GameTile[,] GenerateWorld() {
			GameTile[,] worldTiles = new GameTile[worldHeightInTiles, worldWidthInTiles];

			bool isEvenRow = false;
			for ( int rowIdx = 0 ; rowIdx < worldHeightInTiles ; rowIdx++ ) {
				bool isUpwardTile = isEvenRow;
				for ( int colIdx = 0 ; colIdx < worldWidthInTiles ; colIdx++ ) {
					// instantiate and prepare the tile:
					worldTiles[ rowIdx, colIdx ] = Instantiate<GameTile>( tilePrefab );
					worldTiles[ rowIdx, colIdx ].Initialize( new Location( rowIdx, colIdx ), isUpwardTile );

					// attach and align the tile to the game tiles container:
					worldTiles[ rowIdx, colIdx ].transform.parent = gameTilesContainer.transform;
					worldTiles[ rowIdx, colIdx ].transform.rotation = this.transform.rotation;
					worldTiles[ rowIdx, colIdx ].transform.localPosition = Vector3.zero;

					// reposition and rotate the tile in relation to the world center axis:
					worldTiles[ rowIdx, colIdx ].transform.Rotate( Vector3.right, 90, Space.Self );
					worldTiles[ rowIdx, colIdx ].transform.Translate( new Vector3( 0, worldRadius, -( rowIdx * tileBisector - ( isUpwardTile ? tileDelta : 0 ) + 2 * tileDelta ) ) );
					worldTiles[ rowIdx, colIdx ].transform.RotateAround( gameTilesContainer.transform.position, Vector3.up, colIdx * tileRotationAngle );

					// TODO: add the proper biome material:
					Renderer tileRenderer = worldTiles[ rowIdx, colIdx ].gameObject.GetComponent<Renderer>();
					tileRenderer.material = tileBiomes[ 12 ];

					isUpwardTile = !isUpwardTile;
				}

				isEvenRow = !isEvenRow;
			}

			return worldTiles;
		}

		/* Used to initialize new game. */
		public void CreateNewGame( int height, int width ) {
			// world size must be even numbers and even better if are power of 2
			worldHeightInTiles = ( height % 2 == 0 ) ? height : height - 1;
			worldWidthInTiles = ( width % 2 == 0 ) ? width : width - 1;

			// once we have the world size calculate the rest of the dimensions:
			InitializeWorldDimensions();

			// initialize new game data and its properties:
			currentGame = new GameData();
			currentGame.worldTiles = GenerateWorld();
			currentGame.turn = 1;

			// draw the static objects:
			DrawWorldTiles();

			#region TEMPORARY CODE: This is used to initialize and test various game objects

			PopulationCenter newPopulationCenter = CreatePopulationCenter( "Babylon", new Location( 24, 1 ) );
			//newPopulationCenter.size = PopulationCenterSize.medium;



			DeterminePopulationCenterAreas( ref newPopulationCenter );
			currentGame.populationCenters.Add( newPopulationCenter.uid, newPopulationCenter );

			#endregion

			// draw game objects:
			foreach ( KeyValuePair<string,PopulationCenter> populationCenter in currentGame.populationCenters ) {
				DrawPopulationCenter( populationCenter.Value );
			}
		}

		/* Used to load previously saved game by its unique ID. */
		public void LoadGame( string gameUID ) {
		
		}

		/**
		 * Draw Visuals functions
		 */

		/* Used to (re)draw all the world tiles. It expects that all the tiles are already initialized. */
		private void DrawWorldTiles() {
			foreach ( GameTile worldTile in currentGame.worldTiles ) {
				worldTile.CreateTileMesh( tileSide, tileSideHalf, tileHeight, tileDelta, tileBisector );
				worldTile.CalculatePropAnchors( propAnchorOffsetX, propAnchorOffsetZ, propAnchorDeltaZ );
			}
		}

		/* Used to (re)draw a population center visuals. */
		private void DrawPopulationCenter( PopulationCenter populationCenter ) {
			GameTile centerTile = GetTileByLocation( populationCenter.centerLocation );

			// draw the shield:
			Canvas shield = Instantiate<Canvas>( populationCenterShield );
			shield.transform.SetParent( centerTile.transform, false );
			shield.transform.rotation = centerTile.transform.rotation;
			shield.transform.localPosition = Vector3.zero;
			shield.transform.Rotate( new Vector3( 90, 180, 0 ) );
			shield.transform.Translate( new Vector3( 0, -tileDelta, -120 ), Space.Self );

			EventTrigger shieldTrigger = shield.GetComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerClick;
			entry.callback.AddListener( ( eventData ) => {
				CameraController.instance.FocusAtTile( centerTile );
			} );
			shieldTrigger.triggers.Add( entry );

			// draw the population center borders:
			DrawTileBorders( centerTile, populationCenter.borders );
			if ( (int)populationCenter.size >= (int)PopulationCenterSize.medium ) {
				//DrawTileBorders( centerTile, populationCenter.outskirts );
			}

			// draw the housing:
			foreach ( KeyValuePair<string,PopulationGroup> populationGroup in populationCenter.populationGroups ) {
				// TODO: draw districts and villages
				GameTile groupTile = GetTileByLocation( populationGroup.Value.location );
				List<Location> freeLocations = groupTile.GetAvailablePropLocations();

				// determine the number of props based on the population in each group:
				int propNumber = populationGroup.Value.population.Count * 3;
				for ( int idx = populationGroup.Value.visuals.Count ; idx < propNumber ; idx++ ) {
					int locationIdx = UnityEngine.Random.Range( 0, freeLocations.Count - 1 );
					Location propLocation = freeLocations[ locationIdx ];
					freeLocations.RemoveAt( locationIdx );

					// TODO: this should actually pick a random housing prop instead
					groupTile.DrawPropAtAnchor( propLocation, propPlaceholder );
					populationGroup.Value.visuals.Add( propLocation, propPlaceholder );
				}
			}
		}

		/* Used to (re)draw a set of tile borders. If border visuals already exist they will be destroyed first. */
		private void DrawTileBorders( GameTile centerTile, PopulationCenterArea borders ) {
			// clean up old visuals (if any):
			if ( borders.visuals != null ) {
				for ( int idx = 0 ; idx < borders.visuals.Count ; idx++ ) {
					Destroy( borders.visuals[ idx ] );
				}
			}

			// create the new visuals:
			borders.visuals = new List<GameObject>();
			for ( int idx = 0 ; idx < borders.coordinates.Count ; idx++ ) {
				GameObject visual = Instantiate<GameObject>( tileBorders );
				visual.transform.parent = centerTile.transform;
				visual.transform.localPosition = Vector3.zero;
				visual.transform.position = Vector3.Lerp( borders.coordinates[ idx ], borders.coordinates[ ( idx == borders.coordinates.Count - 1 ) ? 0 : idx + 1 ], 0.5f );
				visual.transform.rotation = Quaternion.LookRotation( borders.coordinates[ idx ] - borders.coordinates[ ( idx == borders.coordinates.Count - 1 ) ? 0 : idx + 1 ], Vector3.forward );
				visual.transform.Rotate( Vector3.up, 90, Space.Self );

				borders.visuals.Add( visual );
			}
		}

		/**
		 * Game Logic functions
		 */

		/* Used to create and initialize a new population center. */
		private PopulationCenter CreatePopulationCenter( string name, Location center ) {
			PopulationCenter populationCenter = new PopulationCenter();

			populationCenter.name = name;
			populationCenter.centerLocation = center;
			populationCenter.size = PopulationCenterSize.tiny;

			PopulationGroup centralDistrict = new PopulationGroup( currentGame.turn );
			centralDistrict.name = name;
			centralDistrict.location = center;
			centralDistrict.type = PopulationGroupType.district;

			Population population = new Population( currentGame.turn );
			centralDistrict.population.Add( population.uid, population );
			populationCenter.populationGroups.Add( centralDistrict.uid, centralDistrict );

			return populationCenter;
		}

		/* Used to determine and update (if allowed) the size of the population center. */
		private void DeterminePopulationCenterSize( ref PopulationCenter populationCenter ) {
			PopulationCenterSize newSize = populationCenter.size;
			int totalPopulatedTiles = 0;
			int twoUnitPopulatedTiles = 0;
			int threeUnitPopulatedTiles = 0;

			/**
			 * Game Rule: Population Center Sizes
			 * Only districts are considered when determining the size of a population center
			 */
			foreach ( KeyValuePair<string,PopulationGroup> populationGroup in populationCenter.populationGroups ) {
				// count the different population density in all relevant tiles:
				if ( populationGroup.Value.type == PopulationGroupType.district ) {
					if ( populationGroup.Value.population.Count > 0 ) {
						totalPopulatedTiles++;
					}
					if ( populationGroup.Value.population.Count > 1 ) {
						twoUnitPopulatedTiles++;
					}
					if ( populationGroup.Value.population.Count > 2 ) {
						threeUnitPopulatedTiles++;
					}
				}
			}

			/**
			 * Game Rule: Population Center Sizes
			 * The size of population centers is determined by the number of the various population density in all its tiles
			 * Only when all conditions are met the population size can increase
			 */
			// TODO: Add all remaining sizes here
			if ( totalPopulatedTiles >= 13 && twoUnitPopulatedTiles >= 5 && threeUnitPopulatedTiles >= 1 ) {
				newSize = PopulationCenterSize.large;
			} else if ( totalPopulatedTiles >= 8 && twoUnitPopulatedTiles >= 3 ) {
				newSize = PopulationCenterSize.medium;
			} else if ( totalPopulatedTiles >= 3 && twoUnitPopulatedTiles >= 1 ) {
				newSize = PopulationCenterSize.small;
			}

			/**
			 * Game Rule: Population Center Sizes
			 * The size of population centers cannot decrease even if the population dwindles
			 */
			if ( (int)newSize > (int)populationCenter.size ) {
				populationCenter.size = newSize;
			}
		}

		/* Used to trace the borders of a tile matrix based on the center location. */
		private List<Vector3> TraceBorders( List<int[]> tileMatrix, Location centerLocation ) {
			List<Vector3> top = new List<Vector3>();
			List<Vector3> left = new List<Vector3>();
			List<Vector3> right = new List<Vector3>();
			List<Vector3> bottom = new List<Vector3>();

			for ( int tileRow = 0 ; tileRow < tileMatrix.Count ; tileRow++ ) {
				int divider = Mathf.FloorToInt( tileMatrix[ tileRow ][ 0 ] / 2 );
				for ( int tileCol = -divider ; tileCol <= divider ; tileCol++ ) {					
					GameTile tile = GetTileByLocation( new Location( centerLocation.row + tileMatrix[ tileRow ][ 1 ], EnsureColWithinBoundary( centerLocation.col + tileCol ) ) );

					if ( tileRow == 0 ) {
						// in the top row pick the first and last tiles most outward points on the top line; from middle tiles only pick the upward tiles top points:
						if ( tileCol == -divider ) {
							if ( tileMatrix.Count == 2 && tile.isUpwardTile ) {
								top.Add( tile.transform.TransformPoint( tile.tileSurface[ 0 ] ) );
							}
							top.Add( tile.transform.TransformPoint( tile.tileSurface[ ( tile.isUpwardTile ) ? 1 : 0 ] ) );
						} else if ( tileCol == divider ) {
							top.Add( tile.transform.TransformPoint( tile.tileSurface[ ( tile.isUpwardTile ) ? 1 : 2 ] ) );
							if ( tileMatrix.Count == 2 && !tile.isUpwardTile ) {
								top.Add( tile.transform.TransformPoint( tile.tileSurface[ 1 ] ) );
							}
						} else {
							if ( tile.isUpwardTile ) {
								top.Add( tile.transform.TransformPoint( tile.tileSurface[ 1 ] ) );
							}
						}
					} else if ( tileRow == tileMatrix.Count - 1 ) {
						// in the bottom row pick the first and last tiles most outward points on the bottom line; from middle tiles only pick the downward tiles bottom points:
						if ( tileCol == -divider ) {
							if ( tileMatrix.Count == 2 && !tile.isUpwardTile ) {
								bottom.Add( tile.transform.TransformPoint( tile.tileSurface[ 0 ] ) );
							}
							bottom.Add( tile.transform.TransformPoint( tile.tileSurface[ ( tile.isUpwardTile ) ? 0 : 1 ] ) );
						} else if ( tileCol == divider ) {
							bottom.Add( tile.transform.TransformPoint( tile.tileSurface[ ( tile.isUpwardTile ) ? 2 : 1 ] ) );
							if ( tileMatrix.Count == 2 && tile.isUpwardTile ) {
								bottom.Add( tile.transform.TransformPoint( tile.tileSurface[ 1 ] ) );
							}
						} else {
							if ( !tile.isUpwardTile ) {
								bottom.Add( tile.transform.TransformPoint( tile.tileSurface[ 1 ] ) );
							}
						}
					} else {
						if ( tileCol == -divider ) {
							if ( tileRow == 1 && tile.isUpwardTile ) {
								left.Add( tile.transform.TransformPoint( tile.tileSurface[ 1 ] ) );
							}
							if ( tileMatrix.Count == 3 || ( tileMatrix[ tileRow ][ 0 ] != tileMatrix[ tileRow - 1 ][ 0 ] && tileRow != 1 ) || ( tileMatrix[ tileRow ][ 0 ] != tileMatrix[ tileRow + 1 ][ 0 ] && tileRow != tileMatrix.Count - 2 ) ) {
								left.Add( tile.transform.TransformPoint( tile.tileSurface[ 0 ] ) );
							}
							if ( tileRow == tileMatrix.Count - 2 && !tile.isUpwardTile ) {
								left.Add( tile.transform.TransformPoint( tile.tileSurface[ 1 ] ) );
							}
						} else if ( tileCol == divider ) {
							if ( tileRow == 1 && tile.isUpwardTile ) {
								right.Add( tile.transform.TransformPoint( tile.tileSurface[ 1 ] ) );
							}
							if ( tileMatrix.Count == 3 || ( tileMatrix[ tileRow ][ 0 ] != tileMatrix[ tileRow - 1 ][ 0 ] && tileRow != 1 ) || ( tileMatrix[ tileRow ][ 0 ] != tileMatrix[ tileRow + 1 ][ 0 ] && tileRow != tileMatrix.Count - 2 ) ) {
								right.Add( tile.transform.TransformPoint( tile.tileSurface[ 2 ] ) );
							}
							if ( tileRow == tileMatrix.Count - 2 && !tile.isUpwardTile ) {
								right.Add( tile.transform.TransformPoint( tile.tileSurface[ 1 ] ) );
							}
						}
					}
				}
			}

			// assemble the entire border:
			List<Vector3> border = new List<Vector3>();
			border.AddRange( top );
			border.AddRange( right );
			bottom.Reverse();
			border.AddRange( bottom );
			left.Reverse();
			border.AddRange( left );

			return border;
		}

		/* Used to determine and update the population center areas - borders and outskirts. */
		private void DeterminePopulationCenterAreas( ref PopulationCenter populationCenter ) {
			// generate a tile matrix:
			List<int[]> tileMatrix = GenerateTileMatrix( populationCenter.size, GetTileByLocation( populationCenter.centerLocation ).isUpwardTile );

			// based on the matrix determine the surrounding tiles:
			populationCenter.borders.tiles = new List<Location>();
			for ( int tileRow = 0 ; tileRow < tileMatrix.Count ; tileRow++ ) {
				int divider = Mathf.FloorToInt( tileMatrix[ tileRow ][ 0 ] / 2 );
				for ( int tileCol = -divider ; tileCol <= divider ; tileCol++ ) {
					populationCenter.borders.tiles.Add( new Location( populationCenter.centerLocation.row + tileMatrix[ tileRow ][ 1 ], EnsureColWithinBoundary( populationCenter.centerLocation.col + tileCol ) ) );
				}
			}
			populationCenter.borders.coordinates = TraceBorders( tileMatrix, populationCenter.centerLocation );

			// if the population center has outskirts calculate them now:
			if ( (int)populationCenter.size >= (int)PopulationCenterSize.medium ) {
				int outskirtsRadius = ExpandTileMatrix( ref tileMatrix, populationCenter.size );

				// determine the outskirts tiles:
				populationCenter.outskirts.tiles = new List<Location>();
				for ( int tileRow = 0 ; tileRow < tileMatrix.Count ; tileRow++ ) {
					int divider = Mathf.FloorToInt( tileMatrix[ tileRow ][ 0 ] / 2 );
					for ( int tileCol = -divider ; tileCol <= divider ; tileCol++ ) {
						if ( ( tileRow < outskirtsRadius / 2 ) || ( tileRow >= tileMatrix.Count - outskirtsRadius / 2 ) || tileCol < -divider + outskirtsRadius || tileCol > divider - outskirtsRadius ) {
							populationCenter.outskirts.tiles.Add( new Location( populationCenter.centerLocation.row + tileMatrix[ tileRow ][ 1 ], EnsureColWithinBoundary( populationCenter.centerLocation.col + tileCol ) ) );
						}
					}
				}
				populationCenter.outskirts.coordinates = TraceBorders( tileMatrix, populationCenter.centerLocation );

				// if there are villages calculate their surrounding border tiles as well:
				List<string> populationGroupKeys = new List<string>( populationCenter.populationGroups.Keys );
				foreach ( string populationGroupUID in populationGroupKeys ) {
					PopulationGroup populationGroup;
					if ( populationCenter.populationGroups.TryGetValue( populationGroupUID, out populationGroup ) ) {
						if ( populationGroup.type == PopulationGroupType.village ) {
							// basically villages are like tiny population centers:
							List<int[]> villageTileMatrix = GenerateTileMatrix( PopulationCenterSize.tiny, GetTileByLocation( populationGroup.location ).isUpwardTile );

							populationGroup.borderArea.tiles = new List<Location>();
							for ( int tileRow = 0 ; tileRow < villageTileMatrix.Count ; tileRow++ ) {
								var divider = Mathf.FloorToInt( villageTileMatrix[ tileRow ][ 0 ] / 2 );
								for ( int tileCol = -divider ; tileCol <= divider ; tileCol++ ) {
									populationGroup.borderArea.tiles.Add( new Location( populationGroup.location.row + villageTileMatrix[ tileRow ][ 1 ], EnsureColWithinBoundary( populationGroup.location.col + tileCol ) ) );
								}
							}

							populationCenter.populationGroups[ populationGroupUID ] = populationGroup;
						}
					}
				}
			}
		}

		/* Used to determine and update the housing availability on all tiles inside the population center areas. */
		private void DeterminePopulationCenterHousing( ref PopulationCenter populationCenter ) {
			foreach ( KeyValuePair<string,PopulationGroup> populationGroup in populationCenter.populationGroups ) {
				/**
	   			 * Game Rule: Population Group Housing
	   			 * The central tiles of the population group have limit on how many populations they can house
				 */
				GetTileByLocation( populationGroup.Value.location ).isHousingAvailable = ( populationGroup.Value.population.Count < maxPopulationPerTile ) ? true : false;

				/**
	   			 * Game Rule: Population Group Housing
	   			 * In addition, village borders do not allow any population in them
				 */
				if ( populationGroup.Value.type == PopulationGroupType.village ) {
					foreach ( Location location in populationGroup.Value.borderArea.tiles ) {
						if ( location.row != populationGroup.Value.location.row && location.col != populationGroup.Value.location.col ) {
							GetTileByLocation( populationGroup.Value.location ).isHousingAvailable = false;
						}
					}
				}
			}
		}


		/**
		 * Helper functions
		 */

		/* Used to generate an unique ID for any of the game objects that need to be distinguished. */
		public string GenerateUID() {
			return Guid.NewGuid().ToString( "N" );
		}

		/* Gets the tile in the provided location. */
		public GameTile GetTileByLocation( Location tileLocation ) {
			return currentGame.worldTiles[ tileLocation.row, tileLocation.col ];
		}

		/* Makes sure that the world tile column provided is within the would width boundaries. Overflow will be automatically adjusted to point to a valid column instead. */
		public int EnsureColWithinBoundary( int col ) {
			int properCol = col;

			if ( col < 0 ) {
				properCol = worldWidthInTiles + col;
			} else if ( col >= worldWidthInTiles ) {
				properCol = col - worldWidthInTiles;
			}

			return properCol;
		}

		/* Used to generate tile matrix around a certain population center size and its center tile direction. */
		public List<int[]> GenerateTileMatrix( PopulationCenterSize size, bool isUpwardTile ) {
			List<int[]> tileMatrix = new List<int[]>();

			/**
	   		 * Game Rule: Tile Matrix
	   		 * The tile matrix defines the area of a poulation center based on its size
	   		 * It is a list of arrays each consisting of two elements where:
	   		 *    the first element determines the number of tiles in that row
	   		 *    the second element determines the offset of the row in regards to the base row (i.e. where the center tile is contained)
	   		 * The base row is always at offset 0
			 */
			tileMatrix.Add( new int[] { 1, 0 } );

			int zeroIdx = 0;
			for ( int currentSize = 1 ; currentSize <= (int)size ; currentSize++ ) {
				bool isEven = ( tileMatrix.Count + 1 ) % 2 == 0;

				if ( isEven ) {
					tileMatrix.Insert( 0, new int[] { 0, 100 } );
					zeroIdx++;
				} else {
					tileMatrix.Add( new int[] { 0, 100 } );
				}

				tileMatrix[ zeroIdx ][ 0 ] = tileMatrix[ zeroIdx ][ 0 ] + 2;

				for ( int idx = 0 ; idx < tileMatrix.Count ; idx++ ) {
					tileMatrix[ idx ][ 1 ] = zeroIdx - idx;
					if ( ( idx == zeroIdx - 1 && !isEven ) || ( idx == zeroIdx + 1 && isEven ) ) {
						tileMatrix[ idx ][ 0 ] = tileMatrix[ zeroIdx ][ 0 ];
					} else if ( idx != zeroIdx ) {
						var zeroRowModifier = ( ( isEven && idx > zeroIdx ) || ( !isEven && idx < zeroIdx ) ) ? 1 : 0;
						tileMatrix[ idx ][ 0 ] = tileMatrix[ zeroIdx ][ 0 ] - ( 2 * ( Mathf.Abs( idx - zeroIdx ) - zeroRowModifier ) );
					}
				}
			}

			// if the center tile is inverted we also need to invert the matrix:
			if ( isUpwardTile ) {
				tileMatrix.Reverse();
				for ( int row = 0 ; row < tileMatrix.Count ; row++ ) {
					tileMatrix[ row ][ 1 ] = -tileMatrix[ row ][ 1 ];
				}
			}

			return tileMatrix;
		}

		/* Used to expand a tile matrix to include the outskirts of a population center. It will return the outskirts radius (0 if there are no outskirts). */
		public int ExpandTileMatrix( ref List<int[]> tileMatrix, PopulationCenterSize size ) {
			int outskirtsRadius = 0;

			/**
			 * Game Rule: Expanded Tile Matrix
			 * The expanded tile matrix defines the outskirts area of a population center based on its size
			 * Population center below the size of 'medium' does not have outskirts so if such a size is possed as param
			 *    this function will not modify the tile matrix at all
			 * The outskirts radius increases with the size of the population center - it is 2 times the size identifier as defined
			 *    in the enum 'PopulationCenterSize'
			 */
			if ( (int)size >= (int)PopulationCenterSize.medium ) {
				outskirtsRadius = (int)size * 2;
				for ( int idx = 0 ; idx < outskirtsRadius / 2 ; idx++ ) {
					tileMatrix.Insert( 0, new int[] { tileMatrix[ 0 ][ 0 ] - 2, tileMatrix[ 0 ][ 1 ] - 1 } );
					tileMatrix.Add( new int[] {
						tileMatrix[ tileMatrix.Count - 1 ][ 0 ] - 2,
						tileMatrix[ tileMatrix.Count - 1 ][ 1 ] + 1
					} );
				}

				for ( int tileRow = 0 ; tileRow < tileMatrix.Count ; tileRow++ ) {
					tileMatrix[ tileRow ][ 0 ] += ( outskirtsRadius * 2 );
				}
			}

			return outskirtsRadius;
		}

		private void PrintTileMatrix( List<int[]> tileMatrix, string info = "Tile Matrix" ) {
			string log = "--- " + ( ( info != null ) ? info : "Tile Matrix" ) + " ---\r\n";

			for ( int idx = 0 ; idx < tileMatrix.Count ; idx++ ) {
				log += tileMatrix[ idx ][ 0 ].ToString() + " : " + tileMatrix[ idx ][ 1 ].ToString() + "\r\n";
			}

			log += "-------------------";

			Debug.Log( log );
		}
	}

}
