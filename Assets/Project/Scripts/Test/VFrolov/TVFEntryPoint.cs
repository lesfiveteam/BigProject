using BigProject.Managers;
using BigProject.Systems;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Test.VFrolov
{
    public class TVFEntryPoint : MonoBehaviour
    {
        [SerializeField]
        private ManualLoop _manualLoop;
        [SerializeField]
        private TVFChangeGameState _stateChanger;

        private void Awake()
        {
            TVFNoMonoLooped obj1 = new("Obj_1");
            TVFNoMonoLooped obj2 = new("Obj_2");
            TVFNoMonoLooped obj3 = new("Obj_3");

            List<object> objs = new() 
            {
                new TVFNoMonoLooped("Obj_4"),
                new TVFNoMonoLooped("Obj_5"),
                new TVFNoMonoLooped("Obj_6")
            };

            _manualLoop.AddTickable(obj1);
            _manualLoop.AddTickable(obj2);
            _manualLoop.AddTickable(obj3, 1);
            _manualLoop.AddTickables(objs, 2);
            _manualLoop.SetTickableQueueActive(1, false);
            _manualLoop.SetTickableQueueActive(2, false);


            GameplayManager gameplayManager = new(_manualLoop);
            gameplayManager.AddQueueToState(GameplayState.Play, 0);
            gameplayManager.AddQueueToState(GameplayState.Pause, 1);
            gameplayManager.AddQueueToState(GameplayState.Map, 1);
            gameplayManager.AddQueueToState(GameplayState.Map, 2);
            _stateChanger.Init(gameplayManager);
        }
    }
}