using System.Collections.Generic;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class BFS : IAlgorithm
    {
        public List<NPCWay> FindShortestWay(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            NPCAttractionPoint startVertex,
            NPCAttractionPoint endVertex)
        {
            if (!adjList.ContainsKey(startVertex) 
                || !adjList.ContainsKey(endVertex) 
                || startVertex == endVertex)
                return new List<NPCWay>();

            Queue<NPCAttractionPoint> queue = new();
            Dictionary<NPCAttractionPoint, NPCWay> cameFromEdge = new();
            Dictionary<NPCAttractionPoint, NPCAttractionPoint> cameFromVertex = new();
            HashSet<NPCAttractionPoint> visited = new();

            queue.Enqueue(startVertex);
            visited.Add(startVertex);

            while (queue.Count > 0)
            {
                NPCAttractionPoint current = queue.Dequeue();

                if (current == endVertex)
                    break;

                if (!adjList.ContainsKey(current))
                    continue;

                foreach (NPCWay way in adjList[current])
                {
                    NPCAttractionPoint next = way.To;

                    if (visited.Contains(next))
                        continue;

                    visited.Add(next);
                    cameFromEdge[next] = way;
                    cameFromVertex[next] = current;
                    queue.Enqueue(next);
                }
            }

            return ReconstructPath(cameFromEdge, cameFromVertex, endVertex);
        }
        private List<NPCWay> ReconstructPath(
            Dictionary<NPCAttractionPoint, NPCWay> cameFromEdge,
            Dictionary<NPCAttractionPoint, NPCAttractionPoint> cameFromVertex,
            NPCAttractionPoint endVertex)
        {
            List<NPCWay> route = new();
            NPCAttractionPoint current = endVertex;

            while (cameFromEdge.ContainsKey(current))
            {
                route.Insert(0, cameFromEdge[current]);
                current = cameFromVertex[current];
            }

            return route;
        }
    }
}