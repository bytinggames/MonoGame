// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public class GraphicsContextSettings
    {
        private ContextProfileMask profileMask = ContextProfileMask.Undefined; // was MonoGame's default
        private int majorVersion = 2; // was default MonoGame OpenGL Context Major Version
        private int minorVersion = 1; // was default MonoGame OpenGL Context Minor Version

        public ContextProfileMask ProfileMask
        {
            get
            {
                return profileMask;
            }
            set
            {
                profileMask = value;
            }
        }

        public int MajorVersion
        {
            get
            {
                return majorVersion;
            }
            set
            {
                majorVersion = value;
            }
        }

        public int MinorVersion
        {
            get
            {
                return minorVersion;
            }
            set
            {
                minorVersion = value;
            }
        }
    }
}

