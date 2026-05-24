using System.Collections.Generic;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class Dijkstra : IAlgorithm
    {
        public List<NPCWay> FindShortestWay(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            NPCAttractionPoint startVertex,
            NPCAttractionPoint endVertex)
        {

            if (!adjList.ContainsKey(startVertex) ||
                !adjList.ContainsKey(endVertex) ||
                startVertex == endVertex)
                return new List<NPCWay>();

            Dictionary<NPCAttractionPoint, float> distances = new();
            Dictionary<NPCAttractionPoint, NPCWay> previousEdge = new();
            Dictionary<NPCAttractionPoint, NPCAttractionPoint> previousVertex = new();
            HashSet<NPCAttractionPoint> unvisited = new();

            foreach (NPCAttractionPoint vertex in adjList.Keys)
            {
                distances[vertex] = float.MaxValue;
                unvisited.Add(vertex);
            }

            distances[startVertex] = 0f;

            while (unvisited.Count > 0)
            {
                NPCAttractionPoint current = null;
                float minDist = float.MaxValue;

                foreach (NPCAttractionPoint vertex in unvisited)
                {
                    if (distances[vertex] < minDist)
                    {
                        minDist = distances[vertex];
                        current = vertex;
                    }
                }

                if (current == null)
                    break;

                if (current == endVertex)
                    break;

                unvisited.Remove(current);

                if (!adjList.ContainsKey(current))
                    continue;

                foreach (NPCWay way in adjList[current])
                {
                    NPCAttractionPoint next = way.To;

                    if (!unvisited.Contains(next))
                        continue;

                    float newDist = distances[current] + way.Distance;

                    if (newDist < distances[next])
                    {
                        distances[next] = newDist;
                        previousEdge[next] = way;
                        previousVertex[next] = current;
                    }
                }
            }

            List<NPCWay> route = new();
            NPCAttractionPoint currentVertex = endVertex;

            while (previousEdge.ContainsKey(currentVertex))
            {
                NPCWay way = previousEdge[currentVertex];
                route.Insert(0, way);
                currentVertex = previousVertex[currentVertex];
            }

            return route;
        }
    }
}