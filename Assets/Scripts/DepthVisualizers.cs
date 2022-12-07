using System;
using System.Collections.Generic;
using System.Linq;
using Lean.Touch;
using UnityEngine;

namespace SystemFriend.ARTest {
    public class DepthVisualizers : MonoBehaviour {
        
        #region Serialized Fields

        [SerializeField] private List<DepthVisualizerBase> _visualizers = new List<DepthVisualizerBase>();

        #endregion

        #region Accessors

        public static DepthVisualizers instance { get; private set; }

        #endregion

        #region Unity Events

        private void Awake() {
            if (instance != null) {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        #endregion

        #region Class Implementation

        public bool IsVisualizerEnabled<T>() where T : DepthVisualizerBase {
            var visualizer = _visualizers.OfType<T>().FirstOrDefault();
            if (visualizer == null) {
                return false;
            }
            return visualizer.gameObject.activeSelf;
        }

        private void DisableAll() {
            _visualizers.ForEach(v => v.gameObject.SetActive(false));
        }

        public void Enable<T>() where T : DepthVisualizerBase {
            DisableAll();
            var visualizer = _visualizers.OfType<T>().FirstOrDefault();
            visualizer.gameObject.SetActive(true);
        }

        public T GetVisualizer<T>() where T : DepthVisualizerBase {
            return _visualizers.OfType<T>().FirstOrDefault();
        }

        #endregion

    }
}