// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	// Crops unused space from around the edge of a glyph bitmap.
	public static class GlyphCropper
	{
		public static void Crop(GlyphData glyph)
		{
			// Crop the top.
			while ((glyph.Subrect.Height > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.Subrect.X, glyph.Subrect.Y, glyph.Subrect.Width, 1)))
			{
				glyph.Subrect.Y++;
				glyph.Subrect.Height--;

				glyph.YOffset++;
			}

			// Crop the bottom.
			while ((glyph.Subrect.Height > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.Subrect.X, glyph.Subrect.Bottom - 1, glyph.Subrect.Width, 1)))
			{
				glyph.Subrect.Height--;
			}

			// Crop the left.
			while ((glyph.Subrect.Width > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.Subrect.X, glyph.Subrect.Y, 1, glyph.Subrect.Height)))
			{
				glyph.Subrect.X++;
				glyph.Subrect.Width--;

				glyph.XOffset++;
			}

			// Crop the right.
			while ((glyph.Subrect.Width > 1) && BitmapUtils.IsAlphaEntirely(0, glyph.Bitmap, new Rectangle(glyph.Subrect.Right - 1, glyph.Subrect.Y, 1, glyph.Subrect.Height)))
			{
				glyph.Subrect.Width--;

				glyph.XAdvance++;
			}
        }

        public static void Enlarge(GlyphData glyph, int amount)
        {
            // disabled, cause it made coordinates go below 0 which threw an exception
            //glyph.Subrect.X -= amount;
            //glyph.Subrect.Y -= amount;
            glyph.Subrect.Width += amount * 2;
            glyph.Subrect.Height += amount * 2;
            glyph.XOffset -= amount; // not sure if this is necessary or even does anything
            glyph.YOffset -= amount; // not sure if this is necessary or even does anything
            glyph.XAdvance -= amount; // not sure if this is necessary or even does anything
        }
    }

}
