// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class ArrayReader<T> : ContentTypeReader<T[]>
    {
        ContentTypeReader elementReader;

        public ArrayReader()
        {
        }

        protected internal override void Initialize(ContentTypeReaderManager manager)
		{
			Type readerType = typeof(T);
			elementReader = manager.GetTypeReader(readerType);
        }

        protected internal override T[] Read(ContentReader input, T[] existingInstance)
        {
            uint count = input.ReadUInt32();
            T[] array = existingInstance;
            if (array == null)
                array = new T[count];

            if (ReflectionHelpers.IsValueType(typeof(T)))
			{
                // if the array is a byte array, read it directly as that. This is way faster.
                if (array is byte[] bytes)
                {
                    int numRead = 0;
                    do
                    {
                        int n = input.Read(bytes, numRead, (int)Math.Min(int.MaxValue, count));
                        if (n == 0)
                        {
                            break;
                        }

                        numRead += n;
                        count -= (uint)n;
                    } while (count > 0);
                }
                else
                {
                    for (uint i = 0; i < count; i++)
                    {
                        array[i] = input.ReadObject<T>(elementReader);
                    }
                }
			}
			else
			{
                for (uint i = 0; i < count; i++)
                {
                    var readerType = input.Read7BitEncodedInt();
                	array[i] = readerType > 0 ? input.ReadObject<T>(input.TypeReaders[readerType - 1]) : default(T);
                }
			}
            return array;
        }
    }
}
