using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using Photon.Pun;

public class WarningPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private GameObject blocker;

    [SerializeField] private RectTransform warningRectTransform;
    [SerializeField] private CanvasGroup warningCanvasGroup;

    [SerializeField] private RectTransform popupRectTransform;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [SerializeField] private RoundTimer timer;

    [SerializeField] private Sprite soundOn;
    [SerializeField] private Sprite soundOff;
    [SerializeField] private Sprite musicOn;
    [SerializeField] private Sprite musicOff;

    [SerializeField] private Image soundImage;
    [SerializeField] private Image musicImage;

    [SerializeField] private Toggle sound;
    [SerializeField] private Toggle music;

    [SerializeField] private TextMeshProUGUI userWalletAddressText;

    private string userWalletAddress;

    private Vector3 initialPosition;

    private float warningShowFactor = 160f;
    private float popUPShowFactor = 250;
    private const float duration = 1f;

    private bool isWarningShown = false;

    private Color finalColor;

    private EventManager eventManager;

    private void Awake()
    {
        initialPosition = warningRectTransform.anchoredPosition3D;
        finalColor.a = 0;
        DontDestroyOnLoad(this);

        yesButton.onClick.AddListener(()=> { OnButtonAction(true); });
        noButton.onClick.AddListener(() => { OnButtonAction(false); });

        eventManager = EventManager.Instance;
        eventManager.OnRematchTimerCompleted += RematchCancled;
    }
    private void OnDestroy()
    {
        eventManager.OnRematchTimerCompleted -= RematchCancled;
    }

    public void ShowWarning(string msg, bool shouldStay=false)
    {
        if (!isWarningShown)
        {
            textMeshProUGUI.text = msg;
            isWarningShown = true;
            warningRectTransform.DOMoveY((initialPosition.y + warningShowFactor), 0.7f).SetEase(Ease.InOutElastic).OnComplete(() =>
            {

                if (!shouldStay)
                {
                    HideWarning();
                }
            });
        }
    }

    public void HideWarning()
    {
        float alpha = 1;
        DOTween.To(() => warningCanvasGroup.alpha, x => alpha = x, 1, duration)
            .OnComplete(() =>
            {
                warningCanvasGroup.alpha = 1;
                isWarningShown = false;
                warningRectTransform.anchoredPosition3D = initialPosition;
            });
    }

    public void ShowPopup()
    {
        blocker.SetActive(true);
        timer.setRoundBool(true);
        popupRectTransform.DOMoveY((initialPosition.y + popUPShowFactor), 0.7f).SetEase(Ease.InOutElastic);        
    }
    public void HidePopUp()
    {
        blocker.SetActive(false);
        timer.setRoundBool(false);
        popupRectTransform.DOMoveY((initialPosition.y - popUPShowFactor), 0f).SetEase(Ease.InOutElastic);
    }

    private void OnButtonAction(bool show)
    {
        HidePopUp();
        if (show)
        {
            GameManager.instance.ReMatchAcceptance();
        }
        else
        {
            RematchCancled();
        }
    }

    private void RematchCancled()
    {
        HidePopUp();
        GameManager.instance.ReMatchCancleRequest(PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void OnMusicToggleValueChange()
    {
        musicImage.sprite = music.isOn ? musicOn : musicOff;
        if(music.isOn)
            EtherOrbManager.Instance.AudioManager.PlayMusic();
        else
            EtherOrbManager.Instance.AudioManager.StopMusic();
    }
    public void OnSoundToggleValueChange()
    {
        soundImage.sprite = sound.isOn ? soundOn : soundOff;
        if (sound.isOn)
            EtherOrbManager.Instance.AudioManager.PlaySfx();
        else
            EtherOrbManager.Instance.AudioManager.StopSfxSound();
    }

    public void SetUserWallet(string walletAddress)
    {
        userWalletAddress = walletAddress;
        userWalletAddressText.text = userWalletAddress;
    }
    public string GetUserWalletAddress()
    {
        return userWalletAddress;
    }
}
