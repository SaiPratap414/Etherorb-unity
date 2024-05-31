using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoundItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI date;
    [SerializeField] private TextMeshProUGUI time;

    [SerializeField] private TextMeshProUGUI winStatus;
    [SerializeField] private TextMeshProUGUI roundScore;

    [SerializeField] private TextMeshProUGUI reward;
    [SerializeField] private TextMeshProUGUI orbId;

    [SerializeField] private Image orbImage;
    [SerializeField] private Color winColor;
    [SerializeField] private Color lossColor;

    public void Init(UserMatchHistory userMatchHistory)
    {
        Debug.Log(userMatchHistory.timestamp);
        date.text = userMatchHistory.GetDate();
        time.text = userMatchHistory.GetTime();
        orbId.text = "#1122";

        winStatus.text = userMatchHistory.GetMatchStatus();
        roundScore.text = userMatchHistory.matchScore;
        reward.color = userMatchHistory.GetMatchColor();
        reward.text =  userMatchHistory.reward.ToString() + "$ORB";
    }
}
