using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using TMPro;

public class EventChecker : MonoBehaviour {
    public static EventChecker eventChecker;

    void Awake(){
        // 씬이 바뀔 때 파괴되지 않음
        DontDestroyOnLoad(this.gameObject);

        if(eventChecker == null){
            eventChecker = this;
        }
    }

    // private List<ChatMessage> messages = new List<ChatMessage>();

    public async Task<bool> EventCheckerGPT(List<ChatMessage> play_messages, string event_trigger) {
        Debug.Log(">>Call Event Checker GPT");

        // 시스템 프롬프팅 추가
        var newMessage = new ChatMessage()
        {
            Role = "system",
            Content = @"당신은 게임의 플레이 데이터를 분석하여 이벤트 발생 여부를 판단하는 Checker이다. 
            대답은 반드시 T 혹은 F로 해야한다. 이벤트는 user의 행동지문이 " + event_trigger + "와 같을 때 발생한다."
        };
        play_messages.Insert(0, newMessage);

        var response = await GptManager.gpt.CallGpt(play_messages);
        Debug.Log(response);

        if(response == "T") {
            return true;
        }

        // for(int i = 0; i < play_messages.Count; i++) {
        //     if(play_messages[i].Content.Contains(event_trigger)) {
        //         return true;
        //     }
        // }

        return false; // 임시 조정
    }
}