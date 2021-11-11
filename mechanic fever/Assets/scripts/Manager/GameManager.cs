using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    private CharacterSelecter characterSelecter;
    private InteractableSpawning spawner;

    [Header("The Amount of players in game")]
    [Space(10)]
    public int playerAmount;
    [Header("The amount of money each player starts with")]
    [Space(10)]
    public int startingMoney;
    [Header("time per turn for each player")]
    [Space(10)]
    public float timePerTurn;

    [Header("cube size in which weapons and powerups spawn")]
    [Space(10)]
    public Vector2 fieldSize;


    public Player[] players;
    public Player winningPlayer;

    [HideInInspector]
    public bool controllingCamera = true;

    private bool timerDone = true;
    [HideInInspector]
    public bool turnTimerPaused;

    private float timer;

    public bool gameOver;

    public enum GameMode
    {
        setup,
        action
    }

    [HideInInspector]
    public GameMode currentGameMode;

    private int turn = 0;

    public static float Map(float value, float valueMin, float valueMax, float resultMin, float resultMax)
    {
        if (resultMin == resultMax) return resultMin;
        if (valueMin == valueMax) return resultMax;
        return resultMin + (value - valueMin) * (resultMax - resultMin) / (valueMax - valueMin);
    }

    public void setCharacterSelector(CharacterSelecter selecter)
    {
        characterSelecter = selecter;
    }

    public void setItemSpawner(InteractableSpawning spawner)
    {
        this.spawner = spawner;
    }

    private void Awake()
    {
        if (gameManager is null)
        {
            gameManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }

        startGame();
    }

    private void startGame()
    {
        players = new Player[playerAmount];

        for (int i = 0; i < playerAmount; i++)
        {
            players[i] = new Player(startingMoney);
        }
    }

    public GameMode getGamemode()
    {
        return currentGameMode;
    }

    public Player GetPlayer()
    {
        return players[turn];
    }

    #region turn management
    public int getTurnIndex()
    {
        return turn;
    }

    private void TurnSystem()
    {
        if (!gameOver)
        {
            turn++;

            if (turn > players.Length - 1)
            {
                turn = 0;
            }

            if (currentGameMode == GameMode.setup)
            {

                if (Array.TrueForAll(players, n => n.getCurrency() <= 0))
                {
                    EndSetupFase();
                }
                else if (players[turn].getCurrency() <= 0)
                {
                    TurnSystem();
                }
            }
            else
            {
                if (players[turn].getUnitLenght() <= 0)
                {
                    TurnSystem();
                    return;
                }

                StartCoroutine(UiManager.uiManager.enableActionSceenOverTime(turn, 2));

                spawner.spawnPowerUp();
                spawner.spawnWeapon();
            }
        }
        else
        {
            UiManager.uiManager.disableAllActionScreens();
            print("victory");
        }
    }

    public void startTimer()
    {
        timerDone = false;
        timer = timePerTurn;
    }

    private void Update()
    {
        if (!turnTimerPaused && timer > 0)
        {
            UiManager.uiManager.updateUnitControlTimer(timer);
            timer -= Time.deltaTime;
        }
        else if (timer <= 0 && !timerDone)
        {
            timerDone = true;
            StartCoroutine(characterSelecter.resetCamera());
            characterSelecter.selectedCharacter.GetComponent<CharacterController>().resetCharacter();
            EndTurn();
        }
    }
    #endregion

    #region turn and fase ending

    public void PlayerDoneSetupFase()
    {
        GetPlayer().zeroCurrency();
        TurnSystem();
    }

    public void EndSetupFase()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].setUnits(GameObject.FindGameObjectsWithTag($"player{i}Owned"));
        }

        currentGameMode = GameMode.action;
        UiManager.uiManager.showActionScreen(0);
        turn = 0;


        //call action fase banner
    }

    public void EndTurn()
    {
        TurnSystem();
    }

    public void endTurnEarly()
    {
        timer = 0;
    }
    #endregion

    #region Victory/draw Mechanic's
    public void unitDeath(GameObject unit, int playerIndex)
    {
        players[playerIndex].RemoveUnit(unit);

        if (XorPlayerValue())
        {
            gameOver = true;
        }
    }

    private bool XorPlayerValue()
    {
        int i = 0;
        foreach (Player player in players)
        {
            if (player.getUnitLenght() <= 0)
            {
                i++;
            }
        }

        return i == players.Length - 1;
    }

    private Player getWiningPlayer()
    {
        Player value = null;

        foreach (Player player in players)
        {
            if (player.getUnitLenght() > 0)
            {
                value = player;
            }
        }

        if (value == null)
        {
            Debug.LogError($"{this}: the winning player returned null");
        }


        return value;
    }

    public void EndGame()
    {
        gameOver = true;
    }
    #endregion
}