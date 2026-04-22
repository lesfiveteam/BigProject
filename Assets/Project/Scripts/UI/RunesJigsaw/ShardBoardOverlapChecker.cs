using UnityEngine;
using UnityEngine.UI;

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

            float xMin = corners[0].x;
            float xMax = corners[0].x;
            float yMin = corners[0].y;
            float yMax = corners[0].y;

            for (int i = 1; i < 4; i++)
            {
                if (corners[i].x < xMin) xMin = corners[i].x;
                if (corners[i].x > xMax) xMax = corners[i].x;
                if (corners[i].y < yMin) yMin = corners[i].y;
                if (corners[i].y > yMax) yMax = corners[i].y;
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
    }
}