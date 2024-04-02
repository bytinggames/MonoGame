// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public class GraphicsContextSettings
    {
        public ContextProfileMask ProfileMask { get; set; } = ContextProfileMask.Undefined; // was MonoGame's default
        public int MajorVersion { get; set; } = 2; // was default MonoGame OpenGL Context Major Version
        public int MinorVersion { get; set; } = 1; // was default MonoGame OpenGL Context Minor Version
    }
}

