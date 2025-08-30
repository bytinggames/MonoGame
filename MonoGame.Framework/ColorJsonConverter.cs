// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Xna.Framework
{
    class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new Color(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToHex());
        }

        public override Color ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new Color(reader.GetString());

        public override void WriteAsPropertyName(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
            => writer.WritePropertyName(value.ToHex());
    }
}
