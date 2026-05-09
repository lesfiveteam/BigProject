using Assets.Project.Scripts.NPC.Animals.Chicken;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField]
    private NPCChickenSpawner _NPCChickenSpawner;

    private void Start()
    {
        _NPCChickenSpawner.Init(ServiceLocator.GetService<SoundsManager>());
    }
}
