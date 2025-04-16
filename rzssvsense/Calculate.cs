using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace rzssvsense
{
    public static class Calculate
    {
        public static Vector2 CalculateAngles(Vector3 from, Vector3 to)
        {
            float yaw;
            float pitch;

            float deltaX = to.X - from.X;
            float deltaY = to.Y - from.Y;
            yaw = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);

            float deltaZ = to.Z - from.Z;
            double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            pitch = -(float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);
            return new Vector2(yaw, pitch);
        }
        public static Vector2 WorldToScreen(ViewMatrix matrix, Vector3 pos, int width, int height)
        {
            Vector2 screenCoordinates = new Vector2();
            float screenW = (matrix.m41 * pos.X) + (matrix.m42 * pos.Y) + (matrix.m43 * pos.Z) + matrix.m44;
            if (screenW > 0.001f)
            {
                float screenX = (matrix.m11 * pos.X) + (matrix.m12 * pos.Y) + (matrix.m13 * pos.Z) + matrix.m14;
                float screenY = (matrix.m21 * pos.X) + (matrix.m22 * pos.Y) + (matrix.m23 * pos.Z) + matrix.m24;
                float camX = width / 2;
                float camY = height / 2;
                float X = camX + (camX * screenX / screenW);
                float Y = camY - (camY * screenY / screenW);
                screenCoordinates.X = X;
                screenCoordinates.Y = Y;
                return screenCoordinates;
            }
            else
            {
                return new Vector2(-99, -99);
            }
        }
    }
}
