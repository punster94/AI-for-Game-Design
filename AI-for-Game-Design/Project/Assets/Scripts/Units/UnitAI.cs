using System;
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

        KeyValuePair<Result, UnitAction> MinMaxResult = MinMax();
        switch (MinMaxResult.Key)
        {
            case Result.Success:
                return MinMaxResult.Value;

            case Result.NothingFound:
                KeyValuePair<Result, UnitAction> AttemptFind = FindUnit();

                //We've won!
                if (AttemptFind.Key == Result.NothingFound)
                {
                    return UnitAction.DoNothing(subjectRef);
                }
                return AttemptFind.Value;

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
    /// Runs MinMax on a unit.
    /// </summary>
    /// <returns>A result-UnitAction pair.</returns>
    private KeyValuePair<Result, UnitAction> MinMax()
    {
        List<Node.NodePointer> targets = pathManager.getCurrentTargets(subjectRef);
        MinPriorityQueue<UnitAction> bestMoves = new MinPriorityQueue<UnitAction>();

        // The "max-y" part: maximize (damage/counter-damage)
        foreach (Node.NodePointer candidateAttackTarget in targets)
        {
            int moveCost = candidateAttackTarget.getDist();

            Unit curEnemy = candidateAttackTarget.getTarget().Occupier;

            AttackRound util = new AttackRound(subjectRef, moveCost, curEnemy);

            UnitAction roundMove = new UnitAction(subjectRef, curEnemy, candidateAttackTarget.getStart());

            int totDmg = 0;

            // Will very likely die, enqueue this move as bad, and don't move to next step.
            if (util.attackerDies())
            {
                bestMoves.Enqueue(roundMove, double.PositiveInfinity);
                util.resetBack();
                continue;
            }

            totDmg += util.getExpectedDamage();

            // Loop through all things that could attack this position, and continue testing attacks.
            // The "min-y" part. Enemy tries to maximize their (damage/counter-damage)
            // Technically could just do damage maximization, since counter-damage is fixed.
            foreach (Unit enemy in subjectsEnemiesRef)
            {
                if (enemy.getClay() > 0 && pathManager.canAttack(enemy, candidateAttackTarget.getStart()))
                {
                    //gets the closest move. This will be the move that maxes damage.
                    int enemyMoveCost = pathManager.maxDamageMoveCost(enemy, candidateAttackTarget.getStart());
                    AttackRound subRound = new AttackRound(enemy, enemyMoveCost, subjectRef);
                    int roundClay = subjectRef.getClay();

                    subRound.resetBack();
                    subjectRef.setClay(roundClay);

                    // If we die, break early, as usual.
                    if (util.defenderDies())
                    {
                        break;
                    }

                    totDmg += subRound.getExpectedCounterDamage();
                }
            }

            // Died. Enqueue with +inf again.
            if (subjectRef.getClay() == 0)
                bestMoves.Enqueue(roundMove, double.PositiveInfinity);
            // enqueue move! min pri queue, so invert answer.
            else
                bestMoves.Enqueue(roundMove, -((double)totDmg / subjectRef.getClay()));

            util.resetBack();
        }

        // no local targets...
        if (bestMoves.Count == 0)
            return new KeyValuePair<Result, UnitAction>(Result.NothingFound, null);
        // all moves die. only move is not to play.
        else if (bestMoves.currentInversePriority(bestMoves.Peek()) == double.PositiveInfinity)
            return new KeyValuePair<Result, UnitAction>(Result.WillDie, bestMoves.Peek());
        // found good target.
        return new KeyValuePair<Result, UnitAction>(Result.Success, bestMoves.Peek());
    }

    private KeyValuePair<Result, UnitAction> FindUnit()
    {
        Node closestEnemyNode = pathManager.getClosestNode(subjectRef, (Node q) => { return q.Occupied && q.Occupier.isEnemy() != subjectRef.isEnemy(); });
        if (closestEnemyNode == null)
            return new KeyValuePair<Result, UnitAction>(Result.NothingFound, null);

        Queue<Node> path = new Queue<Node>();
        pathFinderRef.AStar(path, subjectRef.getNode(), closestEnemyNode);

        Node goTo = null;

        // go to farthest path.
        while (path.Count > 0 && pathManager.inPaths(subjectRef, path.Peek()))
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
