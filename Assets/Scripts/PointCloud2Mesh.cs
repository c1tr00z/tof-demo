using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using TofAr.V0;
using TofAr.V0.Color;
using TofAr.V0.Tof;
using TofArSettings.Tof;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SystemFriend.ARTest {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PointCloud2Mesh : DepthVisualizerBase {

        #region Private Fields

        private MeshFilter _meshFilter;

        private Mesh _mesh;

        private bool _takeImage;

        private Vector3 _startAngles;
        
        private Vector3 _targetAngles;

        #endregion

        #region Serialized Fields
        
        [SerializeField]
        [Range(1, 20)]
        private int _resolutionDivider = 5;

        [SerializeField] private float _swipeSpeed = 1;

        #endregion

        #region Accessors

        private MeshFilter meshFilter {
            get {
                if (_meshFilter == null) {
                    _meshFilter = GetComponent<MeshFilter>();
                }

                return _meshFilter;
            }
        }

        private int resolutionDivider => Mathf.Max(1, _resolutionDivider);

        private int horizontal {
            get {
#if UNITY_EDITOR
                return 144;
#else
                var tofManagerController = FindObjectOfType<TofManagerController>();
                if (tofManagerController == null) {
                    Debug.LogError("!!!!!!!!!!! [System Friend AR Test] No manager controller on scene");
                    return 0;
                }

                if (tofManagerController.CurrentConfig == null) {
                    return 0;
                }

                return tofManagerController.CurrentConfig.width;
#endif
            }
        }

        private int horizontalDivided => horizontal / resolutionDivider;

        #endregion

        #region Unity Events

        protected override void OnEnable() {
            base.OnEnable();
            _targetAngles = transform.rotation.eulerAngles;
            LeanTouch.OnFingerSwipe += LeanTouchOnOnFingerSwipe;
        }

        protected override void OnDisable() {
            base.OnDisable();
            LeanTouch.OnFingerSwipe -= LeanTouchOnOnFingerSwipe;
        }

        #endregion
        
        #region DepthVisualizerBase Implementation

        protected override void UpdateVisuals(List<Vector3> vs) {

            if (!_takeImage) {
                return;
            }

            _takeImage = false;

            if (horizontalDivided == 0) {
                meshFilter.mesh = null;
                return;
            }

            if (_mesh == null) {
                _mesh = new Mesh();
                meshFilter.mesh = _mesh;
            }

            vs = DivideResolution(vs);

            var colors = new List<Color>();
            var triangles = new List<int>();
            for (int i = 0; i < vs.Count; i++) {
                var point = vs[i];
                
                var r = Mathf.Min(1, Mathf.Max(0, 1 - point.z / 2));
                var g = Mathf.Min(1, Mathf.Max(0, (point.y + 1) / 2));
                var b = Mathf.Min(1, Mathf.Max(0, (point.x + 1) / 2));
                colors.Add(new Color(r, g, b));

                if (i + horizontal + 1 >= vs.Count) {
                    continue;
                }

                triangles.Add(i);
                triangles.Add(i + horizontalDivided + 1);
                triangles.Add(i + horizontalDivided);
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + horizontalDivided + 1);
            }

            _mesh.vertices = vs.ToArray();
            _mesh.colors = colors.ToArray();
            _mesh.triangles = triangles.ToArray();
        }

        #endregion

        #region Class Implementation

        private List<Vector3> DivideResolution(List<Vector3> vs) {
            if (resolutionDivider == 1) {
                return vs;
            }
            var result = new List<Vector3>();
            var vertical = vs.Count / horizontal;
            for (int i = 0; i < vs.Count; i++) {
                var y = i / horizontal;
                var x = i % horizontal;
                if (x % resolutionDivider == 0 && y % resolutionDivider == 0) {
                    result.Add(vs[i]);
                }
            }

            return result;
        }

        [ContextMenu("Regenerate")]
        private void RegeneratePoints() {
            ScheduleTakeImage();
            var vertical = 256;
            var horizontal = 144;
            var startY = vertical / 2 * 0.01f;
            var startX = horizontal / 2 * 0.01f * -1;
            var points = new List<Vector3>();
            for (int y = 0; y < vertical; y++) {
                for (int x = 0; x < horizontal; x++) {
                    var coordX = startX + x * 0.01f;
                    var coordY = startY + y * -0.01f;
                    var coordZ = 1f - coordX + coordY; 
                    points.Add(new Vector3(coordX, coordY, coordZ));
                }
            }
            UpdateVisuals(points);
        }

        public void ScheduleTakeImage() {
            transform.rotation = Quaternion.identity;
            _takeImage = true;
            StopCoroutine(nameof(C_Rotate));
        }

        private void LeanTouchOnOnFingerSwipe(LeanFinger finger) {
            if (!finger.Swipe) {
                return;
            }

            var delta = finger.ScaledDelta;
            _targetAngles = transform.rotation.eulerAngles + new Vector3(delta.y, delta.x, 0) * _swipeSpeed;
            StopCoroutine(nameof(C_Rotate));
            StartCoroutine(nameof(C_Rotate));
        }

        private IEnumerator C_Rotate() {
            while ((_targetAngles - transform.rotation.eulerAngles).magnitude > 0.01f) {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_targetAngles), Time.deltaTime * 10);
                yield return null;
            }
        }

        #endregion
    }
}