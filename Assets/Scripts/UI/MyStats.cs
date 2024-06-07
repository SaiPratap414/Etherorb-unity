using UnityEngine;
using TMPro;
using System.Linq;

public class MyStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI userName;
    [SerializeField] private TextMeshProUGUI userWallet;

    [SerializeField] private TextMeshProUGUI totalWinOrbs;
    [SerializeField] private TextMeshProUGUI totalLossOrbs;

    [SerializeField] private TextMeshProUGUI pnl; // differenece wins - loss...
    [SerializeField] private TextMeshProUGUI winRate;
    [SerializeField] private TextMeshProUGUI totalPlayedGames;

    [SerializeField] private TextMeshProUGUI mostValueableOrbs;
    [SerializeField] private TextMeshProUGUI totalWonVolume; // Equals to win matches...

    [SerializeField] private TextMeshProUGUI currencyWon; // Equals to win matches...
    [SerializeField] private TextMeshProUGUI currencyLoss; // Equals to loss matches...

    [SerializeField] private Transform roundsParent;
    [SerializeField] private RoundItem roundItem;

    [SerializeField] private GameObject noGamesPanel;

    private MatchHistory matchHistory;
    private bool isInitialized = false;

   public void Init()
    {
        if (isInitialized)
            return;

        isInitialized = true;
        matchHistory = PlayfabConnet.instance.matchHistories;
        userName.text = PlayfabConnet.instance.PlayerName;
        userWallet.text = EtherOrbManager.Instance.WarningPanel.GetUserWalletAddress();

        winRate.text = $"{matchHistory.winRate.ToString("N1")}%";
        totalPlayedGames.text = matchHistory.totalGamesPlayed.ToString();

        totalWinOrbs.text = matchHistory.totalWinMatches.ToString();
        totalLossOrbs.text = matchHistory.totatLossMatches.ToString();

        pnl.text = (matchHistory.totalWinMatches - matchHistory.totatLossMatches).ToString();
        totalWonVolume.text = matchHistory.totalWinMatches.ToString();

        currencyWon.text = $"+{matchHistory.totalWinMatches} $ORBS";
        currencyLoss.text = $"-{matchHistory.totatLossMatches} $ORBS";

        matchHistory.userMatchHistories = matchHistory.userMatchHistories.OrderByDescending(x => x.timestamp).ToList();

        foreach (UserMatchHistory matchHistory in matchHistory.userMatchHistories)
        {
            RoundItem item = Instantiate(roundItem, roundsParent);
            item.Init(matchHistory);
        }
        noGamesPanel.SetActive(matchHistory.userMatchHistories.Count == 0);
    }
}
