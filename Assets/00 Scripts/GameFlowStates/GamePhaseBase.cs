using NineMensMorris;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NineMensMorris
{
    public enum PhaseName
    {
        Load,
        SwitchPlayer,
        AddTokenFromSupply,
        MoveTokenOnBoard,
        EvaluatePlayerMove,
        DestroyToken,
        WinGame,
        DrawGame,
    }

    public abstract class GamePhaseBase
    {
        protected GameManager gm;
        protected PhaseName name;

        public PhaseName Name => name;

        protected GamePhaseBase(GameManager gameManager)
        {
            gm = gameManager;
        }

        public void ChangeState(PhaseName nextState)
        {
            Debug.Log(nextState);
            gm.ChangeState(nextState);
        }

        public virtual void Enter() { }

        public virtual void EvaluateNodeClicked(Node node) { }

        public virtual void Exit() { }
    }

    public class LoadLevel : GamePhaseBase
    {
        public LoadLevel(GameManager gameManager) : base(gameManager) 
        {
            name = PhaseName.Load;
        }

        public override void Enter()
        {
            gm.Board.SetupBoard(); //TODO: Don't hold creation data in Board, hold it in a SO
            gm.FillTokenSupplies();
        
            ChangeState(PhaseName.SwitchPlayer);
        }
    }

    public class SwitchCurrentPlayer : GamePhaseBase
    {
        public SwitchCurrentPlayer(GameManager gameManager) : base(gameManager) 
        {
            name = PhaseName.SwitchPlayer;
        }

        public override void Enter()
        {
            gm.SwitchCurrentPlayer();

            //If all of player's tokens are on the board
            if (gm.CurrentPlayer.TokenManager.TokensInSupplyCount == 0)
            {
                ChangeState(PhaseName.MoveTokenOnBoard);
            }
            else
            {
                ChangeState(PhaseName.AddTokenFromSupply);
            }
        }
    }

    public class AddTokenFromSupply : GamePhaseBase
    {
        public AddTokenFromSupply(GameManager gameManager) : base(gameManager)
        {
            name = PhaseName.AddTokenFromSupply;
        }

        public override void EvaluateNodeClicked(Node node)
        {
            if (node.Token == null)
            {
                gm.CurrentPlayer.TokenManager.SendTopTokenToNode(node);
                gm.LatestPlayerMove = new PlayerTokenMove(null, node, gm.CurrentPlayer);
                ChangeState(PhaseName.EvaluatePlayerMove);
            }
            else
            {
                //Node INVALID animation
            }
        }
    }

    public class MoveTokenOnBoard : GamePhaseBase
    {
        Node nodeWithFriendlyToken;
        List<Node> validTargetNodes;
        bool moveComplete;

        public MoveTokenOnBoard(GameManager gameManager) : base(gameManager)
        {
            name = PhaseName.MoveTokenOnBoard;
            nodeWithFriendlyToken = null;
            validTargetNodes = new();
            moveComplete = false;
        }

        public override void EvaluateNodeClicked(Node node)
        {
            //Select node with token
            if (nodeWithFriendlyToken == null)
            {
                AttemptSelectFriendlyTokenNode(node);
            }    
            else
            {
                if (node.Token != null)
                {
                    AttemptSelectFriendlyTokenNode(node);
                }
                else 
                {
                    AttemptMoveTokenToNewNode(node);
                }
            }

            if (moveComplete)
            {
                ChangeState(PhaseName.EvaluatePlayerMove);
            }
        }

        public override void Exit()
        {
            nodeWithFriendlyToken = null;
            validTargetNodes.Clear();
            moveComplete = false;
        }

        private void AttemptSelectFriendlyTokenNode(Node selectedNode)
        {
            if (NodeContainsFriendlyToken(selectedNode) == false)
            {
                //TODO: node invalid animation
                return;
            }

            nodeWithFriendlyToken = selectedNode;
            //TODO: friendly node selected animation

            validTargetNodes.Clear();
            //If player's tokens can fly
            if (gm.CurrentPlayer.TokenManager.LivingTokensCount <= gm.MaxTokensForFlying)
            {
                var allNodes = gm.Board.GetAllNodes();
                foreach (var node in allNodes)
                {
                    if (node.Token == null) validTargetNodes.Add(node);
                }
            }
            else
            {
                var connectingNodes = gm.Board.GetConnectingNodes(selectedNode);
                foreach (var node in connectingNodes)
                {
                    if (node.Token == null) validTargetNodes.Add(node);
                }
            }

            Debug.Log($"Connecting Nodes: {validTargetNodes.Count}");
            //TODO: Animate validTargetNodes
        }

        private void AttemptMoveTokenToNewNode(Node targetNode)
        {
            if (validTargetNodes.Contains(targetNode) == false)
            {
                //TODO: node invalid animation
                return;
            }

            Token token = nodeWithFriendlyToken.Token;
            nodeWithFriendlyToken.UnlinkToken();
            targetNode.LinkToken(token);

            gm.LatestPlayerMove = new PlayerTokenMove(nodeWithFriendlyToken, targetNode, gm.CurrentPlayer);

            moveComplete = true;
        }

        private bool NodeContainsFriendlyToken(Node node)
        {
            if (node.Token == null) return false;
            if (node.Token.Player == gm.CurrentPlayer) return true;
            return false;
        }
    }

    public class EvaluatePlayerMove : GamePhaseBase
    {
        public EvaluatePlayerMove(GameManager gameManager) : base(gameManager) 
        {
            name = PhaseName.EvaluatePlayerMove;
        }
     
        public override void Enter()
        {
            //Player has made at least one mill and must destroy opponent's token
            if (gm.UpdateMillsAndGetNewMillCount() > 0)
            {
                ChangeState(PhaseName.DestroyToken);
            }
            //Player has filled up the board completely and it is a draw
            else if (gm.Board.GetAllNodes().Any(n => n.Token == null) == false)
            {
                ChangeState(PhaseName.DrawGame);
            }
            //Player has boxed in opponent and wins
            else if (gm.PlayerHasLegalMoves(gm.EnemyPlayer) == false)
            {
                ChangeState(PhaseName.WinGame);
            }
            //Player has not done any of this and the game continues
            else
            {
                ChangeState(PhaseName.SwitchPlayer);
            }
        }
    }

    public class DestroyToken : GamePhaseBase
    {
        List<Node> validNodesForDestruction;

        public DestroyToken(GameManager gameManager) : base(gameManager)
        {
            name = PhaseName.DestroyToken;
            validNodesForDestruction = new();
        }

        public override void Enter()
        {
            HashSet<Node> allEnemyNodes = gm.Board.GetAllPlayerTokenNodes(gm.EnemyPlayer).ToHashSet();

            HashSet<Node> enemyNodesInMills = gm.GetAllNodesInMills(gm.EnemyPlayer);
            List<Node> enemyNodesOutsideMills = allEnemyNodes.Except(enemyNodesInMills).ToList();

            //If there are no enemy nodes outside mills, you can destroy any node
            if (enemyNodesOutsideMills.Count == 0)
            {
                validNodesForDestruction = allEnemyNodes.ToList();
            }
            else
            {
                validNodesForDestruction = enemyNodesOutsideMills;
            }


            Debug.Log($"Nodes in mill: {enemyNodesInMills.Count}, Total nodes: {allEnemyNodes.Count}. Valid tokens for destruction: {validNodesForDestruction.Count}.");
        }

        public override void EvaluateNodeClicked(Node node)
        {
            if (validNodesForDestruction.Contains(node))
            {
                DestroyTokenOnNode(node);

                //Check if win condition satisfied
                if (gm.EnemyPlayer.TokenManager.LivingTokensCount < gm.MinTokensForSurvival)
                {
                    ChangeState(PhaseName.WinGame);
                }
                else
                {
                    ChangeState(PhaseName.SwitchPlayer);
                }
            }
            else
            {
                //TODO: invalid selection animation
            }
        }

        private void DestroyTokenOnNode(Node node)
        {
            Token token = node.Token;
            node.UnlinkToken();
            token.DestroyToken();
        }
    }

    public class WinGame : GamePhaseBase
    {
        public WinGame(GameManager gameManager) : base(gameManager)
        {
            name = PhaseName.WinGame;
        }

        public override void Enter()
        {
            gm.CurrentPlayerWins();
        }
    }

    public class DrawGame : GamePhaseBase
    {
        public DrawGame(GameManager gameManager) : base(gameManager)
        {
            name = PhaseName.DrawGame;
        }

        public override void Enter()
        {
            gm.DrawGame();
        }
    }
}



