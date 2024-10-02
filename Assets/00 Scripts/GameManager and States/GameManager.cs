using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace NineMensMorris
{
    public class GameManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] BoardManager boardManager;
        [SerializeField] BatchAnimator batchAnim;
        [SerializeField] InfoTextController infoText;
        [SerializeField] CameraSizeController cameraSizeController;

        [Header("Player Data")]
        [SerializeField] PlayerData playerOne;
        [SerializeField] PlayerData playerTwo;

        [Header("Player Monos")]
        [SerializeField] PlayerTokensManager playerTokenManagerPrefab;
        [SerializeField] PlayerInfoDisplayer playerOneInfo;
        [SerializeField] PlayerInfoDisplayer playerTwoInfo;

        [Header("Board and Rules")]
        [SerializeField] LevelDataContainer levelDataContainer;

        [Header("Information SFX")]
        [SerializeField] AudioClip winningSound;
        [SerializeField] AudioClip millCompletionSound;
        [SerializeField] AudioClip illegalMoveSound;

        [Header("Unity Events")]
        [SerializeField] UnityEvent<PlayerData> OnPlayerWin;
        [SerializeField] UnityEvent OnDrawGame;

        List<HashSet<Node>> currentMills = new();
        List<HashSet<Node>> newMills = new();

        List<GamePhaseBase> gamePhases = new();
        GamePhaseBase currentPhase;

        //PROPERTIES, used by the state machine
        public LevelData LevelData => levelDataContainer.CurrentLevel;
        public BoardManager Board => boardManager;
        public BatchAnimator BatchAnim => batchAnim;
        public CameraSizeController CamSize => cameraSizeController;
        public InfoTextController InfoText => infoText;
        public PlayerData CurrentPlayer { get; private set; }
        public PlayerData EnemyPlayer => CurrentPlayer == playerOne ? playerTwo : playerOne;
        
        private void Start()
        {
            PopulateGamePhases();
            ChangeState(PhaseName.Load);
        }

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

        
        public void HandleNodeClicked(Node node)
        {
            currentPhase.EvaluateNodeClicked(node);
        }


        #region Setup Methods

        public void SetupPlayerInfos()
        {
            playerOneInfo.SetupPlayer(playerOne);
            playerTwoInfo.SetupPlayer(playerTwo);
        }

        public void SetupTokenManagers(float offset)
        {
            Vector3 p1Pos = (Vector3.left * offset) + transform.position;
            var p1Tokens = Instantiate(playerTokenManagerPrefab, p1Pos, transform.rotation, transform);
            p1Tokens.SetupPlayerData(playerOne);

            Vector3 p2Pos = (Vector3.right * offset) + transform.position;
            var p2Tokens = Instantiate(playerTokenManagerPrefab, p2Pos, transform.rotation, transform);
            p2Tokens.SetupPlayerData(playerTwo);
        }

        public void InstantiateNewTokens()
        {
            playerOne.TokenManager.InstantiateNewTokens(LevelData.TokensPerPlayer);
            playerTwo.TokenManager.InstantiateNewTokens(LevelData.TokensPerPlayer);
        }

        #endregion


        public void SwitchCurrentPlayer()
        {
            CurrentPlayer = CurrentPlayer == playerOne ? playerTwo : playerOne;
        }

        public bool NodeIsInMill(Node node)
        {
            foreach (HashSet<Node> mill in currentMills)
            {
                if (mill.Any(n => n == node))
                {
                    return true;
                }
            }
            return false;
        }

        public HashSet<Node> GetAllNodesInMillsForPlayer(PlayerData targetPlayer)
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

        public void UpdateMillsAndGetNewMills(Node tokenStartNode, Node tokenEndNode, PlayerData player)
        {
            List<HashSet<Node>> newMills = boardManager.GetNewMills(tokenEndNode, player, LevelData.NumOfTokensForMill);

            UpdateMillsForRemovedToken(tokenStartNode);

            currentMills.AddRange(newMills);
            this.newMills = newMills;
        }

        public List<Node> GetNodesInNewMills()
        {
            List<Node> result = new();
            foreach (HashSet<Node> mill in newMills)
            {
                foreach (Node node in mill) result.Add(node);
            }
            return result;
        }

        public void ClearNewMills()
        {
            newMills.Clear();
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

        public void TriggerMillFXs(List<Node> newMillNodes)
        {
            if (newMillNodes.Count == 0) return;

            SFXManager.Play(millCompletionSound, transform);
            BatchAnim.AnimateNewMillNodes(newMillNodes);
        }

        public void TriggerInvalidNodeFX(Node node)
        {
            SFXManager.Play(illegalMoveSound, transform);
            node.Mono.Shake();
        }

        public void TriggerCurrentPlayerWins()
        {
            SFXManager.Play(winningSound, transform);
            OnPlayerWin.Invoke(CurrentPlayer);
        }

        public void TriggerDrawGame()
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
}



