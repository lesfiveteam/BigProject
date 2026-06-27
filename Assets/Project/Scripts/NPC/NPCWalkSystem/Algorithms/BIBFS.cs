using System.Collections.Generic;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class BIBFS : PathfindingAlgorithm
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

            // Forward
            Queue<NPCAttractionPoint> openSetForward = new();
            Dictionary<NPCAttractionPoint, NPCWay> cameFromForward = new()
            { [startVertex] = null };

            // Backward
            Queue<NPCAttractionPoint> openSetBackward = new();
            Dictionary<NPCAttractionPoint, NPCWay> cameFromBackward = new()
            { [endVertex] = null };

            openSetForward.Enqueue(startVertex);
            openSetBackward.Enqueue(endVertex);

            NPCAttractionPoint meetingPoint = null;

            while (openSetForward.Count > 0 && openSetBackward.Count > 0)
            {
                // Forward step
                if (ExpandBidirectional(adjList, openSetForward, cameFromForward,
                                        cameFromBackward, ref meetingPoint))
                    break;

                // Backward step
                if (ExpandBidirectional(adjList, openSetBackward, cameFromBackward,
                                        cameFromForward, ref meetingPoint))
                    break;
            }

            if (meetingPoint == null)
                return new List<NPCWay>();

            return ReconstructBidirectionalPath(cameFromForward, cameFromBackward,
                                                 meetingPoint, startVertex, endVertex);
        }

        private bool ExpandBidirectional(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            Queue<NPCAttractionPoint> openSet,
            Dictionary<NPCAttractionPoint, NPCWay> cameFromCurrent,
            Dictionary<NPCAttractionPoint, NPCWay> cameFromOther,
            ref NPCAttractionPoint meetingPoint)
        {
            if (openSet.Count == 0)
                return false;

            NPCAttractionPoint current = openSet.Dequeue();

            if (!adjList.ContainsKey(current))
                return false;

            foreach (NPCWay way in adjList[current])
            {
                NPCAttractionPoint next = way.To;

                if (cameFromCurrent.ContainsKey(next))
                    continue;

                cameFromCurrent[next] = way;
                openSet.Enqueue(next);

                if (cameFromOther.ContainsKey(next))
                {
                    meetingPoint = next;
                    return true;
                }
            }

            return false;
        }
    }
}