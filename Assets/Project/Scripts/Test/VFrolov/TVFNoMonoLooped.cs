using BigProject.Systems;
using UnityEngine;

namespace BigProject.Test.VFrolov
{
    public class TVFNoMonoLooped : ITickable//, IFixedTickable, ILateTickable
    {
        private string _name;

        public TVFNoMonoLooped(string name) => _name = name;

        public void Tick() => Debug.Log($"{_name} Tick");

        public void FixedTick() => Debug.Log($"{_name} FixedTick");

        public void LateTick() => Debug.Log($"{_name} LateTick");
    }
}