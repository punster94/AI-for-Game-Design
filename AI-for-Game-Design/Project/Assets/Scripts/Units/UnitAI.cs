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

    /// <summary>
    /// Run AI for a unit, returning a UnitAction.
    /// </summary>
    /// <param name="subject">The subject to run AI on.</param>
    /// <param name="subjectsEnemies">The current list of the subject's enemies.</param>
    /// <returns>The best UnitAction to take.</returns>
    public UnitAction RunAI(Unit subject, List<Unit> subjectsEnemies)
    {
        pathManager.calcUnitPaths(s, subjectsEnemies);
        return null;
    }
    
    private enum Result { Success = 0, NoUnitsInRange = 1, WillDie = 2}

    /// <summary>
    /// Runs MinMax on a unit.
    /// </summary>
    /// <returns>A result-UnitAction pair.</returns>
    private KeyValuePair<Result, UnitAction> MinMax()
    {
        return new KeyValuePair<Result, UnitAction>();
    }

    private UnitAction FindUnit()
    {
        return null;
    }

    private UnitAction RunAway()
    {
        return null;
    }
}
