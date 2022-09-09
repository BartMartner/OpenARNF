using UnityEngine;
using System.Collections;

namespace CreativeSpore.SuperTilemapEditor
{

    public static class BrushUtil
    {
        public static Vector2 GetSnappedPosition(Vector2 position, Vector2 cellSize)
        {
            Vector2 centerCell = position - cellSize / 2f;
            Vector2 snappedPos = new Vector2
            (
                Mathf.Round(centerCell.x / cellSize.x) * cellSize.x,
                Mathf.Round(centerCell.y / cellSize.y) * cellSize.y
            );
            return snappedPos;
        }

        /// <summary>
        /// Get the grid X position for a given position. 
        /// Avoid using positions multiple of cellSize like 0.32f if cellSize = 0.16f because due float imprecisions the return value could be wrong.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="cellSize"></param>
        /// <returns></returns>
        public static int GetGridX(Vector2 position, Vector2 cellSize)
        {
            return Mathf.FloorToInt((position.x + Vector2.kEpsilon) / cellSize.x);
        }

        /// <summary>
        /// Get the grid Y position for a given position. 
        /// Avoid using positions multiple of cellSize like 0.32f if cellSize = 0.16f because due float imprecisions the return value could be wrong.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="cellSize"></param>
        /// <returns></returns>
        public static int GetGridY(Vector2 position, Vector2 cellSize)
        {
            return Mathf.FloorToInt((position.y + Vector2.kEpsilon) / cellSize.y);
        }
    }
}