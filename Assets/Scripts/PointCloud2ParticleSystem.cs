using System.Collections.Generic;
using System.Linq;
using TofAr.V0.Tof;
using UnityEngine;

namespace SystemFriend.ARTest {
    public class PointCloud2ParticleSystem : MonoBehaviour {

        #region Private Fields

        private ParticleSystem _particleSystem;

        private ParticleSystem.Particle[] _particles;

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

        private void Start() {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Update() {
            if (!_updated) {
                return;
            }

            _updated = false;

            lock (_sync) {
                _particles = new ParticleSystem.Particle[_vs.Count];
                _particleSystem.GetParticles(_particles);

                for (int i = 0; i < _particles.Length; i++) {
                    var point = _vs[i];

                    _particles[i].position = point;

                    var r = Mathf.Min(1, Mathf.Max(0, point.z / 2));
                    var g = Mathf.Min(1, Mathf.Max(0, (point.y + 1) / 2));
                    var b = Mathf.Min(1, Mathf.Max(0, (point.x + 1) / 2));

                    _particles[i].startColor = new Color(r, g, b);
                    _particles[i].startSize = 0.02f;
                }
                
                _particleSystem.SetParticles(_particles);
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
        
        // private void TestParse() {
        //     var inputString =
        //         "979.858826,979.858826,636.952942,360.374817,186.003343,186.003343,123.793625,71.909279,1,0,0,0,1,0,0,0,1,0,0,0,1280,720";
        //     var pasteSetting = inputString.Split(',');
        //     if (pasteSetting.Length < 20)
        //     {
        //         throw new System.ArgumentException("the given text must have 20 floats separated by commas then 2 ints");
        //     }
        //     int colorwidth = 0;
        //     int colorheight = 0;
        //     if (pasteSetting.Length > 21)
        //     {
        //         colorwidth = int.Parse(pasteSetting[20]);
        //         colorheight = int.Parse(pasteSetting[21]);
        //     }
        //     
        //     // float fxColor = float.Parse(pasteSetting[0]);
        //     if (!float.TryParse(pasteSetting[0], out float fxColor)) {
        //         Debug.LogError(string.Format(">>> Debug AAAAAA {0} ::: {1}", 0, pasteSetting[0]));
        //     }
        //     Debug.LogError(string.Format(">>> Debug {0} ::: {1}", 1, pasteSetting[1]));
        //     float fyColor = float.Parse(pasteSetting[1]);
        // }

        #endregion
    }
}