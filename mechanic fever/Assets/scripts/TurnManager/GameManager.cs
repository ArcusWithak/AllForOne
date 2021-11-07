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


    private Player[] players;

    [HideInInspector]
    public bool controllingCamera = true;

    private bool timerDone = true;
    [HideInInspector]
    public bool turnTimerPaused;
    
    private float timer;

    private bool gameOver;

    private bool endTurn;

    public enum GameMode
    {
        setup,
        action
    }

    [HideInInspector]
    public GameMode currentGameMode;

    private int turn = 0;

    private void Awake()
    {
        if (gameManager is null)
        {
            gameManager = this;
        }
        else
        {
            Destroy(this);
        }

        characterSelecter = GetComponent<CharacterSelecter>();
        spawner = GetComponent<InteractableSpawning>();

        startGame();
    }

    private void startGame()
    {
        players = new Player[playerAmount];

        for (int i = 0; i < playerAmount; i++)
        {
            players[i] = new Player(startingMoney);
        }

        StartCoroutine(TurnSystem());
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

    private IEnumerator TurnSystem()
    {
        while (!gameOver)
        {
            if (currentGameMode == GameMode.setup)
            {
                if (turn > players.Length - 1)
                {
                    turn = 0;
                }
                yield return new WaitUntil(() => endTurn || players[turn].getCurrency() <= 0);
                endTurn = false;
                turn++;

                if (Array.TrueForAll(players, n => n.getCurrency() <= 0))
                {
                    EndSetupFase();
                }
            }
            else
            {
                if (turn > players.Length - 1)
                {
                    turn = 0;
                }
                yield return new WaitUntil(() => endTurn);
                endTurn = false;
                spawner.spawnPowerUp();
                spawner.spawnWeapon();
                turn++;
            }
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
            timer -= Time.deltaTime;
        }
        else if (timer < 0 && !timerDone)
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
    }

    public void EndSetupFase()
    {
        StopAllCoroutines();
        currentGameMode = GameMode.action;
        turn = 0;
        StartCoroutine(TurnSystem());

        //call action fase banner
    }

    public void EndTurn()
    {
        endTurn = true;
    }

    public void EndGame()
    {
        gameOver = true;
    }
    #endregion
}