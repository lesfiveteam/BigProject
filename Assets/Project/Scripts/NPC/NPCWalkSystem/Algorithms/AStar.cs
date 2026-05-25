using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class AStar : IAlgorithm
    {
        public List<NPCWay> FindShortestWay(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            NPCAttractionPoint startVertex,
            NPCAttractionPoint endVertex)
        {
            if (!adjList.ContainsKey(startVertex) || !adjList.ContainsKey(endVertex) || startVertex == endVertex)
                return new List<NPCWay>();

            Dictionary<NPCAttractionPoint, float> distances = new();
            Dictionary<NPCAttractionPoint, NPCWay> cameFromEdge = new();
            Dictionary<NPCAttractionPoint, NPCAttractionPoint> cameFromVertex = new();
            PriorityQueue<NPCAttractionPoint> pq = new();

            foreach (NPCAttractionPoint vertex in adjList.Keys)
                distances[vertex] = float.MaxValue;

            distances[startVertex] = 0f;
            pq.Enqueue(startVertex, Heuristic(startVertex, endVertex));

            while (pq.Count > 0)
            {
                NPCAttractionPoint current = pq.Dequeue();

                if (current == endVertex)
                    break;

                if (!adjList.ContainsKey(current))
                    continue;

                foreach (NPCWay way in adjList[current])
                {
                    NPCAttractionPoint next = way.To;
                    float newG = distances[current] + way.Distance;

                    if (newG < distances[next])
                    {
                        distances[next] = newG;
                        cameFromEdge[next] = way;
                        cameFromVertex[next] = current;
                        float newF = newG + Heuristic(next, endVertex);
                        pq.Enqueue(next, newF);
                    }
                }
            }

            return ReconstructPath(cameFromEdge, cameFromVertex, endVertex);
        }

        private float Heuristic(NPCAttractionPoint a, NPCAttractionPoint b) => Vector3.Distance(a.Position, b.Position);

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