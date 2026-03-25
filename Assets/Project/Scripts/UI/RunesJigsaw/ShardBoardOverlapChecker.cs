using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace BigProject.UI
{
    public static class ShardBoardOverlapChecker
    {
        public static bool IsShardInsideBoard(Image shardImage, Image boardImage)
        {
            if (shardImage == null || boardImage == null) return false;

            Rect shardRect = GetWorldRect(shardImage.rectTransform);
            Rect boardRect = GetWorldRect(boardImage.rectTransform);

            return shardRect.xMin >= boardRect.xMin &&
                   shardRect.xMax <= boardRect.xMax &&
                   shardRect.yMin >= boardRect.yMin &&
                   shardRect.yMax <= boardRect.yMax;
        }

        private static Rect GetWorldRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            float xMin = corners.Min(c => c.x);
            float xMax = corners.Max(c => c.x);
            float yMin = corners.Min(d => d.y);
            float yMax = corners.Max(d => d.y);

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
    }
}