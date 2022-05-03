// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// This class handles the queueing of batch items into the GPU by creating the triangle tesselations
    /// that are used to draw the sprite textures. This class supports int.MaxValue number of sprites to be
    /// batched and will process them into short.MaxValue groups (strided by 6 for the number of vertices
    /// sent to the GPU). 
    /// </summary>
	internal class PrimitiveBatcher
	{
        /*
         * Note that this class is fundamental to high performance for SpriteBatch games. Please exercise
         * caution when making changes to this class.
         */

        /// <summary>
        /// Initialization size for the batch item list and queue.
        /// </summary>
        private const int InitialBatchSize = 256;
        /// <summary>
        /// The maximum number of batch items that can be processed per iteration
        /// </summary>
        private const int MaxIndices = short.MaxValue;
        /// <summary>
        /// Initialization size for the vertex array, in batch units.
        /// </summary>
		private const int InitialVertexArraySize = 256;

        /// <summary>
        /// The list of batch items to process.
        /// </summary>
	    private PrimitiveBatchItem[] _batchItemList;
        /// <summary>
        /// The list of batch items to process.
        /// </summary>
	    //private List<SpriteBatchItemReference> _batchItemReferenceList = new List<SpriteBatchItemReference>();
        /// <summary>
        /// Index pointer to the next available SpriteBatchItem in _batchItemList.
        /// </summary>
        private int _batchItemCount;
        
        /// <summary>
        /// The target graphics device.
        /// </summary>
        private readonly GraphicsDevice _device;

        /// <summary>
        /// Vertex index array. The values in this array never change.
        /// </summary>
        public short[] LocalIndices = new short[0];

        public VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[0];
        public int CurrentVertexIndex { get; set; }
        public int CurrentIndexIndex { get; set; }

        public VertexPositionColorTexture[] VerticesToRender = new VertexPositionColorTexture[0];
        public short[] IndicesToRender = new short[0];

        public PrimitiveBatcher(GraphicsDevice device, int capacity = 0)
		{
            _device = device;

            if (capacity <= 0)
                capacity = InitialBatchSize;
            else
                capacity = (capacity + 63) & (~63); // ensure chunks of 64.

            _batchItemList = new PrimitiveBatchItem[capacity];
            _batchItemCount = 0;

            for (int i = 0; i < capacity; i++)
                _batchItemList[i] = new PrimitiveBatchItem();

            EnsureIndexArrayCapacity(capacity);
            EnsureVertexArrayCapacity(capacity);
		}

        /// <summary>
        /// Reuse a previously allocated SpriteBatchItem from the item pool. 
        /// if there is none available grow the pool and initialize new items.
        /// </summary>
        /// <returns></returns>
        public PrimitiveBatchItem CreateBatchItem(short indexCount, short vertexCount)
        {
            if (_batchItemCount >= _batchItemList.Length)
            {
                var oldSize = _batchItemList.Length;
                var newSize = oldSize + oldSize / 2; // grow by x1.5
                newSize = (newSize + 63) & (~63); // grow in chunks of 64.
                Array.Resize(ref _batchItemList, newSize);
                for (int i = oldSize; i < newSize; i++)
                    _batchItemList[i] = new PrimitiveBatchItem();
            }

            if (CurrentIndexIndex + indexCount - 1 >= LocalIndices.Length)
            {
                var oldSize = LocalIndices.Length;
                var newSize = Math.Max(oldSize + oldSize / 2, CurrentIndexIndex + indexCount); // grow by x1.5
                newSize = (newSize + 255) & (~255); // grow in chunks of 256.
                EnsureIndexArrayCapacity(newSize);
            }
            if (CurrentVertexIndex + vertexCount - 1 >= Vertices.Length)
            {
                var oldSize = Vertices.Length;
                var newSize = Math.Max(oldSize + oldSize / 2, CurrentVertexIndex + vertexCount); // grow by x1.5
                newSize = (newSize + 127) & (~127); // grow in chunks of 128.
                EnsureVertexArrayCapacity(newSize);
            }

            var item = _batchItemList[_batchItemCount++];
            item.IndexCount = indexCount;
            item.VertexCount = vertexCount;
            item.FirstVertexIndex = CurrentVertexIndex;
            item.FirstIndexIndex = CurrentIndexIndex;
            return item;
        }

        //internal void AddBatchItem(SpriteBatchItemReference spriteBatchItem)
        //{
        //    _batchItemReferenceList.Add(spriteBatchItem);
        //}

        /// <summary>
        /// Resize and recreate the missing indices for the index and vertex position color buffers.
        /// </summary>
        /// <param name="numBatchItems"></param>
        private unsafe void EnsureIndexArrayCapacity(int indexCount)
        {
            if (LocalIndices != null && indexCount <= LocalIndices.Length)
            {
                // Short circuit out of here because we have enough capacity.
                return;
            }
            Array.Resize(ref LocalIndices, indexCount);
            Array.Resize(ref IndicesToRender, indexCount);
        }
        private unsafe void EnsureVertexArrayCapacity(int vertexCount)
        {
            if (Vertices != null && vertexCount <= Vertices.Length)
            {
                // Short circuit out of here because we have enough capacity.
                return;
            }
            Array.Resize(ref Vertices, vertexCount);
            Array.Resize(ref VerticesToRender, vertexCount);
        }

        /// <summary>
        /// Sorts the batch items and then groups batch drawing into maximal allowed batch sets that do not
        /// overflow the 16 bit array indices for vertices.
        /// </summary>
        /// <param name="sortMode">The type of depth sorting desired for the rendering.</param>
        /// <param name="effect">The custom effect to apply to the drawn geometry</param>
        public unsafe void DrawBatch(SpriteSortMode sortMode, Effect effect)
        {
            if (effect != null && effect.IsDisposed)
                throw new ObjectDisposedException("effect");

            // nothing to do
            if (_batchItemCount == 0)
                return;

            // sort the batch items
            switch (sortMode)
            {
                case SpriteSortMode.Texture:
                case SpriteSortMode.FrontToBack:
                case SpriteSortMode.BackToFront:
                    Array.Sort(_batchItemList, 0, _batchItemCount);
                    break;
            }

            // Determine how many iterations through the drawing code we need to make
            int batchCount = _batchItemCount;


            unchecked
            {
                _device._graphicsMetrics._spriteCount += batchCount;
            }

            // setup the vertexArray array
            Texture2D tex = null;

            int vertexIndex = 0;
            int indexIndex = 0;

            // Avoid the array checking overhead by using pointer indexing!
            fixed (VertexPositionColorTexture* vertexArrayFixedPtr = VerticesToRender)
            fixed (short* indexArrayFixedPtr = IndicesToRender)
            {
                var vertexArrayPtr = vertexArrayFixedPtr;
                var indexArrayPtr = indexArrayFixedPtr;

                // Draw the batches
                for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
                {
                    PrimitiveBatchItem item = _batchItemList[batchIndex];
                    // if the texture changed, we need to flush and bind the new texture
                    var shouldFlush = !ReferenceEquals(item.Texture, tex)
                        || indexIndex + item.IndexCount > MaxIndices;
                    if (shouldFlush)
                    {
                        FlushVertexArray(indexIndex, vertexIndex, effect, tex);

                        tex = item.Texture;
                        vertexArrayPtr = vertexArrayFixedPtr;
                        indexArrayPtr = indexArrayFixedPtr;
                        vertexIndex = indexIndex = 0;
                        _device.Textures[0] = tex;
                    }

                    // store the SpriteBatchItem data in our vertexArray
                    for (int j = 0; j < item.IndexCount; j++)
                        *(indexArrayPtr++) = (short)(vertexIndex + LocalIndices[item.FirstIndexIndex + j]);
                    for (int j = 0; j < item.VertexCount; j++)
                        *(vertexArrayPtr++) = Vertices[item.FirstVertexIndex + j];

                    vertexIndex += item.VertexCount;
                    indexIndex += item.IndexCount;

                    // Release the texture.
                    item.Texture = null;
                }

                // flush the remaining vertexArray data
                FlushVertexArray(indexIndex, vertexIndex, effect, tex);
            }

            // return items to the pool.  
            _batchItemCount = 0;
            CurrentIndexIndex = 0;
            CurrentVertexIndex = 0;

            //_batchItemReferenceList.Clear();

        }

        /// <summary>
        /// Sends the triangle list to the graphics device. Here is where the actual drawing starts.
        /// </summary>
        /// <param name="start">Start index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
        /// <param name="end">End index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
        /// <param name="effect">The custom effect to apply to the geometry</param>
        /// <param name="texture">The texture to draw.</param>
        private void FlushVertexArray(int indexCount, int vertexCount, Effect effect, Texture texture)
        {
            if (indexCount == 0)
                return;

            // If the effect is not null, then apply each pass and render the geometry
            if (effect != null)
            {
                var passes = effect.CurrentTechnique.Passes;
                foreach (var pass in passes)
                {
                    pass.Apply();

                    // Whatever happens in pass.Apply, make sure the texture being drawn
                    // ends up in Textures[0].
                    _device.Textures[0] = texture;

                    _device.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        VerticesToRender,
                        0,
                        vertexCount,
                        IndicesToRender,
                        0,
                        indexCount / 3,
                        VertexPositionColorTexture.VertexDeclaration);
                }
            }
            else
            {
                // If no custom effect is defined, then simply render.
                _device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    VerticesToRender,
                    0,
                    vertexCount,
                    IndicesToRender,
                    0,
                    indexCount / 3,
                    VertexPositionColorTexture.VertexDeclaration);
            }
        }
    }
}

