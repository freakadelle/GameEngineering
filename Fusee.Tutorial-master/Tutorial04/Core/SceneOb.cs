using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Fusee.Engine.Core;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core
{
    class SceneOb
    {

        public string ob_name;
        public List<SceneOb> Children;

        public Mesh Mesh;
        public float3 Albedo = new float3(0.8f, 0.8f, 0.8f);
        public float3 Pos = float3.Zero;
        public float3 Rot = float3.Zero;
        public float3x3 Rotbounds = float3x3.Zero;
        public float4 target = float4.Zero;
        public float speed = 0;
        public float3 Pivot = float3.Zero;
        public float3 Scale = float3.One;
        public float3 ModelScale = float3.One;

        public void adjustBoundsOfRotation()
        {
            Rot.x = System.Math.Max(Rot.x, Rotbounds.M11);
            Rot.x = System.Math.Min(Rot.x, Rotbounds.M12);
            Rot.y = System.Math.Max(Rot.y, Rotbounds.M21);
            Rot.y = System.Math.Min(Rot.y, Rotbounds.M22);
            Rot.z = System.Math.Max(Rot.z, Rotbounds.M31);
            Rot.z = System.Math.Min(Rot.z, Rotbounds.M32);
        }

        public void update()
        {
            Rot.x += target.x * speed / 1000.0f;
            Rot.y += target.y * speed / 1000.0f;
            Rot.z += target.z * speed / 1000.0f;
            target.w--;
        }

        public void rndNewTarget(Random rnd)
        {
            Debug.WriteLine(rnd.NextDouble());
            target.x = ((float)(rnd.NextDouble() * 2.0f) - 1.0f);
            target.y = ((float)(rnd.NextDouble() * 2.0f) - 1.0f);
            target.z = ((float)(rnd.NextDouble() * 2.0f) - 1.0f);
            target.w = rnd.Next(50, 200);

            speed = rnd.Next(20, 100);
        }

        public Boolean hasReachedTarget()
        {
            if (target.w <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
