using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CreativeSpore.SuperTilemapEditor
{

    public interface IBrush
    {
        /// <summary>
        /// The tileData used to show the preview tile in the tile palette
        /// </summary>
        /// <returns></returns>
        uint PreviewTileData();
        /// <summary>
        /// Called when brush is painted
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="chunkGx"></param>
        /// <param name="chunkGy"></param>
        /// <param name="tileData"></param>
        /// <returns></returns>
        uint OnPaint(TilemapChunk chunk, int chunkGx, int chunkGy, uint tileData);
        /// <summary>
        /// Called when brush is erased
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="chunkGx"></param>
        /// <param name="chunkGy"></param>
        /// <param name="tileData"></param>
        void OnErase(TilemapChunk chunk, int chunkGx, int chunkGy, uint tileData, int brushId);
        /// <summary>
        /// This is called by the tilemap chunks when a tile needs to be refreshed. Return the updated tile data.
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="tileData"></param>
        /// <returns></returns>
        uint Refresh(STETilemap tilemap, int gridX, int gridY, uint tileData);
        /// <summary>
        /// Returns if the tile should be updated for animation
        /// </summary>
        /// <returns></returns>
        bool IsAnimated();
        /// <summary>
        /// Returns the tile UV for the current frame
        /// </summary>
        /// <returns></returns>
        Rect GetAnimUV();
        /// <summary>
        /// Returns the tile UV for the current frame with flaps applied
        /// </summary>
        /// <returns></returns>
        Vector2[] GetAnimUVWithFlags(float innerPadding = 0f);
        /// <summary>
        /// Gets the current animation index
        /// </summary>
        /// <returns></returns>
        int GetAnimFrameIdx();
        /// <summary>
        /// Returns the current frame tile data
        /// </summary>
        /// <returns></returns>
        uint GetAnimTileData();
        /// <summary>
        /// If a tile is divided in 4 subtiles, this is returning an array of 4 tileData, one per each subtile, from bottom to top, from left to right
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="tileData"></param>
        /// <returns>Null if subtiles is disabled or an array of 4 tile data, one per each corner of the tile</returns>    
        uint[] GetSubtiles(STETilemap tilemap, int gridX, int gridY, uint tileData);
        /// <summary>
        /// If the brush use subtiles, this will return the merged collider vectices in pairs (v0, v1) 
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="tileData"></param>
        /// <returns></returns>
        Vector2[] GetMergedSubtileColliderVertices(STETilemap tilemap, int gridX, int gridY, uint tileData);
    }
}