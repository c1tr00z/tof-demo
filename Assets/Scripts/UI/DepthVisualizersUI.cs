using System;
using UnityEngine;
using UnityEngine.UI;

namespace SystemFriend.ARTest.UI {
    public class DepthVisualizersUI : MonoBehaviour {

        #region Serialized Fields

        [SerializeField] private Button _buttonEnableParticles;
        [SerializeField] private Button _buttonEnableMesh;
        [SerializeField] private Button _buttonTakeImage;

        #endregion

        #region Accessors

        private bool isAvailableForInteraction => DepthVisualizers.instance != null;

        #endregion

        #region Unity Events

        private void OnEnable() {
            UpdateVisuals();
        }

        #endregion
        
        #region Class Implementation

        public void EnableParticles() {
            if (!isAvailableForInteraction) {
                return;
            }
            
            DepthVisualizers.instance.Enable<PointCloud2ParticleSystem>();
            UpdateVisuals();
        }

        public void EnableMesh() {
            if (!isAvailableForInteraction) {
                return;
            }
            
            DepthVisualizers.instance.Enable<PointCloud2Mesh>();
            UpdateVisuals();
        }

        public void TakeImage() {
            if (!isAvailableForInteraction) {
                return;
            }
            DepthVisualizers.instance.GetVisualizer<PointCloud2Mesh>().ScheduleTakeImage();
        }

        private void UpdateVisuals() {
            if (!isAvailableForInteraction) {
                return;
            }

            _buttonEnableParticles.gameObject.SetActive(!DepthVisualizers.instance.IsVisualizerEnabled<PointCloud2ParticleSystem>());
            _buttonEnableMesh.gameObject.SetActive(!DepthVisualizers.instance.IsVisualizerEnabled<PointCloud2Mesh>());
            _buttonTakeImage.gameObject.SetActive(DepthVisualizers.instance.IsVisualizerEnabled<PointCloud2Mesh>());
        }

        #endregion
    }
}