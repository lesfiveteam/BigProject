using BigProject.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class InventoryItemNoteUI : InventoryItemUI
{
    [SerializeField] Button _button;
    [SerializeField] GameObject _noteObject;

    private bool isNoteOpened;

    private void OnEnable()
    {
        _button.onClick.AddListener(OpenCloseNote()); 
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OpenCloseNote());
    }

    private UnityAction OpenCloseNote()
    {
        return () => 
        { 
            isNoteOpened = !isNoteOpened;
            _noteObject.SetActive(isNoteOpened);
        };
    }
}