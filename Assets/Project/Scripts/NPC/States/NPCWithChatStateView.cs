using BigProject.NPC.States;
using BigProject.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class NPCWithChatStateView : NPCStateView
{
    [SerializeField]
    private Canvas _chatPanel;
    [SerializeField]
    private Image _chatBoard;
    [SerializeField]
    private TMP_Text _chatText;

    protected override void OnAwake()
    {
        Assert.IsNotNull(_chatPanel, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Chat Canvas"));
        Assert.IsNotNull(_chatBoard, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Chat Image"));
        Assert.IsNotNull(_chatText, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Chat Text"));
    }

    protected override void PrepareNewState(INPCState currentState)
    {
        switch (currentState.State)
        {
            case NPCState.Chat:
                if (currentState is NPCStateChat stateChat)
                {
                    stateChat.Speak += OnSpeak;
                }

                _chatPanel.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    protected override void ClearOldState(INPCState currentState)
    {
        switch (currentState.State)
        {
            case NPCState.Chat:
                if (currentState is NPCStateChat stateChat)
                {
                    OnSpeak("");
                    stateChat.Speak -= OnSpeak;
                }

                _chatPanel.gameObject.SetActive(false);
                break;
            default:
                break;

        }
    }

    private void OnSpeak(string text)
    {
        _chatText.text = text;
        _chatBoard.enabled = !string.IsNullOrEmpty(_chatText.text);
    }
}