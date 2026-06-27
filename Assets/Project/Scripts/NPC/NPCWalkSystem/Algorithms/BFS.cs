using System.Collections.Generic;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class BFS : PathfindingAlgorithm
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
            Queue<NPCAttractionPoint> openSet = new(estimatedSize);
            Dictionary<NPCAttractionPoint, NPCWay> cameFromEdge = new(estimatedSize)
                {[startVertex] = null};
            NPCAttractionPoint current;
            NPCAttractionPoint next;

            openSet.Enqueue(startVertex);

            while (openSet.Count > 0)
            {
                current = openSet.Dequeue();

                if (current == endVertex)
                    break;

                if (!adjList.ContainsKey(current))
                    continue;

                foreach (NPCWay way in adjList[current])
                {
                    next = way.To;

                    if (cameFromEdge.ContainsKey(next))
                        continue;

                    cameFromEdge[next] = way;
                    openSet.Enqueue(next);
                }
            }

            if (!cameFromEdge.ContainsKey(endVertex))
                return new List<NPCWay>();

            return ReconstructPath(cameFromEdge, endVertex);
        }
    }
}