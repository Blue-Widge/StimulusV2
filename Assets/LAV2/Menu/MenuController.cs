using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Levels To Load")]
    public string mineCart;
    public string raceGame;

    public void MineCartDialogStart()
    {
        SceneManager.LoadScene(mineCart);
    }

    public void RaceGameDialogStart()
    {
        SceneManager.LoadScene(raceGame);
    }

    public void ExitButton()
    {
        Application.Quit();
    }
}
