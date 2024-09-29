using NineMensMorris;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Required Managers")]
    [SerializeField] BoardManager boardManager;

    [Header("Player Data and Supply")]
    [SerializeField] PlayerData playerOne;
    [SerializeField] PlayerData playerTwo;

    [Header("Board and Rules")]
    [SerializeField] BoardAndRulesData boardAndRules;

    [Header("Unity Events")]
    [SerializeField] UnityEvent<PlayerData> OnPlayerWin;
    [SerializeField] UnityEvent OnDrawGame;

    List<HashSet<Node>> currentMills = new();


    //PROPERTIES, used by the state machine
    public BoardAndRulesData BRData => boardAndRules;
    public BoardManager Board => boardManager;
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

    public void SetupTokenManagers()
    {
        playerOne.TokenManager.InstantiateTokens(boardAndRules.TokensPerPlayer);
        playerTwo.TokenManager.InstantiateTokens(boardAndRules.TokensPerPlayer);
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
        List<HashSet<Node>> newMills = boardManager.GetNewMills(tokenEndNode, player, boardAndRules.NumOfTokensForMill);

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
        if (targetPlayer.TokenManager.LivingTokensCount <= boardAndRules.MaxTokensForFlying
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

    public void CurrentPlayerWins()
    {
        OnPlayerWin.Invoke(CurrentPlayer);
    }

    public void DrawGame()
    {
        OnDrawGame.Invoke();
    }
}

