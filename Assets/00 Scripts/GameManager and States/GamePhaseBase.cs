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
            gm.Board.SetupBoard(gm.LevelData); 
            gm.SetupTokenManagers(gm.Board.GetOffsetForTokenManagers());
            gm.InstantiateNewTokens();
            gm.SetupPlayerInfos();

            //Update camera size
            float height = gm.Board.GetBoardHeight();
            float width = gm.Board.GetOffsetForTokenManagers();
            gm.CamSize.UpdateCameraSize(height, width);
        
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

            //Update player info text
            gm.CurrentPlayer.PlayerInfo.UpdateActive(true);
            gm.EnemyPlayer.PlayerInfo.UpdateActive(false);

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

        public override void Enter()
        {
            List<Node> emptyNodes = gm.Board
                .GetAllEmptyNodes();
            gm.BatchAnim.MarkValidNodesForMovement(emptyNodes);

            gm.InfoText.WritePermanentText($"{gm.CurrentPlayer}, add a token to the board.");
            gm.CurrentPlayer.PlayerInfo.UpdateInfoText("Adding Token to Board");
        }

        public override void EvaluateNodeClicked(Node node)
        {
            if (node.Token == null)
            {
                Token token = gm.CurrentPlayer.TokenManager.SendTopTokenToNode(node);
                node.LinkToken(token);
                gm.UpdateMillsAndGetNewMills(null, node, gm.CurrentPlayer);

                token.FlyTo(node);

                ChangeState(PhaseName.EvaluatePlayerMove);
            }
            else
            {
                gm.TriggerInvalidNodeFX(node);

                gm.InfoText.WriteTempText($"Pick an empty node!");
            }
        }
    }

    public class MoveTokenOnBoard : GamePhaseBase
    {
        Node nodeWithFriendlyToken;
        List<Node> tokenNodesForSelection;
        List<Node> emptyNodesForMovement;
        bool moveComplete;

        public MoveTokenOnBoard(GameManager gameManager) : base(gameManager)
        {
            name = PhaseName.MoveTokenOnBoard;
            nodeWithFriendlyToken = null;
            tokenNodesForSelection = new();
            emptyNodesForMovement = new();
            moveComplete = false;
        }

        public override void Enter()
        {
            if (PlayerCanFly())
            {
                tokenNodesForSelection = gm.Board.GetAllPlayerTokenNodes(gm.CurrentPlayer);

                gm.CurrentPlayer.PlayerInfo.UpdateInfoText("Flying");
            }
            else
            {
                tokenNodesForSelection = gm.Board
                    .GetAllPlayerTokenNodes(gm.CurrentPlayer)
                    .Where(n => gm.Board.GetConnectingNodes(n).Count > 0)
                    .ToList();

                gm.CurrentPlayer.PlayerInfo.UpdateInfoText("Sliding");
            }

            gm.BatchAnim.MarkValidNodesForMovement(emptyNodesForMovement);
            gm.BatchAnim.MarkValidTokenNodesForSelection(tokenNodesForSelection, true);
            gm.InfoText.WritePermanentText($"{gm.CurrentPlayer}, choose a token to move, then move it.");
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
            tokenNodesForSelection.Clear();
            emptyNodesForMovement.Clear();
            moveComplete = false;

            gm.BatchAnim.MarkValidNodesForMovement(emptyNodesForMovement);
            gm.BatchAnim.MarkValidTokenNodesForSelection(tokenNodesForSelection, true);
        }

        private void AttemptSelectFriendlyTokenNode(Node selectedNode)
        {
            if (tokenNodesForSelection.Contains(selectedNode) == false)
            {
                gm.TriggerInvalidNodeFX(selectedNode);
                gm.InfoText.WriteTempText("Select your own piece!");
                return;
            }

            nodeWithFriendlyToken = selectedNode;
            gm.BatchAnim.MarkSelectedTokenNode(selectedNode);

            if (PlayerCanFly() == false)
            {
                emptyNodesForMovement = gm.Board
                    .GetConnectingNodes(selectedNode)
                    .Where(n => n.Token == null)
                    .ToList();
            }
            else
            {
                emptyNodesForMovement = gm.Board.GetAllEmptyNodes();
            }

            gm.BatchAnim.MarkValidNodesForMovement(emptyNodesForMovement);
        }

        private void AttemptMoveTokenToNewNode(Node targetNode)
        {
            if (emptyNodesForMovement.Contains(targetNode) == false)
            {
                gm.TriggerInvalidNodeFX(targetNode);
                gm.InfoText.WriteTempText($"You cannot move there!");
                return;
            }

            //Handle linking to node
            Token token = nodeWithFriendlyToken.Token;
            nodeWithFriendlyToken.UnlinkToken();
            targetNode.LinkToken(token);

            //Animate movement
            if (PlayerCanFly()) token.FlyTo(targetNode);
            else token.SlideTo(targetNode);

            gm.UpdateMillsAndGetNewMills(nodeWithFriendlyToken, targetNode, gm.CurrentPlayer);

            moveComplete = true;
        }

        private bool PlayerCanFly()
        {
            return gm.CurrentPlayer.TokenManager.LivingTokensCount <= gm.LevelData.MaxTokensForFlying;
        }
    }

    public class EvaluatePlayerMove : GamePhaseBase
    {
        List<Node> newMillNodes;

        public EvaluatePlayerMove(GameManager gameManager) : base(gameManager) 
        {
            name = PhaseName.EvaluatePlayerMove;
            newMillNodes = new();
        }
     
        public override void Enter()
        {
            newMillNodes = gm.GetNodesInNewMills();

            //Player has made at least one mill and must destroy opponent's token
            if (newMillNodes.Count > 0)
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

        public override void Exit()
        {
            gm.TriggerMillFXs(newMillNodes);

            newMillNodes.Clear();
            gm.ClearNewMills();
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

            HashSet<Node> enemyNodesInMills = gm.GetAllNodesInMillsForPlayer(gm.EnemyPlayer);
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

            gm.BatchAnim.MarkValidTokenNodesForSelection(validNodesForDestruction, false);
            gm.InfoText.WritePermanentText($"{gm.CurrentPlayer}, pick one of {gm.EnemyPlayer}'s tokens to destroy!");
            gm.InfoText.WriteTempText($"Well done, {gm.CurrentPlayer}, you have built a mill!");
            gm.CurrentPlayer.PlayerInfo.UpdateInfoText("Destroying Enemy Token");
        }

        public override void EvaluateNodeClicked(Node node)
        {
            if (validNodesForDestruction.Contains(node))
            {
                DestroyTokenOnNode(node);

                //Check if win condition satisfied
                if (gm.EnemyPlayer.TokenManager.LivingTokensCount < gm.LevelData.NumOfTokensForMill)
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
                gm.TriggerInvalidNodeFX(node);

                if (node.Token != null)
                {
                    if (node.Token.Player == gm.CurrentPlayer)
                    {
                        gm.InfoText.WriteTempText($"That's your token! Don't destroy that.");
                    }
                    else if (gm.NodeIsInMill(node))
                    {
                        gm.InfoText.WriteTempText($"Sorry, that token is in a mill.");
                    }
                }
                else
                {
                    gm.InfoText.WriteTempText($"That's not {gm.EnemyPlayer}'s token!");
                }
            }
        }

        public override void Exit()
        {
            validNodesForDestruction.Clear();

            gm.BatchAnim.MarkValidTokenNodesForSelection(validNodesForDestruction, false);
        }

        private void DestroyTokenOnNode(Node node)
        {
            gm.UpdateMillsForRemovedToken(node);
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
            gm.TriggerCurrentPlayerWins();
            gm.InfoText.WritePermanentText($"{gm.CurrentPlayer} wins!");
            gm.CurrentPlayer.PlayerInfo.UpdateInfoText("Winner");
            gm.EnemyPlayer.PlayerInfo.UpdateInfoText("Loser");
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
            gm.TriggerDrawGame();
            gm.InfoText.WritePermanentText($"Draw!");
        }
    }
}



