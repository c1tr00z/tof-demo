using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SystemFriend.ARTest {
    public class ObjectPlacement : MonoBehaviour {

        #region Private Fields

        private Transform _mainCameraTransform;

        #endregion
        
        #region Serialized Fields

        [SerializeField] private Camera _mainCamera;

        [SerializeField] private GameObject _spawnObjectPrefab;

        [SerializeField] private ARRaycastManager _arRaycastManager;

        #endregion

        #region Accessors

        private Transform mainCameraTransform {
            get {
                if (_mainCamera == null) {
                    return null;
                }
                
                if (_mainCameraTransform == null) {
                    _mainCameraTransform = _mainCamera.transform;
                }
                
                return _mainCameraTransform;
            }
        }

        #endregion

        #region Unity Events

        private void Update() {
            Touch touch;
            if (Input.touchCount > 0 && (touch = Input.GetTouch(0)).phase == TouchPhase.Began) {
                var hitResults = new List<ARRaycastHit>();

                if (!_arRaycastManager.Raycast(touch.position, hitResults, TrackableType.PlaneWithinPolygon)) {
                    return;
                }
                
                hitResults.ForEach(hit => {
                    var hitPosition = hit.pose.position;
                    if (Vector3.Dot(mainCameraTransform.position - hitPosition, hit.pose.up) == 0) {
                        return;
                    }

                    hitPosition.y += 0.15f;
                    var planeObject = Instantiate(_spawnObjectPrefab, hitPosition, hit.pose.rotation);
                });
            }
        }

        #endregion
    }
}