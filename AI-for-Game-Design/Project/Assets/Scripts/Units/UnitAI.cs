using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    /// <summary>
    /// Run AI for a unit, returning a UnitAction.
    /// </summary>
    /// <param name="subject">The subject to run AI on.</param>
    /// <param name="subjectsEnemies">The current list of the subject's enemies.</param>
    /// <returns>The best UnitAction to take.</returns>
    public UnitAction RunAI(Unit subject, List<Unit> subjectsEnemies)
    {
        subjectRef = subject;

        pathManager.calcUnitPaths(subjectRef, subjectsEnemies);

        KeyValuePair<Result, UnitAction> MinMaxResult = MinMax();
        switch (MinMaxResult.Key)
        {
            case Result.Success:
                return MinMaxResult.Value;

            case Result.NoUnitsFound:
                KeyValuePair<Result, UnitAction> AttemptFind = FindUnit();

                //We've won!
                if (AttemptFind.Key == Result.NoUnitsFound)
                {
                    return UnitAction.DoNothing(subjectRef);
                }
                return AttemptFind.Value;

            case Result.WillDie:
                return RunAway();

            default:
                throw new InvalidProgramException("RunAI: MinMaxResult enum reached invalid state: " + MinMaxResult.Key);
        }
    }

    //Internal enum for cleaner code.
    private enum Result { Success = 0, NoUnitsFound = 1, WillDie = 2 }

    /// <summary>
    /// Runs MinMax on a unit.
    /// </summary>
    /// <returns>A result-UnitAction pair.</returns>
    private KeyValuePair<Result, UnitAction> MinMax()
    {
        return new KeyValuePair<Result, UnitAction>(Result.WillDie, null);
    }

    private KeyValuePair<Result, UnitAction> FindUnit()
    {
        return new KeyValuePair<Result, UnitAction>(Result.NoUnitsFound, null);
    }

    private UnitAction RunAway()
    {
        return null;
    }
}
