﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Civilization {

	[RequireComponent( typeof( MeshFilter ) )]
	[RequireComponent( typeof( MeshRenderer ) )]
	[RequireComponent( typeof( MeshCollider ) )]
	public class GameTile : MonoBehaviour {
		public bool isUpwardTile { get; private set; }
		public Vector3[] tileSurface { get; private set; }
		public Location tileLocation { get; private set; }
		public bool isHousingAvailable { get; set; }

		private int propAnchorsPerSide = 9;
		private Vector3 propAnchorPivot;
		private Vector3[][] propAnchorCoordinates;
		private GameObject[][] propAnchors;

		/* Used to initialize the tile properties. */
		public void Initialize( Location location, bool isUpward ) {
			this.tileLocation = location;
			this.isUpwardTile = isUpward;

			propAnchorCoordinates = new Vector3[propAnchorsPerSide][];
			propAnchors = new GameObject[propAnchorsPerSide][];
		}

		/* Used to generate the tile mesh and all related components. */
		public void CreateTileMesh( int side, float halfSide, float height, float delta, float bisector ) {
			MeshFilter meshFilter = GetComponent<MeshFilter>();
			MeshCollider meshCollider = GetComponent<MeshCollider>();
			if ( meshFilter == null ) {
				Debug.LogError( "MeshFilter not found!" );
				return;
			}

			// ordering of the vertices is extremely important in order to set the pivot point correctly (i.e. p0) pointing straight down to the zero vector:
			Vector3 p0 = new Vector3( 0, 0, 0 );
			Vector3 p1 = ( isUpwardTile ) ? new Vector3( 0, height, -bisector + delta ) : new Vector3( halfSide, height, -delta );
			Vector3 p2 = ( isUpwardTile ) ? new Vector3( halfSide, height, delta ) : new Vector3( 0, height, bisector - delta );
			Vector3 p3 = ( isUpwardTile ) ? new Vector3( -halfSide, height, delta ) : new Vector3( -halfSide, height, -delta );

			// preserve the tile surface points; the order should always be 0=pivotPoint, 1=verticalBisector, 2=remaining:
			tileSurface = new Vector3[3];
			tileSurface[ 0 ] = p3;
			tileSurface[ 1 ] = ( isUpwardTile ) ? p1 : p2;
			tileSurface[ 2 ] = ( isUpwardTile ) ? p2 : p1;

			// prop anchor pivot is always the p3:
			propAnchorPivot = p3;

			Mesh mesh = meshFilter.sharedMesh;
			if ( mesh == null ) {
				meshFilter.mesh = new Mesh();
				mesh = meshFilter.sharedMesh;
			}
			mesh.Clear();

			mesh.vertices = new Vector3[] {
				p0, p1, p2,
				p0, p2, p3,
				p2, p1, p3,
				p0, p3, p1
			};
			mesh.triangles = new int[] {
				0, 1, 2,
				3, 4, 5,
				6, 7, 8,
				9, 10, 11
			};

			// Mathf.Sqrt( 0.75f ) = 0.86602540378443864676372317075294
			// Mathf.Sqrt( 0.75f ) / 2 = 0.43301270189221932338186158537647
			Vector2 uv3a = new Vector2( 0f, 0f );
			Vector2 uv1 = new Vector2( 0.5f, 0f );
			Vector2 uv0 = new Vector2( 0.25f, 0.433013f );
			Vector2 uv2 = new Vector2( 0.75f, 0.433013f );
			Vector2 uv3b = new Vector2( 0.5f, 0.866025f );
			Vector2 uv3c = new Vector2( 1f, 0f );

			mesh.uv = new Vector2[] {
				uv2, uv1, uv3c,
				uv0, uv2, uv3b,
				uv0, uv1, uv2,
				uv0, uv3a, uv1
			};

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			mesh.Optimize();

			mesh.name = "TileBase";
			meshCollider.sharedMesh = mesh;
		}

		/* Used to calculate the prop anchor coordinates. */
		public void CalculatePropAnchors( float anchorOffsetX, float anchorOffsetZ, float anchorDeltaZ ) {
			// calculate the prop anchor coordinates and initialize the placeholders:
			for ( int rowIdx = 0 ; rowIdx < propAnchorsPerSide ; rowIdx++ ) {
				propAnchorCoordinates[ rowIdx ] = new Vector3[propAnchorsPerSide - rowIdx];
				propAnchors[ rowIdx ] = new GameObject[propAnchorsPerSide - rowIdx];

				float anchorDeltaX = anchorOffsetX / 2 * rowIdx;
				for ( int colIdx = 0 ; colIdx < propAnchorsPerSide - rowIdx ; colIdx++ ) {
					float offsetX = anchorOffsetX * ( colIdx + 1 ) + anchorDeltaX;
					float offsetZ = ( ( isUpwardTile ) ? -1 : 1 ) * ( anchorDeltaZ + anchorOffsetZ * rowIdx );
					propAnchorCoordinates[ rowIdx ][ colIdx ] = new Vector3( propAnchorPivot.x + offsetX, propAnchorPivot.y, propAnchorPivot.z + offsetZ );
				}
			}
		}

		/* Used to draw a prop attached to the specified anchor. */
		public void DrawPropAtAnchor( Location location, GameObject prop ) {
			propAnchors[ location.row ][ location.col ] = Instantiate<GameObject>( prop );
			propAnchors[ location.row ][ location.col ].transform.parent = this.transform;
			propAnchors[ location.row ][ location.col ].transform.rotation = this.transform.rotation;
			propAnchors[ location.row ][ location.col ].transform.localPosition = Vector3.zero;

			propAnchors[ location.row ][ location.col ].transform.Translate( propAnchorCoordinates[ location.row ][ location.col ], Space.Self );
		}

		/* Used to destroy a prop attached to the specified anchor. */
		public void DestroyPropAtAnchor( Location location ) {
			if ( propAnchors[ location.row ][ location.col ] != null ) {
				Destroy( propAnchors[ location.row ][ location.col ] );
				propAnchors[ location.row ][ location.col ] = null;
			}
		}

		/* Will return a list of empty prop locations. */
		public List<Location> GetAvailablePropLocations() {
			List<Location> propLocations = new List<Location>();

			for ( int rowIdx = 0 ; rowIdx < propAnchorsPerSide ; rowIdx++ ) {
				for ( int colIdx = 0 ; colIdx < propAnchorsPerSide - rowIdx ; colIdx++ ) {
					if ( propAnchors[ rowIdx ][ colIdx ] == null ) {
						propLocations.Add( new Location( rowIdx, colIdx ) );
					}
				}
			}

			return propLocations;
		}
	}

}
