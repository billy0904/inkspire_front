using System.Collections;
using System.Collections.Generic;
using OpenAI;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private ScriptManager scriptManager;
    public static MapManager mapinfo;
    public struct place
    {
        public string place_name;
        public string place_info; //장소 설명
        public string item_name;
        public string item_type; //recover, weapon, mob, null (목표이벤트일 경우 report 추가)
        public bool event_type; //일반 이벤트 == 0, 목표 이벤트 == 1 
        public bool ANPC_exist; //ANPC 미등장 == 0, 등장 == 1 (목표이벤트일 경우 무조건 0)
    }
    public place[] map = new place[14];
    public string PNPC_place;
    public int curr_place;

    private OpenAIApi openai = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();
    private ChatMessage input_msg = new ChatMessage();
    private string system_prompt = "너는 플레이어가 탐색할 수 있는 장소를 한 단어로 출력해야 해.";
    //background + " 배경의 " + genre + "분위기에 어울리는 장소명 1개를 출력해줘";

    // def getProtaNPCName(background, genre):
    // npc_setting = "너는 조력자 NPC 캐릭터의 이름을 한 단어로 출력해야 해."
    // query = background + " 배경의 " + genre + "분위기에 어울리는 조력자 NPC 이름 1개를 출력해줘"

    // messages = [
    //     {"role": "system", "content": npc_setting},
    //     {"role": "user", "content": query}
    // ]

    // PNPC_name = callGPT(messages=messages, stream=False)

    // return PNPC_name

    void Awake()
    {
        // 씬이 바뀔 때 파괴되지 않음
        DontDestroyOnLoad(this.gameObject);

        if (mapinfo == null)
        {
            mapinfo = this;
        }
    }

    public void setBackground(string time, string space, string gen)
    {
        time_background = time;
        space_background = space;
        genre = gen;
        CreatePlace();
    }

    private async void CreateItem()
    {

    }
    private async void CreatePlace(int place_idx)
    {
        Debug.Log(">>Call Create Place GPT");
        Debug.Log(">>현재 장소 인덱스: " + place_idx);
        gpt_messages.Clear();

        if (place_idx == 0)
        {
            var prompt_msg = new ChatMessage()
            {
                Role = "system",
                Content = @"당신은 조력자 NPC가 머무는 장소를 제시한다.
            다음은 게임의 배경인 
            " + time_background + "시대" + space_background + "를 배경으로 하는 세계관에 대한 설명이다." + world_detail +
                @"장소는 해당 게임의 조력자 NPC의 집 혹은 직장으로 생성되며 조력자 NPC의 정보는 다음과 같다." + PNPC_info + //이거 변수 어디서 가져오는지 확인
                @"장소 생성 양식은 아래와 같다. 각 줄의 요소는 반드시 모두 포함되어야 하며, 답변할 때 줄바꿈을 절대 하지 않는다. ** 이 표시 안의 내용은 문맥에 맞게 채운다.


            장소명: *장소 이름을 한 단어로 출력*
            장소설명: *장소에 대한 설명을 50자 내외로 설명 * "
            };
        }
        else
        {
            var prompt_msg = new ChatMessage()
            {
                Role = "system",
                Content = @"당신은 게임 진행에 필요한 장소를 제시한다.
            다음은 게임의 배경인 
            " + time_background + "시대" + space_background + "를 배경으로 하는 세계관에 대한 설명이다." + world_detail +
                @"장소는 게임의 배경에 맞추어 플레이어가 흥미롭게 탐색할 수 있는 곳으로 생성된다. 장소 생성 양식은 아래와 같다. 각 줄의 요소는 반드시 모두 포함되어야 하며, 답변할 때 줄바꿈을 절대 하지 않는다. ** 이 표시 안의 내용은 문맥에 맞게 채운다.
            
            장소명: *장소 이름을 한 단어로 출력*
            장소설명: *장소에 대한 설명을 50자 내외로 설명*"
            };
        }
        gpt_messages.Add(prompt_msg);

        var query_msg = new ChatMessage()
        {
            Role = "user",
            Content = "진행중인 게임의 " + genre + "장르와 세계관에 어울리는 장소 생성"
        };
        gpt_messages.Add(query_msg);

        // Complete the instruction
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-3.5-turbo",
            Messages = gpt_messages
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();
            Debug.Log(message.Content);
            chapter_obj[chapter_num + 1] = StringToObjective(message.Content);
            curr_chapter = chapter_num + 1;
            if (curr_chapter != 1)
            {
                //StartCoroutine(PostChapterObjective(curr_chapter));
            }

        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }


    }

    //장소 이름 및 장소 설명 파싱 함수
    place StringToPlace(string plc_string)
    {
        place plc = new place();
        plc.clear = false;

        string[] plc_arr;
        plc_string = plc_string.Replace("\n", ":");
        plc_arr = plc_string.Split(':');

        plc.place_name = plc_arr[1];
        plc.place_info = plc_arr[3];

        return plc;
    }
}
