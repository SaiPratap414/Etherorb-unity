using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenManager : MonoBehaviour
{

    [SerializeField] private Button myOrbButton;
    [SerializeField] private Button myStatsButton;
    [SerializeField] private Button leaderboardButton;

    private TextMeshProUGUI myStatText;
    private TextMeshProUGUI myOrbText;
    private TextMeshProUGUI leaderboardText;

    [SerializeField] private MyStats myStats;
    [SerializeField] private LeaderBoard leaderBoard;
    [SerializeField] private GameObject myPanel;

    [SerializeField] private Color deSelectedColor;


    private void Awake()
    {
        myStatText = myStatsButton.GetComponent<TextMeshProUGUI>();
        myOrbText = myOrbButton.GetComponent<TextMeshProUGUI>();
        leaderboardText = leaderboardButton.GetComponent<TextMeshProUGUI>();

        myStatText.fontStyle = FontStyles.Underline;

        myOrbButton.onClick.AddListener(ShowMyOrbScreen);
        myStatsButton.onClick.AddListener(ShowMyStatsScreen);
        leaderboardButton.onClick.AddListener(ShowLeaderBoard);

        ShowMyOrbScreen();
    }

    private void ShowMyOrbScreen()
    {
        myStats.gameObject.SetActive(false);
        DeSelectButtonAndText();
        myOrbText.fontStyle = FontStyles.UpperCase;

        myOrbText.color = Color.white;
        myPanel.SetActive(true);

        myStatsButton.enabled = true;
        leaderboardButton.enabled = true;
    }
    private void ShowMyStatsScreen()
    {
        myStats.Init();
        myPanel.SetActive(false);
        DeSelectButtonAndText();

        myStatText.fontStyle = FontStyles.UpperCase;
        myStatText.color = Color.white;
        myStats.gameObject.SetActive(true);

        myOrbButton.enabled = true;
        leaderboardButton.enabled = true;
    }
    private void ShowLeaderBoard()
    {
        //leaderBoard.Init();
        myPanel.SetActive(false);
        DeSelectButtonAndText();

        leaderboardText.fontStyle = FontStyles.UpperCase;
        leaderboardText.color = Color.white;
        leaderBoard.gameObject.SetActive(true);

        myOrbButton.enabled = true;
        myStatsButton.enabled = true;
    }


    private void DeSelectButtonAndText()
    {

        myStatText.fontStyle = FontStyles.Underline;
        myOrbText.fontStyle = FontStyles.Underline;
        leaderboardText.fontStyle = FontStyles.Underline;

        myStatText.color = deSelectedColor;
        myOrbText.color = deSelectedColor;
        leaderboardText.color = deSelectedColor;

        myOrbButton.enabled = false;
        myStatsButton.enabled = false;
        leaderboardButton.enabled = false;

        myPanel.SetActive(false);
        myStats.gameObject.SetActive(false);
        leaderBoard.gameObject.SetActive(false);

    }
}
