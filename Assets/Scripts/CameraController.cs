using UnityEngine;
using System.Collections;

namespace Civilization {

	public class CameraController : MonoBehaviour {
		/* Camera singleton */
		public static CameraController instance { get; private set; }

		public struct BoxLimit {
			public float left;
			public float right;
			public float top;
			public float bottom;
		}

		public static BoxLimit scrollLimits = new BoxLimit();

		public GameObject gameTilesContainer;
		public Camera mainCamera;
		public LayerMask terrainOnly;

		private float cameraMoveTilesPerSecond = 3.5f;
		private float cameraMoveSpeed;
		private float cameraOrbitSpeed;
		private float cameraSpeedBoost = 2.5f;
		private float cameraPanEase;
		private float cameraDistanceMin;
		private float cameraDistanceMax;
		private float cameraSmoothTime = 0.02f;
		private float cameraZoomEase;
		private float cameraZoomIgnoreThreshold = 0.01f;
		private float cameraTiltAngleMax = 15f;
		private float mouseInBoundary = 25f;

		private float lastMouseX;
		private float lastMouseY;

		/* Keyboard & Mouse controlls */
		public static KeyCode keyScrollUp = KeyCode.W;
		public static KeyCode keyScrollDown = KeyCode.S;
		public static KeyCode keyScrollLeft = KeyCode.A;
		public static KeyCode keyScrollRight = KeyCode.D;
		public static KeyCode keyCameraMoveBoost = KeyCode.LeftShift;
		public static bool reverseScrollZoom = false;

		/* System function. */
		void Awake() {
			// make the game camera a singleton:
			instance = this;
		}

		/* System function. */
		void Start() {
			scrollLimits.left = mouseInBoundary;
			scrollLimits.right = mouseInBoundary;
			scrollLimits.top = mouseInBoundary;
			scrollLimits.bottom = mouseInBoundary;

			// calculate distances and ease factors based on tile size:
			cameraDistanceMin = GameManager.instance.tileSide * 3.5f;
			cameraDistanceMax = GameManager.instance.tileSide * 8.5f;
			cameraPanEase = GameManager.instance.tileSide / 10f;
			cameraZoomEase = GameManager.instance.tileSide * 2.5f;

			// calculate camera move and orbit speeds - we want to move x tiles per second in either direction where x is our 'cameraMoveTilesPerSecond' property:
			cameraMoveSpeed = cameraMoveTilesPerSecond * GameManager.instance.tileBisector;
			cameraOrbitSpeed = 2 * cameraMoveTilesPerSecond * GameManager.instance.tileRotationAngle;

			// make sure the camera is at min distance from the surface:
			instance.transform.Translate( new Vector3( 0, GameManager.instance.worldHeight / 2, GameManager.instance.worldRadius + GameManager.instance.tileHeight + cameraDistanceMin ) );
			mainCamera.transform.rotation = Quaternion.Euler( -cameraTiltAngleMax, 180f, 0 );
		}

		/* System function. */
		void LateUpdate() {
			// use a combination of camera scroll and pan in case both are active at the same time:
			Vector3 desiredMovement = CameraScroll() + CameraPan() + CameraZoom();

			MoveCamera( desiredMovement, Time.smoothDeltaTime );
		}

		/* Used to focus the camera at specific tile and optionally restrict the scrolling boundaries within new limits. */
		public void FocusAtTile( GameTile tile ) {
			RaycastHit hit;

			// make sure we have an intersection point with the terrain (i.e. the tiles):
			if ( Physics.Raycast( instance.transform.position, mainCamera.transform.forward, out hit, cameraDistanceMax * 2, terrainOnly ) ) {
				Debug.DrawRay( hit.point, instance.transform.position, Color.red, Mathf.Infinity );

				Vector3 tileSurfaceProjection = tile.transform.position;
				tileSurfaceProjection.z += GameManager.instance.tileHeight;
				Vector3 alignedCameraPosition = hit.point;
				alignedCameraPosition.y += tileSurfaceProjection.y - hit.point.y;

				Debug.DrawLine( hit.point, tileSurfaceProjection, Color.blue, Mathf.Infinity );
				Debug.DrawLine( hit.point, alignedCameraPosition, Color.green, Mathf.Infinity );

				Vector3 requiredMovement = new Vector3( 0, 0, 0 );

				int angleSign = Vector3.Cross( tileSurfaceProjection, alignedCameraPosition ).z < 0 ? -1 : 1;
				requiredMovement.x = angleSign * Vector3.Angle( tileSurfaceProjection, alignedCameraPosition );
				requiredMovement.y = tileSurfaceProjection.y - hit.point.y;

				Debug.Log( "FocusAtTile: " + requiredMovement );

				MoveCamera( requiredMovement, 1f );
			}
		}

		/* Handles all camera scrolling including the keyboard controlls and edge scrolling. */
		private Vector3 CameraScroll() {
			Vector3 scrollMovement = new Vector3( 0, 0, 0 );

			if ( IsCameraScrollButtonPressed() || IsMouseWithinScrollBoundaries() ) {
				bool isBoostActive = false;
				if ( Input.GetKey( keyCameraMoveBoost ) ) {
					isBoostActive = true;
				}

				// handle movement by keyboard or scroll edges:
				if ( Input.GetKey( keyScrollUp ) || ( Input.mousePosition.y > ( Screen.height - scrollLimits.top ) && Input.mousePosition.y < Screen.height ) ) {
					scrollMovement.y = cameraMoveSpeed * ( isBoostActive ? cameraSpeedBoost : 1 );
				}
				if ( Input.GetKey( keyScrollDown ) || ( Input.mousePosition.y < scrollLimits.bottom && Input.mousePosition.y >= 0 ) ) {
					scrollMovement.y = -cameraMoveSpeed * ( isBoostActive ? cameraSpeedBoost : 1 );
				}
				if ( Input.GetKey( keyScrollLeft ) || ( Input.mousePosition.x < scrollLimits.left && Input.mousePosition.x >= 0 ) ) {
					scrollMovement.x = cameraOrbitSpeed * ( isBoostActive ? cameraSpeedBoost : 1 );
				}
				if ( Input.GetKey( keyScrollRight ) || ( Input.mousePosition.x > ( Screen.width - scrollLimits.right ) && Input.mousePosition.x < Screen.width ) ) {
					scrollMovement.x = -cameraOrbitSpeed * ( isBoostActive ? cameraSpeedBoost : 1 );
				}
			}

			return scrollMovement;
		}

		/* Handles the camera panning using the mouse right button. */
		private Vector3 CameraPan() {
			Vector3 panMovement = new Vector3( 0, 0, 0 );

			if ( Input.GetMouseButton( 1 ) ) {
				if ( Input.mousePosition.x != lastMouseX || Input.mousePosition.y != lastMouseY ) {
					panMovement.x = ( ( Input.mousePosition.x - lastMouseX ) / cameraPanEase ) * cameraOrbitSpeed;
					panMovement.y = -( ( Input.mousePosition.y - lastMouseY ) / cameraPanEase ) * cameraMoveSpeed;
				}
			}

			// remember the last mouse position:
			lastMouseX = Input.mousePosition.x;
			lastMouseY = Input.mousePosition.y;

			return panMovement;
		}

		/* Handles the camera zoom using the scroll wheel. */
		private Vector3 CameraZoom() {
			Vector3 zoomMovement = new Vector3( 0, 0, 0 );
			float scrollWheelValue = Input.GetAxis( "Mouse ScrollWheel" ) * cameraZoomEase;

			if ( scrollWheelValue != 0f && ( scrollWheelValue < -cameraZoomIgnoreThreshold || scrollWheelValue > cameraZoomIgnoreThreshold ) ) {
				zoomMovement.z = scrollWheelValue * ( ( reverseScrollZoom ) ? 1 : -1 );
			}

			return zoomMovement;
		}

		/* Handles actual camera movements up / down and around the world base. */
		private void MoveCamera( Vector3 movement, float smoothFactor ) {
			// make sure we don do unnecessary calculations:
			if ( movement.x != 0f || movement.y != 0f || movement.z != 0f ) {
				Vector3 verticalTranslation = new Vector3( 0, movement.y * smoothFactor, 0 );
				Vector3 worldTranslation = instance.transform.TransformPoint( verticalTranslation );

				// make sure to check the up and down limits:
				float limitDifference = 0f;
				if ( worldTranslation.y >= GameManager.instance.worldHeight ) {
					limitDifference = GameManager.instance.worldHeight - worldTranslation.y;
				} else if ( worldTranslation.y <= 0 ) {
					limitDifference = 0 - worldTranslation.y;
				}

				// the vertical movement adjust both vetical postion and distance:
				verticalTranslation = new Vector3( 0, movement.y * smoothFactor + limitDifference, 0 );
				instance.transform.Translate( verticalTranslation );

				// the horizontal rotation has no limits (i.e. orbit in a circle):
				instance.transform.RotateAround( gameTilesContainer.transform.position, Vector3.up, movement.x * smoothFactor );

				// now calculate the distance adjustments:
				RaycastHit hit;
				Vector3 cameraWorldDirection = -( gameTilesContainer.transform.position + instance.transform.position );

				// make sure we have an intersection point with the terrain (i.e. the tiles) and the zoom is large enough to make sense showing:
				if ( Physics.Raycast( instance.transform.position, cameraWorldDirection, out hit, cameraDistanceMax * 2, terrainOnly ) && Mathf.Abs( movement.z ) >= 0.5f ) {
					float currentDistance = Vector3.Distance( instance.transform.position, hit.point );
					float targetDistance = currentDistance + movement.z;
					Vector3 velocity = Vector3.zero;

					if ( targetDistance < cameraDistanceMin ) {
						targetDistance = cameraDistanceMin;
					} else if ( targetDistance > cameraDistanceMax ) {
						targetDistance = cameraDistanceMax;
					}

					// apply smooth distance transition:
					Vector3 targetPosition = ( instance.transform.position - hit.point ).normalized * targetDistance + hit.point;
					instance.transform.position = Vector3.SmoothDamp( instance.transform.position, targetPosition, ref velocity, cameraSmoothTime, Mathf.Infinity, smoothFactor );

					// adjust the camera tilt angle:
					float targetTiltAngle = 360f - ( 1 - ( targetDistance - cameraDistanceMin ) / ( cameraDistanceMax - cameraDistanceMin ) ) * cameraTiltAngleMax;
					float tiltDifference = targetTiltAngle - mainCamera.transform.rotation.eulerAngles.x;

					// avoid unnecessary rotation calls:
					if ( tiltDifference != 0 && tiltDifference != 360f ) {
						mainCamera.transform.Rotate( new Vector3( tiltDifference, 0, 0 ) );
					}
				}
			}
		}

		/**
		 * Helper functions
		 */

		/* Verifies whether the camera scrolling buttons are pressed. */
		public static bool IsCameraScrollButtonPressed() {
			if ( Input.GetKey( keyScrollUp ) || Input.GetKey( keyScrollDown ) || Input.GetKey( keyScrollLeft ) || Input.GetKey( keyScrollRight ) ) {
				return true;
			} else {
				return false;
			}
		}

		/* Verifies whether the mouse is within the screen scrolling edges. */
		public static bool IsMouseWithinScrollBoundaries() {
			if ( ( Input.mousePosition.x < scrollLimits.left && Input.mousePosition.x >= 0 ) ||
			     ( Input.mousePosition.x > ( Screen.width - scrollLimits.right ) && Input.mousePosition.x < Screen.width ) ||
			     ( Input.mousePosition.y < scrollLimits.bottom && Input.mousePosition.y >= 0 ) ||
			     ( Input.mousePosition.y > ( Screen.height - scrollLimits.top ) && Input.mousePosition.y < Screen.height ) ) {
				return true;
			} else {
				return false;
			}
		}
	}

}
