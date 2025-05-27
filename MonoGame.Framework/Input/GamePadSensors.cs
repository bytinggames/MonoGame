using System;

namespace Microsoft.Xna.Framework.Input
{
    public struct GamePadSensors(GamePadSensor main, GamePadSensor left, GamePadSensor right)
    {
        /// <summary>Default</summary>
        public GamePadSensor Main { get; private set; } = main;
        /// <summary>For joy cons f.ex.</summary>
        public GamePadSensor Left { get; private set; } = left;
        /// <summary>For joy cons f.ex.</summary>
        public GamePadSensor Right { get; private set; } = right;

        public static bool operator ==(GamePadSensors left, GamePadSensors right)
        {
            return (left.Main == right.Main)
                && (left.Left == right.Left)
                && (left.Right == right.Right);
        }

        public static bool operator !=(GamePadSensors left, GamePadSensors right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return (obj is GamePadSensors g) && (this == g);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Main.GetHashCode(), Left.GetHashCode(), Right.GetHashCode());
        }

        public override string ToString()
        {
            return $"Main={Main} Left={Left} Right={Right}";
        }
    }
}
