using System.Collections.Generic;
using UnityEngine;

namespace SystemFriend.ARTest {
    public class PointCloud2ParticleSystem : DepthVisualizerBase {

        #region Private Fields

        private ParticleSystem _particleSystem;

        private ParticleSystem.Particle[] _particles;

        #endregion

        #region Unity Events

        private void Start() {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        #endregion

        #region DepthVisualizerBase Implementation

        protected override void UpdateVisuals(List<Vector3> vs) {
            _particles = new ParticleSystem.Particle[vs.Count];
            _particleSystem.GetParticles(_particles);

            for (int i = 0; i < _particles.Length; i++) {
                var point = vs[i];

                _particles[i].position = point;

                var r = Mathf.Min(1, Mathf.Max(0, point.z / 2));
                var g = Mathf.Min(1, Mathf.Max(0, (point.y + 1) / 2));
                var b = Mathf.Min(1, Mathf.Max(0, (point.x + 1) / 2));

                _particles[i].startColor = new Color(r, g, b);
                _particles[i].startSize = 0.02f;
            }
                
            _particleSystem.SetParticles(_particles);
        }

        #endregion
    }
}