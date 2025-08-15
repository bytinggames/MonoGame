using System;

namespace Microsoft.Xna.Framework.Input
{
    public struct GamePadSensor
    {
        public Vector3 Gyro { get; private set; }
        public Vector3 Acceleration { get; private set; }

        public GamePadSensor(Vector3 gyro, Vector3 acceleration)
        {
            Gyro = gyro;
            Acceleration = acceleration;
        }

        public static bool operator ==(GamePadSensor left, GamePadSensor right)
        {
            return (left.Gyro == right.Gyro)
                && (left.Acceleration == right.Acceleration);
        }

        public static bool operator !=(GamePadSensor left, GamePadSensor right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return (obj is GamePadSensor) && (this == (GamePadSensor)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Gyro.GetHashCode(), Acceleration.GetHashCode());
        }

        public override string ToString()
        {
            return "Gyro=" + Gyro + " Acceleration=" + Acceleration;
        }
    }
}
