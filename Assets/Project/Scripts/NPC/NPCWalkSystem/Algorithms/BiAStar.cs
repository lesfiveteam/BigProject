using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class BiAStar : IAlgorithm
    {
        public List<NPCWay> FindShortestWay(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            NPCAttractionPoint startVertex,
            NPCAttractionPoint endVertex)
        {
            if (!adjList.ContainsKey(startVertex) || !adjList.ContainsKey(endVertex) || startVertex == endVertex)
                return new List<NPCWay>();

            // Forward search (from start)
            Dictionary<NPCAttractionPoint, float> distForward = new();
            Dictionary<NPCAttractionPoint, NPCWay> edgeForward = new();
            Dictionary<NPCAttractionPoint, NPCAttractionPoint> prevForward = new();
            PriorityQueue<NPCAttractionPoint> pqForward = new();

            // Backward search (from end)
            Dictionary<NPCAttractionPoint, float> distBackward = new();
            Dictionary<NPCAttractionPoint, NPCWay> edgeBackward = new();
            Dictionary<NPCAttractionPoint, NPCAttractionPoint> prevBackward = new();
            PriorityQueue<NPCAttractionPoint> pqBackward = new();

            // Initialize
            foreach (NPCAttractionPoint vertex in adjList.Keys)
            {
                distForward[vertex] = float.MaxValue;
                distBackward[vertex] = float.MaxValue;
            }

            distForward[startVertex] = 0f;
            distBackward[endVertex] = 0f;
            pqForward.Enqueue(startVertex, Heuristic(startVertex, endVertex));
            pqBackward.Enqueue(endVertex, Heuristic(endVertex, startVertex));

            NPCAttractionPoint meetingPoint = null;
            float bestPathCost = float.MaxValue;

            while (pqForward.Count > 0 && pqBackward.Count > 0)
            {
                // Forward step
                if (ExpandBidirectional(adjList, pqForward, distForward, distBackward,
                                        edgeForward, prevForward, endVertex,
                                        ref meetingPoint, ref bestPathCost))
                    break;

                // Backward step
                if (ExpandBidirectional(adjList, pqBackward, distBackward, distForward,
                                        edgeBackward, prevBackward, startVertex,
                                        ref meetingPoint, ref bestPathCost))
                    break;
            }

            return ReconstructBidirectionalPath(edgeForward, prevForward, edgeBackward, prevBackward,
                                                 meetingPoint, startVertex, endVertex);
        }

        private bool ExpandBidirectional(
            Dictionary<NPCAttractionPoint, List<NPCWay>> adjList,
            PriorityQueue<NPCAttractionPoint> pq,
            Dictionary<NPCAttractionPoint, float> distCurrent,
            Dictionary<NPCAttractionPoint, float> distOther,
            Dictionary<NPCAttractionPoint, NPCWay> edgeMap,
            Dictionary<NPCAttractionPoint, NPCAttractionPoint> prevMap,
            NPCAttractionPoint targetVertex,
            ref NPCAttractionPoint meetingPoint,
            ref float bestPathCost)
        {
            if (pq.Count == 0)
                return false;

            NPCAttractionPoint current = pq.Dequeue();
            float currentG = distCurrent[current];

            if (distOther[current] < float.MaxValue)
            {
                float totalCost = currentG + distOther[current];

                if (totalCost < bestPathCost)
                {
                    bestPathCost = totalCost;
                    meetingPoint = current;
                    return true;
                }
            }

            if (!adjList.ContainsKey(current))
                return false;

            foreach (NPCWay way in adjList[current])
            {
                NPCAttractionPoint next = way.To;
                float newG = currentG + way.Distance;

                if (newG < distCurrent[next])
                {
                    distCurrent[next] = newG;
                    edgeMap[next] = way;
                    prevMap[next] = current;
                    pq.Enqueue(next, newG + Heuristic(next, targetVertex));
                }

                // Check for meeting
                if (distOther[next] < float.MaxValue)
                {
                    float totalCost = newG + distOther[next];

                    if (totalCost < bestPathCost)
                    {
                        bestPathCost = totalCost;
                        meetingPoint = next;
                    }
                }
            }

            return false;
        }

        private List<NPCWay> ReconstructBidirectionalPath(
            Dictionary<NPCAttractionPoint, NPCWay> edgeForward,
            Dictionary<NPCAttractionPoint, NPCAttractionPoint> prevForward,
            Dictionary<NPCAttractionPoint, NPCWay> edgeBackward,
            Dictionary<NPCAttractionPoint, NPCAttractionPoint> prevBackward,
            NPCAttractionPoint meetingPoint,
            NPCAttractionPoint startVertex,
            NPCAttractionPoint endVertex)
        {
            if (meetingPoint == null)
                return new List<NPCWay>();

            List<NPCWay> route = new();
            NPCAttractionPoint current = meetingPoint;

            // Forward part: meetingPoint → startVertex (reverse)
            List<NPCWay> forwardPath = new();
            while (edgeForward.ContainsKey(current) && current != startVertex)
            {
                forwardPath.Insert(0, edgeForward[current]);
                current = prevForward[current];
            }
            route.AddRange(forwardPath);

            // Backward part: meetingPoint → endVertex
            current = meetingPoint;
            List<NPCWay> backwardPath = new();
            while (edgeBackward.ContainsKey(current) && current != endVertex)
            {
                backwardPath.Add(edgeBackward[current]);
                current = prevBackward[current];
            }

            // Reverse backward part
            route.AddRange(backwardPath); 

            return route;
        }

        private float Heuristic(NPCAttractionPoint a, NPCAttractionPoint b) => Vector3.Distance(a.Position, b.Position);
    }
}