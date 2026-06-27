using System.Collections.Generic;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class IDDFS : PathfindingAlgorithm
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

            for (int depthLimit = 0; depthLimit < int.MaxValue; depthLimit++)
            {
                Dictionary<NPCAttractionPoint, NPCWay> cameFrom = new()
                { [startVertex] = null };

                List<NPCWay> result = DFSLimited(adjList, startVertex, endVertex, depthLimit, cameFrom);

                if (result != null)
                    return result;
            }

            return new List<NPCWay>();
        }

        private List<NPCWay> DFSLimited(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            NPCAttractionPoint current,
            NPCAttractionPoint endVertex,
            int depthLimit,
            Dictionary<NPCAttractionPoint, NPCWay> cameFrom)
        {
            if (current == endVertex)
                return ReconstructPath(cameFrom, endVertex);

            if (depthLimit == 0)
                return null;

            if (!adjList.ContainsKey(current))
                return null;

            foreach (NPCWay way in adjList[current])
            {
                NPCAttractionPoint next = way.To;

                if (cameFrom.ContainsKey(next))
                    continue;

                cameFrom[next] = way;

                List<NPCWay> result = DFSLimited(adjList, next, endVertex, depthLimit - 1, cameFrom);

                if (result != null)
                    return result;

                cameFrom.Remove(next);
            }

            return null;
        }
    }
}