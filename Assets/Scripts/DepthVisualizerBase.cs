using System.Collections.Generic;
using System.Linq;
using TofAr.V0.Tof;
using UnityEngine;

namespace SystemFriend.ARTest {
    public abstract class DepthVisualizerBase : MonoBehaviour {

        #region Private Fields

        private object _sync = new object();

        private bool _updated = false;

        private List<Vector3> _vs = new List<Vector3>();

        #endregion

        #region Unity Events

        private void OnEnable() {
            TofArTofManager.OnFrameArrived += OnFrameArrived;
        }

        private void OnDisable() {
            TofArTofManager.OnFrameArrived -= OnFrameArrived;
        }

        private void Update() {
            if (!_updated) {
                return;
            }

            _updated = false;

            lock (_sync) {
                UpdateVisuals(_vs);
            }
        }

        #endregion

        #region Class Implementation

        private void OnFrameArrived(object stream) {
            if (!TofArTofManager.Instantiated) {
                return;
            }

            var pointCloudData = TofArTofManager.Instance.PointCloudData;
            if (pointCloudData == null || pointCloudData.Points == null) {
                return;
            }

            lock (_sync) {
                _vs = pointCloudData.Points.ToList();
            }

            _updated = true;
        }

        protected abstract void UpdateVisuals(List<Vector3> vs);

        #endregion
        
    }
}