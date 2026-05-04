using BigProject.Systems;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BigProject.UI
{
    [RequireComponent(typeof(Image))]
    public class GlintController : MonoBehaviour
    {
        private Image _image;
        private Material _glintMat;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.material = new Material(_image.material);
            _glintMat = _image.material;
            Assert.IsNotNull(_glintMat, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Glint Material"));
            Stop();
        }

        public void Play()
        {
            _glintMat.SetFloat("_StartTime", Time.time);
            _glintMat.EnableKeyword("ENABLE_GLINT");
        }

        public void Stop()
        {
            _glintMat.DisableKeyword("ENABLE_GLINT");
        }

        public void SetMask(bool r, bool g, bool b)
        {
            Vector4 selector = new(r ? 1 : 0, g ? 1 : 0, b ? 1 : 0, 0);
            _glintMat.SetVector("_ChannelSelector", selector);
        }

        private void OnDestroy()
        {
            if (_image.material != null)
            {
                Destroy(_image.material);
            }
        }
    }
}