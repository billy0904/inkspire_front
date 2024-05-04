using System.Collections;
using System.Collections.Generic;
using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


public class DiceEvent : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject dicewindow;
    [SerializeField] GameObject resultwindow;
    [SerializeField] TextMeshProUGUI result_calc;
    [SerializeField] TextMeshProUGUI result_txt;
    [SerializeField] TextMeshProUGUI tens_dice;
    [SerializeField] TextMeshProUGUI ones_dice;

    [SerializeField] Play play_manager;

    int req_value = 0;
    bool result = false;
    int pl_value = 0;
    int luk_value = 0;

    public void SetDiceEvent(int val)
    {
        dicewindow.gameObject.SetActive(true);

        resultwindow.gameObject.SetActive(false);
        pl_value = 0;
        tens_dice.text = "00";
        ones_dice.text = "00";

        req_value = val;
        Debug.Log(">>set dice event call");
    }

    public void RollDice()
    {
        Debug.Log(">>roll dice event call");
        int ones = Random.Range(0, 10);
        int tens = Random.Range(0, 10);

        ones_dice.text = ones.ToString();
        tens_dice.text = tens.ToString();
        pl_value = tens * 10 + ones;
        //행운 스탯은 모든 판정에 int(rand(0,행운)*0.5)만큼의 보정치를 더해준다.
        luk_value = Random.Range(0, PlayerStatManager.playerstat.p_stats.GetStatAmount(StatType.Luck) / 2);

        Invoke("ResultActive", 0.5f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (resultwindow.activeSelf == false )
        {
            return;
        }
        dicewindow.gameObject.SetActive(false);
    }

    void ResultActive()
    {
        int curr_place = ScriptManager.script_manager.GetCurrPlaceIdx();
        resultwindow.gameObject.SetActive(true);
        result_calc.text = pl_value.ToString() + " + " + luk_value.ToString() + "(Bonus)\n";
        ChatMessage result_msg = new ChatMessage(){
            Role = "user",
            Content = "주사위 판정 "
        };
        if (pl_value + luk_value >= req_value)
        {
            result_txt.text = "<color=#074AB0>Success</color>";
            result_msg.Content += "성공";
            Item map_item = ScriptManager.script_manager.GetCurrItem();
            Debug.Log("맵 아이템 이름:"+map_item.item_name);
            InventoryManager.i_manager.AddItem(map_item);
            ScriptManager.script_manager.SetPlaceClear(true);
            play_manager.AddToMessagesGPT(result_msg);
            result_msg.Content = ScriptManager.script_manager.GetCurrEvent().event_succ.Replace(".", ".\n");
        }
        else
        {
            result_txt.text = "<color=#B40000>Fail</color>";
            result_msg.Content += "실패";
            play_manager.AddToMessagesGPT(result_msg);
            result_msg.Content = ScriptManager.script_manager.GetCurrEvent().event_fail.Replace(".", ".\n");
        }
        result_msg.Role = "assistant";
        play_manager.AddToMessagesGPT(result_msg);
        play_manager.text_scroll.AppendMsg(result_msg);
    }
}