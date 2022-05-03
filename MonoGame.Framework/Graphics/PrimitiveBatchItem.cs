// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class PrimitiveBatchItem : IComparable<PrimitiveBatchItem>
    {
        public Texture2D Texture;
        public float SortKey;
        public int FirstVertexIndex;
        public short VertexCount;
        public int FirstIndexIndex;
        public short IndexCount;

        #region Implement IComparable
        public int CompareTo(PrimitiveBatchItem other)
        {
            return SortKey.CompareTo(other.SortKey);
        }
        #endregion
    }
}

