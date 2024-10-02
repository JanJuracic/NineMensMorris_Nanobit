using NineMensMorris;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Required Managers")]
    [SerializeField] BoardManager boardManager;
    [SerializeField] BatchAnimator batchAnim;
    [SerializeField] InfoTextController infoText;
    [SerializeField] CameraSizeController cameraSizeController;

    [Header("Prefabs")]
    [SerializeField] PlayerTokensManager playerTokenManager;

    [Header("Player Data and Supply")]
    [SerializeField] PlayerData playerOne;
    [SerializeField] PlayerData playerTwo;

    [Header("Board and Rules")]
    [SerializeField] LevelDataContainer levelDataContainer;

    [Header("Unity Events")]
    [SerializeField] UnityEvent<PlayerData> OnPlayerWin;
    [SerializeField] UnityEvent OnDrawGame;

    List<HashSet<Node>> currentMills = new();

    //PROPERTIES, used by the state machine
    public LevelData LevelData => levelDataContainer.CurrentLevel;
    public BoardManager Board => boardManager;
    public BatchAnimator BatchAnim => batchAnim;
    public CameraSizeController CamSize => cameraSizeController;
    public InfoTextController InfoText => infoText;
    public PlayerData CurrentPlayer { get; private set; }
    public PlayerData EnemyPlayer => CurrentPlayer == playerOne ? playerTwo : playerOne;
    public int NewMillsCreatedLastAction { get; set; }


    List<GamePhaseBase> gamePhases = new();
    GamePhaseBase currentPhase;

    private void PopulateGamePhases()
    {
        gamePhases = new()
        {
            new LoadLevel(this),
            new SwitchCurrentPlayer(this),
            new AddTokenFromSupply(this),
            new MoveTokenOnBoard(this),
            new EvaluatePlayerMove(this),
            new DestroyToken(this),
            new WinGame(this),
            new DrawGame(this),
        };
    }

    public void ChangeState(PhaseName name)
    {
        if (currentPhase != null)
        {
            currentPhase.Exit();
        }

        currentPhase = gamePhases.First(phase => phase.Name == name);
        currentPhase.Enter();
    }

    private void Start()
    {
        PopulateGamePhases();
        ChangeState(PhaseName.Load);
    }

    public void HandleNodeClicked(Node node)
    {
        currentPhase.EvaluateNodeClicked(node);
    }

    public void SwitchCurrentPlayer()
    {
        CurrentPlayer = CurrentPlayer == playerOne ? playerTwo : playerOne;
    }

    public void SetupTokenManagers(float offset)
    {
        Vector3 p1Pos = (Vector3.left * offset) + transform.position;
        var p1Tokens = Instantiate(playerTokenManager, p1Pos, transform.rotation, transform);
        p1Tokens.SetupPlayerData(playerOne);

        Vector3 p2Pos = (Vector3.right * offset) + transform.position;
        var p2Tokens = Instantiate(playerTokenManager, p2Pos, transform.rotation, transform);
        p2Tokens.SetupPlayerData(playerTwo);
    }

    public void InstantiateNewTokens()
    {
        playerOne.TokenManager.InstantiateNewTokens(LevelData.TokensPerPlayer);
        playerTwo.TokenManager.InstantiateNewTokens(LevelData.TokensPerPlayer);
    }

    public HashSet<Node> GetAllNodesInMills(PlayerData targetPlayer)
    {
        HashSet<Node> result = new();
        foreach (HashSet<Node> mill in currentMills)
        {
            if (mill.Any(n => n.Token.Player == targetPlayer))
            {
                result.AddRange(mill);
            }
        }
        return result;
    }

    public void UpdateMillsAndSetNewMillCount(Node tokenStartNode, Node tokenEndNode, PlayerData player)
    {
        List<HashSet<Node>> newMills = boardManager.GetNewMills(tokenEndNode, player, LevelData.NumOfTokensForMill);

        UpdateMillsForRemovedToken(tokenStartNode);

        currentMills.AddRange(newMills);

        NewMillsCreatedLastAction = newMills.Count;
    }

    public void UpdateMillsForRemovedToken(Node nodeWithRemovedToken)
    {
        //Remove any mills that are no longer valid, due to a moved token
        if (nodeWithRemovedToken != null)
        {
            for (int i = currentMills.Count - 1; 0 < i; i--)
            {
                HashSet<Node> mill = currentMills[i];
                if (mill.Contains(nodeWithRemovedToken))
                {
                    currentMills.RemoveAt(i);
                }
            }
        }
    }

    public bool PlayerHasLegalMoves(PlayerData targetPlayer)
    {
        //If the player can fly, or still has tokens in supply
        if (targetPlayer.TokenManager.LivingTokensCount <= LevelData.MaxTokensForFlying
            || targetPlayer.TokenManager.TokensInSupplyCount > 0)
        {
            List<Node> emptyNodes = Board
                .GetAllNodes()
                .Where(n => n.Token == null)
                .ToList();

            if (emptyNodes.Count == 0 ) return false;
            else return true;
        }
        else
        {
            List<Node> playerTokenNodes = Board.GetAllPlayerTokenNodes(targetPlayer);

            foreach (Node node in playerTokenNodes)
            {
                foreach (Vector3Int edge in node.EdgeDirections)
                {
                    bool hasEmptyConnectingNodes = Board
                        .GetConnectingNodes(node)
                        .Any(n => n.Token == null);

                    if (hasEmptyConnectingNodes) return true;
                }
            }
            return false;
        }
    }

    [ContextMenu("Force Win")]
    public void CurrentPlayerWins()
    {
        OnPlayerWin.Invoke(CurrentPlayer);
    }

    [ContextMenu("Force Draw")]
    public void DrawGame()
    {
        OnDrawGame.Invoke();
    }

    #region Button Interaction

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("GameScene");
    }


    #endregion
}

