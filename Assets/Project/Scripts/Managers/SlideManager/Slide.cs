using UnityEngine;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public abstract class Slide : MonoBehaviour
    {

        public abstract void Play();

        public virtual void Hide() { }
        public abstract void Stop();

        protected void OnDestroy() => Stop();
    }
}
