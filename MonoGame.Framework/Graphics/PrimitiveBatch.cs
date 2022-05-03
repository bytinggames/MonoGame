// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Helper class for drawing primitives in one or more optimized batches.
    /// This was an experiment to draw triangles and polygons with a spritebatch.
    /// It works, but with the cost of being more memory heavy and half as fast when only drawing quads (this is where the SpriteBatch is good at).
    /// Probably don't use this class at all. I implemented Triangle and Polygon draw methods to the SpriteBatch class. The performance is totally fine.
    /// One Triangle draw call costs no more than a sprite draw call. And a polygon draw costs ((vertexCount - 2) / 2) sprite calls (probably a bit less).
    /// If you really need to draw extremely complex polygons, render them to a texture and render that texture then.
    /// This class will stay for reference purpose. But I will mark it internal.
    /// </summary>
	internal class PrimitiveBatch : GraphicsResource
	{
        #region Private Fields
        readonly PrimitiveBatcher _batcher;

		SpriteSortMode _sortMode;
		BlendState _blendState;
		SamplerState _samplerState;
		DepthStencilState _depthStencilState; 
		RasterizerState _rasterizerState;		
		Effect _effect;
        bool _beginCalled;

		SpriteEffect _spriteEffect;
        readonly EffectPass _spritePass;

		Rectangle _tempRect = new Rectangle (0,0,0,0);
		Vector2 _texCoordTL = new Vector2 (0,0);
		Vector2 _texCoordBR = new Vector2 (0,0);
        #endregion

        /// <summary>
        /// Constructs a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>        
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
        public PrimitiveBatch(GraphicsDevice graphicsDevice) : this(graphicsDevice, 0)
        {            
        }

        /// <summary>
        /// Constructs a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>
        /// <param name="capacity">The initial capacity of the internal array holding batch items (the value will be rounded to the next multiple of 64).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
        public PrimitiveBatch(GraphicsDevice graphicsDevice, int capacity)
		{
			if (graphicsDevice == null)
            {
				throw new ArgumentNullException ("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
			}	

			this.GraphicsDevice = graphicsDevice;

            _spriteEffect = new SpriteEffect(graphicsDevice);
            _spritePass = _spriteEffect.CurrentTechnique.Passes[0];

            _batcher = new PrimitiveBatcher(graphicsDevice, capacity);

            _beginCalled = false;
		}

        /// <summary>
        /// Begins a new sprite and text batch with the specified render state.
        /// </summary>
        /// <param name="sortMode">The drawing order for sprite and text drawing. <see cref="SpriteSortMode.Deferred"/> by default.</param>
        /// <param name="blendState">State of the blending. Uses <see cref="BlendState.AlphaBlend"/> if null.</param>
        /// <param name="samplerState">State of the sampler. Uses <see cref="SamplerState.LinearClamp"/> if null.</param>
        /// <param name="depthStencilState">State of the depth-stencil buffer. Uses <see cref="DepthStencilState.None"/> if null.</param>
        /// <param name="rasterizerState">State of the rasterization. Uses <see cref="RasterizerState.CullCounterClockwise"/> if null.</param>
        /// <param name="effect">A custom <see cref="Effect"/> to override the default sprite effect. Uses default sprite effect if null.</param>
        /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix.Identity"/> if null.</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> is called next time without previous <see cref="End"/>.</exception>
        /// <remarks>This method uses optional parameters.</remarks>
        /// <remarks>The <see cref="Begin"/> Begin should be called before drawing commands, and you cannot call it again before subsequent <see cref="End"/>.</remarks>
        public void Begin
        (
             SpriteSortMode sortMode = SpriteSortMode.Deferred,
             BlendState blendState = null,
             SamplerState samplerState = null,
             DepthStencilState depthStencilState = null,
             RasterizerState rasterizerState = null,
             Effect effect = null,
             Matrix? transformMatrix = null
        )
        {
            if (_beginCalled)
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");

            // defaults
            _sortMode = sortMode;
            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            _effect = effect;
            _spriteEffect.TransformMatrix = transformMatrix;

            // Setup things now so a user can change them.
            if (sortMode == SpriteSortMode.Immediate)
            {
                Setup();
            }

            _beginCalled = true;
        }

        /// <summary>
        /// Flushes all batched text and sprites to the screen.
        /// </summary>
        /// <remarks>This command should be called after <see cref="Begin"/> and drawing commands.</remarks>
		public void End ()
		{
            if (!_beginCalled)
                throw new InvalidOperationException("Begin must be called before calling End.");

			_beginCalled = false;

			if (_sortMode != SpriteSortMode.Immediate)
				Setup();
            
            _batcher.DrawBatch(_sortMode, _effect);
        }
		
		void Setup() 
        {
            var gd = GraphicsDevice;
			gd.BlendState = _blendState;
			gd.DepthStencilState = _depthStencilState;
			gd.RasterizerState = _rasterizerState;
			gd.SamplerStates[0] = _samplerState;

            _spritePass.Apply();
		}
		
        void CheckValid(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            if (!_beginCalled)
                throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
        }

		// Mark the end of a draw operation for Immediate SpriteSortMode.
		internal void FlushIfNeeded()
		{
			if (_sortMode == SpriteSortMode.Immediate)
			{
				_batcher.DrawBatch(_sortMode, _effect);
			}
		}

        public void DrawTriangle(Texture2D texture, Vector2 tl, Vector2 tr, Vector2 bl, Color color, float depth = 0f)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem(3, 3);
            item.Texture = texture;
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : depth;
            short localVertexIndex = 0;
            _batcher.LocalIndices[_batcher.CurrentIndexIndex++] = localVertexIndex++;
            _batcher.LocalIndices[_batcher.CurrentIndexIndex++] = localVertexIndex++;
            _batcher.LocalIndices[_batcher.CurrentIndexIndex++] = localVertexIndex++;
            _batcher.Vertices[_batcher.CurrentVertexIndex++] = new VertexPositionColorTexture(new Vector3(tl, depth), color, Vector2.Zero);
            _batcher.Vertices[_batcher.CurrentVertexIndex++] = new VertexPositionColorTexture(new Vector3(tr, depth), color, Vector2.Zero);
            _batcher.Vertices[_batcher.CurrentVertexIndex++] = new VertexPositionColorTexture(new Vector3(bl, depth), color, Vector2.Zero);

            FlushIfNeeded();

        }

        public void DrawRectangle(Texture2D texture, Rectangle rectangle, Color color, float depth = 0f)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem(6, 4);
            item.Texture = texture;
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : depth;

            _batcher.LocalIndices[_batcher.CurrentIndexIndex] = 0;
            _batcher.LocalIndices[_batcher.CurrentIndexIndex + 1] = 1;
            _batcher.LocalIndices[_batcher.CurrentIndexIndex + 2] = 2;
            _batcher.LocalIndices[_batcher.CurrentIndexIndex + 3] = 2;
            _batcher.LocalIndices[_batcher.CurrentIndexIndex + 4] = 1;
            _batcher.LocalIndices[_batcher.CurrentIndexIndex + 5] = 3;
            _batcher.CurrentIndexIndex += 6;

            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.X = rectangle.Left;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.Y = rectangle.Top;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.Z = depth;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Color = color;
            _batcher.Vertices[_batcher.CurrentVertexIndex].TextureCoordinate.X = 0f;
            _batcher.Vertices[_batcher.CurrentVertexIndex].TextureCoordinate.Y = 0f;
            _batcher.CurrentVertexIndex++;

            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.X = rectangle.Right;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.Y = rectangle.Top;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.Z = depth;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Color = color;
            _batcher.Vertices[_batcher.CurrentVertexIndex].TextureCoordinate.X = 0f;
            _batcher.Vertices[_batcher.CurrentVertexIndex].TextureCoordinate.Y = 0f;
            _batcher.CurrentVertexIndex++;

            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.X = rectangle.Left;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.Y = rectangle.Bottom;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.Z = depth;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Color = color;
            _batcher.Vertices[_batcher.CurrentVertexIndex].TextureCoordinate.X = 0f;
            _batcher.Vertices[_batcher.CurrentVertexIndex].TextureCoordinate.Y = 0f;
            _batcher.CurrentVertexIndex++;

            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.X = rectangle.Right;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.Y = rectangle.Bottom;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Position.Z = depth;
            _batcher.Vertices[_batcher.CurrentVertexIndex].Color = color;
            _batcher.Vertices[_batcher.CurrentVertexIndex].TextureCoordinate.X = 0f;
            _batcher.Vertices[_batcher.CurrentVertexIndex].TextureCoordinate.Y = 0f;
            _batcher.CurrentVertexIndex++;

            FlushIfNeeded();
        }

        public void DrawPolygon(Texture2D texture, Vector2[] vertices, Color color, float depth = 0f)
        {
            if (vertices.Length < 3)
                return;

            CheckValid(texture);

            short indexCount = (short)((vertices.Length - 2) * 3);
            var item = _batcher.CreateBatchItem(indexCount, (short)vertices.Length);
            item.Texture = texture;
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : depth;

            for (int i = 0; i < vertices.Length; i++)
            {
                _batcher.Vertices[_batcher.CurrentVertexIndex++] = new VertexPositionColorTexture(new Vector3(vertices[i], depth), color, Vector2.Zero);
            }


            short localVertexIndex = 1;
            for (int i = 2; i < vertices.Length; i++)
            {
                _batcher.LocalIndices[_batcher.CurrentIndexIndex++] = 0;
                _batcher.LocalIndices[_batcher.CurrentIndexIndex++] = localVertexIndex;
                _batcher.LocalIndices[_batcher.CurrentIndexIndex++] = (short)(localVertexIndex++ + 1);
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_spriteEffect != null)
                    {
                        _spriteEffect.Dispose();
                        _spriteEffect = null;
                    }
                }
            }
            base.Dispose(disposing);
        }
	}
}

