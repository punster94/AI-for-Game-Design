﻿using System;
using System.Collections.Generic;
using Graph;

class UnitAI
{
    PathFinder pathFinderRef;
    PathManager pathManager;

    public UnitAI(PathFinder pathRef)
    {
        pathFinderRef = pathRef;
        pathManager = new PathManager(pathFinderRef);
    }

    Unit subjectRef;
    List<Unit> subjectsEnemiesRef;

    /// <summary>
    /// Run AI for a unit, returning a UnitAction.
    /// </summary>
    /// <param name="subject">The subject to run AI on.</param>
    /// <param name="subjectsEnemies">The current list of the subject's enemies.</param>
    /// <returns>The best UnitAction to take.</returns>
    public UnitAction RunAI(Unit subject, List<Unit> subjectsEnemies)
    {
        subjectRef = subject;
        subjectsEnemiesRef = subjectsEnemies;

        pathManager.calcUnitPaths(subjectRef, subjectsEnemies);

        int sanityCheck = subjectRef.getClay();
        
        KeyValuePair<Result, UnitAction> MinMaxResult = MinMax();
        if (subjectRef.getClay() != sanityCheck)
            UnityEngine.Debug.Log("ERROR IN MINMAX AGAIN!");
        switch (MinMaxResult.Key)
        {
            case Result.Success:
                return MinMaxResult.Value;

            // Nothing in range.
            case Result.NothingFound:
                KeyValuePair<Result, UnitAction> AttemptFind = FindUnit();

                //We've won!
                if (AttemptFind.Key == Result.NothingFound)
                {
                    return UnitAction.DoNothing(subjectRef);
                }
                return AttemptFind.Value;

            // Note that even if we can attack from the running position, we don't while running,
            // as we know we can die then.
            case Result.WillDie:
                KeyValuePair<Result, UnitAction> runTo = RunAway();

                // fight to the last! we can't move, so...
                if (runTo.Key == Result.NothingFound)
                {
                    return MinMaxResult.Value;
                }
                return runTo.Value;

            default:
                throw new InvalidProgramException("RunAI: MinMaxResult enum reached invalid state: " + MinMaxResult.Key);
        }
    }

    //Internal enum for cleaner code.
    private enum Result { Success = 0, NothingFound = 1, WillDie = 2 }

    /// <summary>
    /// Runs MinMax on a unit, judging running vs fighting.
    /// </summary>
    /// <returns>A result-UnitAction pair.</returns>
    private KeyValuePair<Result, UnitAction> MinMax()
    {
        List<Node.NodePointer> targets = pathManager.getCurrentTargets(subjectRef);
        MinPriorityQueue<UnitAction> bestMoves = new MinPriorityQueue<UnitAction>();
        int realclay = subjectRef.getClay();

        string counter = "I, " + subjectRef.ident() + " am evaluating moves.\n";
        int round = 0;

        // The "max-y" part: maximize (damage/counter-damage)
        foreach (Node.NodePointer candidateAttackTarget in targets)
        {
            int moveCost = candidateAttackTarget.getDist();

            Unit curEnemy = candidateAttackTarget.getTarget().Occupier;
            counter += "round " + round++ + ": attacking " + curEnemy.ident() +"\n";

            AttackRound util = new AttackRound(subjectRef, moveCost, curEnemy);
            
            UnitAction roundMove = new UnitAction(subjectRef, curEnemy, candidateAttackTarget.getStart());

            int totDmg = 0;

            // Will very likely die, enqueue this move as bad, and don't move to next step.
            if (util.attackerDies())
            {
                bestMoves.Enqueue(roundMove, double.PositiveInfinity);
                util.resetBack();
                subjectRef.setClay(realclay);
                counter += "\nround failed, 1st stage.";
                continue;
            }

            totDmg += util.getExpectedDamage();

            totDmg += RunEnemyTurn(candidateAttackTarget.getStart());

            // Died. Enqueue with +inf again.
            if (subjectRef.getClay() <= 0)
            {
                bestMoves.Enqueue(roundMove, double.PositiveInfinity);
                counter += "round failed, 2nd stage.\n";
            }
            // enqueue move! min pri queue, so invert answer.
            else
            {
                bestMoves.Enqueue(roundMove, -((double)totDmg * subjectRef.getClay()));
                counter += "round succeeded, value of " + -(double) totDmg * subjectRef.getClay() + "\n";
            }
            
            util.resetBack();
            subjectRef.setClay(realclay);
        }

        // If we're going to die, try to look for spot to run away to.
        if (bestMoves.Count > 0 && bestMoves.currentInversePriority(bestMoves.Peek()) == double.PositiveInfinity)
            // The "max-y" part: maximize (damage/counter-damage)
            foreach (Node candidateRunSquare in pathManager.getAccessibleNodes(subjectRef))
            {
                round++;
                int moveCost = pathManager.costOfSquare(subjectRef, candidateRunSquare);
                float totDmg = 0.01f;
                UnitAction roundMove = new UnitAction(subjectRef, null, candidateRunSquare);

                totDmg += RunEnemyTurn(candidateRunSquare);

                // Died. Enqueue with +inf again.
                if (subjectRef.getClay() <= 0)
                {
                    bestMoves.Enqueue(roundMove, double.PositiveInfinity);
                    counter += "round failed, 2nd stage.\n";
                }
                // enqueue move! min pri queue, so invert answer.
                else
                {
                    bestMoves.Enqueue(roundMove, -((double)totDmg * subjectRef.getClay()));
                    counter += "round succeeded, value of " + -(double)totDmg * subjectRef.getClay() + "\n";
                }
                
                subjectRef.setClay(realclay);
            }


        if (counter.Length > 0)
        {
            if (bestMoves.Count > 0)
                counter += "Expected value of best move: " + bestMoves.currentInversePriority(bestMoves.Peek());
            else
                counter += "No best moves.";
            UnityEngine.Debug.Log(counter);
        }

        subjectRef.setClay(realclay);

        // no local targets...
        if (bestMoves.Count == 0)
            return new KeyValuePair<Result, UnitAction>(Result.NothingFound, null);
        // all moves die, including running away! fight to the death!
        //else if (bestMoves.currentInversePriority(bestMoves.Peek()) == double.PositiveInfinity)
        //    return new KeyValuePair<Result, UnitAction>(Result.WillDie, bestMoves.Peek());
        // found good target.
        return new KeyValuePair<Result, UnitAction>(Result.Success, bestMoves.Peek());
    }

    // Loop through all things that could attack this position, and continue testing attacks.
    // The "min-y" part. Enemy tries to maximize their (damage/counter-damage)
    // Note: because there is no teamwork, units guestimate that a unit far away might not come to attack them always.
    private int RunEnemyTurn(Node weWillBeAt)
    {
        int totDmg = 0;
        foreach (Unit enemy in subjectsEnemiesRef)
        {
            if (enemy.getClay() > 0 && pathManager.canAttack(enemy, weWillBeAt))
            {
                // save current enemy water state.
                int curEnemyWater = enemy.getCurrentWater();
                enemy.setCurrentWater(enemy.getMaxWater());
                // gets the closest move. This will be the move that maxes damage.
                int enemyMoveCost = pathManager.maxDamageMoveCost(enemy, weWillBeAt);

                int prevClay = subjectRef.getClay();

                //guestimation: if removed, significant performance cost imposed, although becomes more like minmax.
                float probability = UnityEngine.Mathf.Min(enemyMoveCost / 1.4f, enemy.getMaxWater() / 2);
                probability /= enemy.getMaxWater();
                probability = 1 - probability;

                AttackRound subRound = new AttackRound(enemy, enemyMoveCost, subjectRef);
                int roundClay = subjectRef.getClay();
                int usCost = UnityEngine.Mathf.RoundToInt((probability) * (prevClay - roundClay));

                // reset enemy state.
                subRound.resetBack();
                enemy.setCurrentWater(curEnemyWater);
                subjectRef.setClay(prevClay - usCost);

                totDmg += subRound.getExpectedCounterDamage();

                if (subjectRef.getClay() <= 0)
                    return totDmg;
            }
        }
        return totDmg;
    }

    private KeyValuePair<Result, UnitAction> FindUnit()
    {
        Predicate<Node> findEn = (Node q) =>
        {
            bool isOccupied = q.Occupied;
            if (!isOccupied)
                return false;
            if (q.Occupier.isEnemy() != subjectRef.isEnemy())
                return true;
            return false;
        };

        Node closestEnemyNode = pathManager.getClosestNode(subjectRef, findEn);
        if (closestEnemyNode == null)
            return new KeyValuePair<Result, UnitAction>(Result.NothingFound, null);

        Queue<Node> path = new Queue<Node>();
        pathFinderRef.AStar(path, subjectRef.getNode(), closestEnemyNode, closestEnemyNode);
        
        Node goTo = subjectRef.getNode();

        // go to farthest path.
        while (path.Count > 0 && pathManager.canWalkTo(subjectRef, path.Peek()))
            goTo = path.Dequeue();

        UnitAction moveCloserToClosestEnemy = new UnitAction(subjectRef, null, goTo);

        return new KeyValuePair<Result, UnitAction>(Result.Success, moveCloserToClosestEnemy);
    }

    // run away to the node that's farthest away from all enemies.
    private KeyValuePair<Result, UnitAction> RunAway()
    {
        MinPriorityQueue<UnitAction> farthestActions = new MinPriorityQueue<UnitAction>();
        foreach (Node n in pathManager.getAccessibleNodes(subjectRef))
        {
            int curCount = 0;
            foreach (Unit en in subjectsEnemiesRef)
                curCount += Node.range(n, en.getNode());

            // min priority queue: negate results.
            UnitAction runAwayTo = new UnitAction(subjectRef, null, n);
            farthestActions.Enqueue(runAwayTo, -curCount);
        }

        if (farthestActions.Count == 0)
            return new KeyValuePair<Result, UnitAction>(Result.NothingFound, UnitAction.DoNothing(subjectRef));
        else
            return new KeyValuePair<Result, UnitAction>(Result.Success, farthestActions.Dequeue());
    }
}
