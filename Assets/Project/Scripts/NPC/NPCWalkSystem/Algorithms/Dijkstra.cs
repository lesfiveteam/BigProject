using System.Collections.Generic;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class Dijkstra : PathfindingAlgorithm
    {
        public override List<NPCWay> FindShortestWay(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            NPCAttractionPoint startVertex,
            NPCAttractionPoint endVertex)
        {
            if (!adjList.ContainsKey(startVertex) ||
                !adjList.ContainsKey(endVertex) ||
                startVertex == endVertex)
            {
                return new();
            }

            int estimatedSize = adjList.Count;
            PriorityQueue<NPCAttractionPoint> openSet = new(estimatedSize);
            Dictionary<NPCAttractionPoint, float> gScore = new(estimatedSize);
            Dictionary<NPCAttractionPoint, NPCWay> cameFromEdge = new(estimatedSize);
            NPCAttractionPoint current;
            NPCAttractionPoint next;

            foreach (NPCAttractionPoint vertex in adjList.Keys)
                gScore[vertex] = float.MaxValue;

            gScore[startVertex] = 0f;
            openSet.Enqueue(startVertex, 0f);

            while (openSet.Count > 0)
            {
                openSet.TryDequeue(out current, out float currentG);

                if (currentG > gScore[current])
                    continue;

                if (current == endVertex)
                    break;

                if (!adjList.ContainsKey(current))
                    continue;

                foreach (NPCWay way in adjList[current])
                {
                    next = way.To;
                    float newG = currentG + way.Distance;

                    if (newG < gScore[next])
                    {
                        gScore[next] = newG;
                        cameFromEdge[next] = way;
                        openSet.Enqueue(next, newG);
                    }
                }
            }

            if (!cameFromEdge.ContainsKey(endVertex))
                return new List<NPCWay>();

            return ReconstructPath(cameFromEdge, endVertex);
        }
    }
}