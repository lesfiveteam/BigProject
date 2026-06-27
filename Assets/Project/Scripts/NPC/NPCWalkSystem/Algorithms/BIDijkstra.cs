using System.Collections.Generic;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class BIDijkstra : PathfindingAlgorithm
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

            Dictionary<NPCAttractionPoint, float> gScoreForward = new();
            Dictionary<NPCAttractionPoint, NPCWay> cameFromForward = new();
            PriorityQueue<NPCAttractionPoint> openSetForward = new();

            Dictionary<NPCAttractionPoint, float> gScoreBackward = new();
            Dictionary<NPCAttractionPoint, NPCWay> cameFromBackward = new();
            PriorityQueue<NPCAttractionPoint> openSetBackward = new();

            foreach (NPCAttractionPoint vertex in adjList.Keys)
            {
                gScoreForward[vertex] = float.MaxValue;
                gScoreBackward[vertex] = float.MaxValue;
            }

            gScoreForward[startVertex] = 0f;
            gScoreBackward[endVertex] = 0f;
            openSetForward.Enqueue(startVertex, 0f);
            openSetBackward.Enqueue(endVertex, 0f);

            NPCAttractionPoint meetingPoint = null;
            float bestPathCost = float.MaxValue;

            while (openSetForward.Count > 0 && openSetBackward.Count > 0)
            {
                ExpandBidirectional(adjList, openSetForward, gScoreForward, gScoreBackward,
                                    cameFromForward, ref meetingPoint, ref bestPathCost);

                ExpandBidirectional(adjList, openSetBackward, gScoreBackward, gScoreForward,
                                    cameFromBackward, ref meetingPoint, ref bestPathCost);

                float minForward = openSetForward.PeekPriority();
                float minBackward = openSetBackward.PeekPriority();

                if (minForward + minBackward >= bestPathCost)
                    break;
            }

            if (meetingPoint == null)
                return new List<NPCWay>();

            return ReconstructBidirectionalPath(cameFromForward, cameFromBackward,
                                                 meetingPoint, startVertex, endVertex);
        }

        private void ExpandBidirectional(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            PriorityQueue<NPCAttractionPoint> openSet,
            Dictionary<NPCAttractionPoint, float> gScoreCurrent,
            Dictionary<NPCAttractionPoint, float> gScoreOther,
            Dictionary<NPCAttractionPoint, NPCWay> cameFrom,
            ref NPCAttractionPoint meetingPoint,
            ref float bestPathCost)
        {
            if (openSet.Count == 0)
                return;

            NPCAttractionPoint current = openSet.Dequeue();
            float currentG = gScoreCurrent[current];

            if (gScoreOther[current] < float.MaxValue)
            {
                float totalCost = currentG + gScoreOther[current];

                if (totalCost < bestPathCost)
                {
                    bestPathCost = totalCost;
                    meetingPoint = current;
                }
            }

            if (!adjList.ContainsKey(current))
                return;

            foreach (NPCWay way in adjList[current])
            {
                NPCAttractionPoint next = way.To;
                float newG = currentG + way.Distance;

                if (newG < gScoreCurrent[next])
                {
                    gScoreCurrent[next] = newG;
                    cameFrom[next] = way;
                    openSet.Enqueue(next, newG);
                }

                if (gScoreOther[next] < float.MaxValue)
                {
                    float totalCost = newG + gScoreOther[next];

                    if (totalCost < bestPathCost)
                    {
                        bestPathCost = totalCost;
                        meetingPoint = next;
                    }
                }
            }
        }
    }
}