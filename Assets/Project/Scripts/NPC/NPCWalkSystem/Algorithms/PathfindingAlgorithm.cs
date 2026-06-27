using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public abstract class PathfindingAlgorithm
    {
        public abstract List<NPCWay> FindShortestWay(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            NPCAttractionPoint startVertex,
            NPCAttractionPoint endVertex);

        protected float Heuristic(NPCAttractionPoint a, NPCAttractionPoint b) => 
            Vector3.Distance(a.Position, b.Position);

        protected List<NPCWay> ReconstructPath(
            Dictionary<NPCAttractionPoint, NPCWay> cameFromEdge,
            NPCAttractionPoint endVertex)
        {
            List<NPCWay> route = new();
            NPCAttractionPoint current = endVertex;

            while (cameFromEdge.TryGetValue(current, out NPCWay way) && way != null)
            {
                route.Add(way);
                current = way.From;
            }

            route.Reverse();

            return route;
        }

        protected List<NPCWay> ReconstructBidirectionalPath(
            Dictionary<NPCAttractionPoint, NPCWay> cameFromForward,
            Dictionary<NPCAttractionPoint, NPCWay> cameFromBackward,
            NPCAttractionPoint meetingPoint,
            NPCAttractionPoint startVertex,
            NPCAttractionPoint endVertex)
        {
            if (meetingPoint == null)
                return new List<NPCWay>();

            List<NPCWay> route = new();
            NPCAttractionPoint current = meetingPoint;

            // Forward part: meetingPoint → startVertex
            List<NPCWay> forwardPath = new();
            while (current != startVertex && cameFromForward.TryGetValue(current, out NPCWay way))
            {
                forwardPath.Add(way);
                current = way.From;
            }

            if (current != startVertex)
                return new List<NPCWay>();

            forwardPath.Reverse();
            route.AddRange(forwardPath);

            // Backward part: meetingPoint → endVertex
            current = meetingPoint;
            List<NPCWay> backwardPath = new();
            while (current != endVertex && cameFromBackward.TryGetValue(current, out NPCWay way))
            {
                // Создаем обратное ребро
                NPCWay reversedWay = way.CreateReverse();  // ← использование метода из NPCWay
                backwardPath.Add(reversedWay);
                current = way.From;
            }

            if (current != endVertex)
                return new List<NPCWay>();

            // backwardPath уже в правильном порядке
            route.AddRange(backwardPath);

            return route;
        }
    }
}