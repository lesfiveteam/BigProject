using System.Collections.Generic;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class BIDDFS : PathfindingAlgorithm
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

            for (int depth = 0; depth < int.MaxValue; depth++)
            {
                Dictionary<NPCAttractionPoint, NPCWay> cameFromForward = new() 
                { [startVertex] = null };
                Dictionary<NPCAttractionPoint, NPCWay> cameFromBackward = new() 
                { [endVertex] = null };
                HashSet<NPCAttractionPoint> visitedForward = new() 
                { startVertex };
                HashSet<NPCAttractionPoint> visitedBackward = new() 
                { endVertex };

                DFSLimited(adjList, startVertex, depth, cameFromForward, visitedForward);
                DFSLimited(adjList, endVertex, depth, cameFromBackward, visitedBackward);

                NPCAttractionPoint meetingPoint = FindIntersection(visitedForward, visitedBackward);

                if (meetingPoint != null)
                {
                    return ReconstructBidirectionalPath(cameFromForward, cameFromBackward,
                                                         meetingPoint, startVertex, endVertex);
                }
            }

            return new List<NPCWay>();
        }

        private void DFSLimited(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            NPCAttractionPoint current,
            int depthLimit,
            Dictionary<NPCAttractionPoint, NPCWay> cameFrom,
            HashSet<NPCAttractionPoint> visited)
        {
            if (depthLimit == 0 || !adjList.ContainsKey(current))
                return;

            foreach (NPCWay way in adjList[current])
            {
                NPCAttractionPoint next = way.To;

                if (visited.Contains(next))
                    continue;

                visited.Add(next);
                cameFrom[next] = way;

                DFSLimited(adjList, next, depthLimit - 1, cameFrom, visited);
            }
        }

        private NPCAttractionPoint FindIntersection(
            HashSet<NPCAttractionPoint> visitedForward,
            HashSet<NPCAttractionPoint> visitedBackward)
        {
            foreach (NPCAttractionPoint vertex in visitedForward)
            {
                if (visitedBackward.Contains(vertex))
                    return vertex;
            }

            return null;
        }
    }
}