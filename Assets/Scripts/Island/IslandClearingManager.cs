
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandClearingManager : MonoBehaviour
{
    [Header("Clearing Settings")]
    [SerializeField] private float workRate = 1f;
    [SerializeField] private float chopImpactInterval = 1.2f;
    [SerializeField] private float assignmentInterval = 1f;
    [SerializeField] private float maxCrewToIslandDistance = 10f;
    [SerializeField] private float workDistance = 0.3f;

    private IslandDataManager dataManager;
    private IslandTileVisuals tileVisuals;
    private IslandObstacleVisuals obstacleVisuals;
    private IslandNavigation navigation;

    private readonly Dictionary<CrewMovement, WorkerTask> workers =
        new Dictionary<CrewMovement, WorkerTask>();

    private readonly HashSet<IslandObstacle> reservedObstacles =
        new HashSet<IslandObstacle>();

    private float assignmentTimer;

    private static readonly int[,] Directions =
    {
        { 0, 1 },
        { -1, 1 },
        { -1, 0 },
        { 0, -1 },
        { 1, -1 },
        { 1, 0 }
    };

    private class WorkerTask
    {
        public IslandTile tile;
        public IslandObstacle obstacle;
        public IslandObstacleVisual visual;
        public TreeChopAnimation animation;
        public float impactTimer;
        public bool arrived;
        public bool completing;
    }

    private IEnumerator Start()
    {
        dataManager = GetComponent<IslandDataManager>();
        tileVisuals = GetComponent<IslandTileVisuals>();
        obstacleVisuals = GetComponent<IslandObstacleVisuals>();
        navigation = GetComponent<IslandNavigation>();

        // Allow the tile visuals to finish generating.
        yield return null;
        yield return null;

        if (dataManager == null ||
            tileVisuals == null ||
            obstacleVisuals == null ||
            navigation == null)
        {
            Debug.LogError(
                "CLEARING MANAGER | Missing island components."
            );
            enabled = false;
            yield break;
        }

        Debug.Log("CLEARING MANAGER | Ready.");
    }

    private void Update()
    {
        if (dataManager == null)
            return;

        assignmentTimer += Time.deltaTime;

        if (assignmentTimer >= assignmentInterval)
        {
            assignmentTimer = 0f;
            AssignAvailableWorkers();
        }

        UpdateWorkers();
    }

    private void AssignAvailableWorkers()
    {
        if (CrewManager.Instance == null)
            return;

        foreach (CrewMovement crew in CrewManager.Instance.crewMembers)
        {
            if (!IsEligibleCrew(crew))
                continue;

            if (workers.ContainsKey(crew))
                continue;

            TryAssignNearestTree(crew);
        }
    }

    private bool IsEligibleCrew(CrewMovement crew)
    {
        if (crew == null || !crew.gameObject.activeInHierarchy)
            return false;

        if (crew.crewData == null)
            return false;

        if (crew.crewData.isOnVoyage)
            return false;

        if (crew.assignedToMission)
            return false;

        if (crew.currentJob != CrewMovement.CrewJob.Idle)
            return false;

        if (crew.crewData.islandJob != CrewIslandJob.Idle)
            return false;

        return true;
    }

    private void TryAssignNearestTree(CrewMovement crew)
    {
        IslandTile startTile = FindNearestWalkableTile(
            crew.transform.position
        );

        if (startTile == null)
            return;

        GameObject startVisual =
            tileVisuals.GetTileVisual(startTile);

        if (startVisual == null)
            return;

        float distanceToIsland = Vector3.Distance(
            crew.transform.position,
            startVisual.transform.position
        );

        // Never send crew wandering across the scene.
        if (distanceToIsland > maxCrewToIslandDistance)
            return;

        IslandTile bestTile = null;
        IslandObstacle bestObstacle = null;
        IslandObstacleVisual bestVisual = null;
        List<Vector3> bestPath = null;
        float bestDistance = float.MaxValue;

        foreach (IslandTile tile in dataManager.Tiles)
        {
            if (tile == null ||
                !tile.owned ||
                !tile.isExpansionTile ||
                tile.tileState != IslandTileState.Clearing)
                continue;

            foreach (IslandObstacle obstacle in tile.obstacles)
            {
                if (obstacle == null ||
                    obstacle.obstacleType != IslandObstacleType.Tree ||
                    reservedObstacles.Contains(obstacle))
                    continue;

                IslandObstacleVisual visual =
                    obstacleVisuals.GetObstacleVisual(obstacle);

                if (visual == null)
                    continue;

                if (!TryGetApproachPath(
                    startTile,
                    tile,
                    visual.transform.position,
                    out List<Vector3> path))
                    continue;

                float distance = GetPathDistance(
                    crew.transform.position,
                    path
                );

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTile = tile;
                    bestObstacle = obstacle;
                    bestVisual = visual;
                    bestPath = path;
                }
            }
        }

        if (bestTile == null || bestObstacle == null ||
            bestVisual == null || bestPath == null)
            return;

        WorkerTask task = new WorkerTask
        {
            tile = bestTile,
            obstacle = bestObstacle,
            visual = bestVisual,
            animation = bestVisual.GetComponent<TreeChopAnimation>(),
            impactTimer = 0f
        };

        workers.Add(crew, task);
        reservedObstacles.Add(bestObstacle);

        int obstacleIndex = bestTile.obstacles.IndexOf(bestObstacle);

        crew.AssignClearingJob(
            bestTile,
            bestObstacle,
            bestPath,
            bestVisual.transform.position,
            this
        );

        Debug.Log(
            "CLEARING ASSIGNED | " +
            crew.crewData.crewName +
            " | Tile (" + bestTile.q + ", " + bestTile.r + ")" +
            " | Obstacle " + obstacleIndex
        );
    }

    private IslandTile FindNearestWalkableTile(Vector3 position)
    {
        IslandTile closest = null;
        float closestDistance = float.MaxValue;

        foreach (IslandTile tile in dataManager.Tiles)
        {
            if (tile == null || !tile.owned)
                continue;

            if (tile.tileState != IslandTileState.Available &&
                tile.tileState != IslandTileState.Developed &&
                tile.tileState != IslandTileState.UnderConstruction)
                continue;

            if (navigation.IsTileObstructed(tile))
                continue;

            GameObject visual = tileVisuals.GetTileVisual(tile);

            if (visual == null)
                continue;

            float distance = Vector3.Distance(
                position,
                visual.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = tile;
            }
        }

        return closest;
    }

    private bool TryGetApproachPath(
        IslandTile startTile,
        IslandTile clearingTile,
        Vector3 treePosition,
        out List<Vector3> bestPath)
    {
        bestPath = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < 6; i++)
        {
            IslandTile approachTile = dataManager.GetTile(
                clearingTile.q + Directions[i, 0],
                clearingTile.r + Directions[i, 1]
            );

            if (approachTile == null ||
                !approachTile.owned ||
                (approachTile.tileState != IslandTileState.Available &&
                 approachTile.tileState != IslandTileState.Developed &&
                 approachTile.tileState != IslandTileState.UnderConstruction))
                continue;

            if (!navigation.TryFindPath(
                startTile,
                approachTile,
                out List<Vector3> path))
                continue;

            // Finish the route at the actual tree position.
            path.Add(treePosition);

            float distance = GetPathDistance(
                tileVisuals.GetTileVisual(startTile).transform.position,
                path
            );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestPath = path;
            }
        }

        return bestPath != null;
    }

    private float GetPathDistance(
        Vector3 start,
        List<Vector3> path)
    {
        float distance = 0f;
        Vector3 previous = start;

        foreach (Vector3 point in path)
        {
            distance += Vector3.Distance(previous, point);
            previous = point;
        }

        return distance;
    }

    public void NotifyCrewArrived(CrewMovement crew)
    {
        if (!workers.TryGetValue(crew, out WorkerTask task))
            return;

        task.arrived = true;

        Debug.Log(
            "CLEARING WORK STARTED | " +
            crew.crewData.crewName +
            " | Tile (" + task.tile.q + ", " + task.tile.r + ")"
        );
    }

    private void UpdateWorkers()
    {
        List<CrewMovement> finishedWorkers =
            new List<CrewMovement>();

        foreach (KeyValuePair<CrewMovement, WorkerTask> entry in workers)
        {
            CrewMovement crew = entry.Key;
            WorkerTask task = entry.Value;

            if (crew == null || task == null)
            {
                finishedWorkers.Add(crew);
                continue;
            }

            if (!task.arrived || task.completing)
                continue;

            if (task.obstacle == null ||
                !task.tile.obstacles.Contains(task.obstacle))
            {
                FinishWorker(crew, task);
                finishedWorkers.Add(crew);
                continue;
            }

            task.obstacle.workRemaining -=
                workRate * Time.deltaTime;

            task.impactTimer += Time.deltaTime;

            if (task.animation != null &&
                task.impactTimer >= chopImpactInterval)
            {
                task.impactTimer = 0f;
                Vector3 hitDirection =
    task.visual.transform.position - crew.transform.position;

                task.animation.PlayChopImpact(hitDirection);
            }

            if (task.obstacle.workRemaining <= 0f)
            {
                task.completing = true;
                StartCoroutine(FinishTree(crew, task));
            }
        }

        foreach (CrewMovement crew in finishedWorkers)
        {
            if (crew != null)
                workers.Remove(crew);
        }
    }

    private IEnumerator FinishTree(
        CrewMovement crew,
        WorkerTask task)
    {
        if (task.animation != null)
        {
            Vector3 fallDirection =
                task.visual.transform.position - crew.transform.position;

            task.animation.Fall(fallDirection);

            float fallTime =
                task.animation.fallDuration +
                task.animation.bounceDuration;

            yield return new WaitForSeconds(fallTime + 0.2f);
        }

        obstacleVisuals.RemoveObstacleVisual(task.obstacle);
        task.tile.obstacles.Remove(task.obstacle);

        Debug.Log(
            "TREE CLEARED | Tile (" +
            task.tile.q + ", " + task.tile.r +
            ") | Remaining obstacles: " +
            task.tile.obstacles.Count
        );

        if (task.tile.obstacles.Count == 0)
        {
            task.tile.tileState = IslandTileState.Available;

            Debug.Log(
                "TILE CLEARING COMPLETE | Tile (" +
                task.tile.q + ", " + task.tile.r + ")"
            );
        }

        reservedObstacles.Remove(task.obstacle);

        FinishWorker(crew, task);
        workers.Remove(crew);
    }

    private void FinishWorker(
        CrewMovement crew,
        WorkerTask task)
    {
        if (task != null && task.obstacle != null)
            reservedObstacles.Remove(task.obstacle);

        if (crew != null)
            crew.CompleteClearingJob();
    }
}
