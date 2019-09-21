using UnityEngine;
using UnityEngine.UI;

public class UI_CanvasManager : MonoBehaviour
{

    public static UI_CanvasManager instance;
    public Text scorePoints;
    public Text highScorePoints;
    public GameObject[] lifesIcons;
    public GameObject[] cherrysIcons;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        foreach (GameObject cherrys in cherrysIcons)
        {
            cherrys.SetActive(false);
        }
    }

    public void UpdateScore(uint _score)
    {
        scorePoints.text = _score.ToString("0000");
    }

    public void ShowOneMoreCherry()
    {
        for (int i = 0; i < cherrysIcons.Length; i++)
        {
            if (!cherrysIcons[i].activeSelf)
            {
                cherrysIcons[i].SetActive(true);
                return;
            }
        }
    }

    public void RemoveOneLifeIcon()
    {
        for (int i = 0; i < lifesIcons.Length; i++)
        {
            if (lifesIcons[i].activeSelf)
            {
                lifesIcons[i].SetActive(false);
                return;
            }
        }
    }
}