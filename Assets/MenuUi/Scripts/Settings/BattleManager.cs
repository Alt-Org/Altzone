using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public GameObject warningPanel;
    public Text warningText;

    private bool battleActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        warningPanel.SetActive(false);
    }

    public void StartBattle()
    {
        if (GameTimeManager.Instance.IsTimeExpired())
        {
            ShowWarning("Time is up, come back tomorrow!");
        }
        else
        {
            battleActive = true;
        }
    }

    public void EndBattle()
    {
        battleActive = false;
        if (GameTimeManager.Instance.IsTimeExpired())
        {
            GameTimeManager.Instance.ReturnToMainMenu();
        }
    }

    public void ShowTimeWarning()
    {
        if (battleActive)
        {
            ShowWarning("Time limit reached!");
        }
        else
        {
            GameTimeManager.Instance.ReturnToMainMenu();
        }
    }

    private void ShowWarning(string message)
    {
        warningText.text = message;
        warningPanel.SetActive(true);
    }

    public void TryPlayAgain()
    {
        if (GameTimeManager.Instance.IsTimeExpired())
        {
            ShowWarning("Time is up, come back tomorrow!");
        }
    }
}
