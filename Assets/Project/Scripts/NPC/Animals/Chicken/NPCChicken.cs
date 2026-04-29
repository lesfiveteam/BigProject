using BigProject.Utilities;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Chicken
{
    public class NPCChicken : NPCFowl
    {
        private const float DELAY_TIME_TO_CHANGE_PECK_POINT = 2f;

        [SerializeField] private Renderer _renderer;

        private NPCCock _cockLeader;

        private readonly Color From = new(1f, 1f, 1f); // #FFFFFF
        private readonly Color To = new(0.5f, 0.32f, 0f); // #805200

        protected override void Start()
        {
            base.Start();

            ExceptionUtilities.ThrowIfNullFormat(_renderer);
        }

        public void Init(NPCCock cock, NPCPeckPoint peckPoint)
        {
            _cockLeader = cock;
            _cockLeader.changePeckPoint += OnChangePeckPoint;
            _currentPeckPoint = peckPoint;

            _isAlive = true;

            _renderer.material.color = SetRandomColor();
            _peckCoroutine = StartCoroutine(PeckRoutine());
        }

        private void OnChangePeckPoint(NPCPeckPoint newPeckPoint)
        {
            _newPeckPoint = newPeckPoint;
            _goToNewPeckPointCoroutine = StartCoroutine(GoToNewPeckPointRoutine(delayTime: DELAY_TIME_TO_CHANGE_PECK_POINT));
        }

        private Color SetRandomColor()
        {
            float random = Random.Range(0f, 1f);
            return Color.Lerp(From, To, random);
        }
    }
}