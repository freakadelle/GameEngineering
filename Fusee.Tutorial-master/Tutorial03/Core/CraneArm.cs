using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{

    public class CraneArm
    {
        public float yaw;
        public float pitch;
        public float speed;

        public float3 target;

        public float4x4 xForm;
        public float3 position;
        public float3 scaling;
        public Mesh mesh;

        public CraneArm()
        {
            mesh = new Mesh
            {
                Vertices = new[]
                {
                    // left, down, front vertex
                    new float3(-1f, -1f, -1f), // 0  - belongs to left
                    new float3(-1f, -1f, -1f), // 1  - belongs to down
                    new float3(-1f, -1f, -1f), // 2  - belongs to front

                    // left, down, back vertex
                    new float3(-1f, -1f,  1f),  // 3  - belongs to left
                    new float3(-1f, -1f,  1f),  // 4  - belongs to down
                    new float3(-1f, -1f,  1f),  // 5  - belongs to back

                    // left, up, front vertex
                    new float3(-1f,  1f, -1f),  // 6  - belongs to left
                    new float3(-1f,  1f, -1f),  // 7  - belongs to up
                    new float3(-1f,  1f, -1f),  // 8  - belongs to front

                    // left, up, back vertex
                    new float3(-1f,  1f,  1f),  // 9  - belongs to left
                    new float3(-1f,  1f,  1f),  // 10 - belongs to up
                    new float3(-1f,  1f,  1f),  // 11 - belongs to back

                    // right, down, front vertex
                    new float3( 1f, -1f, -1f), // 12 - belongs to right
                    new float3( 1f, -1f, -1f), // 13 - belongs to down
                    new float3( 1f, -1f, -1f), // 14 - belongs to front

                    // right, down, back vertex
                    new float3( 1f, -1f,  1f),  // 15 - belongs to right
                    new float3( 1f, -1f,  1f),  // 16 - belongs to down
                    new float3( 1f, -1f,  1f),  // 17 - belongs to back

                    // right, up, front vertex
                    new float3( 1f,  1f, -1f),  // 18 - belongs to right
                    new float3( 1f,  1f, -1f),  // 19 - belongs to up
                    new float3( 1f,  1f, -1f),  // 20 - belongs to front

                    // right, up, back vertex
                    new float3( 1f,  1f,  1f),  // 21 - belongs to right
                    new float3( 1f,  1f,  1f),  // 22 - belongs to up
                    new float3( 1f,  1f,  1f),  // 23 - belongs to back

                },
                Normals = new[]
                {
                    // left, down, front vertex
                    new float3(-1,  0,  0), // 0  - belongs to left
                    new float3( 0, -1,  0), // 1  - belongs to down
                    new float3( 0,  0, -1), // 2  - belongs to front

                    // left, down, back vertex
                    new float3(-1,  0,  0),  // 3  - belongs to left
                    new float3( 0, -1,  0),  // 4  - belongs to down
                    new float3( 0,  0,  1),  // 5  - belongs to back

                    // left, up, front vertex
                    new float3(-1,  0,  0),  // 6  - belongs to left
                    new float3( 0,  1,  0),  // 7  - belongs to up
                    new float3( 0,  0, -1),  // 8  - belongs to front

                    // left, up, back vertex
                    new float3(-1,  0,  0),  // 9  - belongs to left
                    new float3( 0,  1,  0),  // 10 - belongs to up
                    new float3( 0,  0,  1),  // 11 - belongs to back

                    // right, down, front vertex
                    new float3( 1,  0,  0), // 12 - belongs to right
                    new float3( 0, -1,  0), // 13 - belongs to down
                    new float3( 0,  0, -1), // 14 - belongs to front

                    // right, down, back vertex
                    new float3( 1,  0,  0),  // 15 - belongs to right
                    new float3( 0, -1,  0),  // 16 - belongs to down
                    new float3( 0,  0,  1),  // 17 - belongs to back

                    // right, up, front vertex
                    new float3( 1,  0,  0),  // 18 - belongs to right
                    new float3( 0,  1,  0),  // 19 - belongs to up
                    new float3( 0,  0, -1),  // 20 - belongs to front

                    // right, up, back vertex
                    new float3( 1,  0,  0),  // 21 - belongs to right
                    new float3( 0,  1,  0),  // 22 - belongs to up
                    new float3( 0,  0,  1),  // 23 - belongs to back
                },
                Triangles = new ushort[]
                {
                   0,  6,  3,     3,  6,  9, // left
                   2, 14, 20,     2, 20,  8, // front
                  12, 15, 18,    15, 21, 18, // right
                   5, 11, 17,    17, 11, 23, // back
                   7, 22, 10,     7, 19, 22, // top
                   1,  4, 16,     1, 16, 13, // bottom 
                },
            };

            position = new float3(0, 0, 0);
            scaling = new float3(1, 1, 1);
            xForm = float4x4.Identity;

            speed = 100;
            yaw = 0;
            pitch = 0;
            target = new float3(0,0, 0);
        }

        public float3 getSize()
        {
            return scaling;
        }

        public float4x4 getScalingMat()
        {
            return float4x4.CreateScale(scaling);
        }

        public void rotateAroundPivot(float3 rot, float3 pivot)
        {
            xForm =  float4x4.CreateTranslation(position + pivot)
                   * float4x4.CreateRotationY(rot.y)
                   * float4x4.CreateRotationX(rot.x)
                   * float4x4.CreateRotationZ(rot.z)
                   * float4x4.CreateTranslation(-pivot);
        }

        public void update()
        {
            pitch += (target.x / 8000.0f) * speed;
            yaw += (target.y / 15000.0f) * speed;
            yaw = System.Math.Min(yaw, 0.5f);
            yaw = System.Math.Max(yaw, -0.5f);
            target.z--;
        }

        public void rndNewTarget(Random rnd)
        {
            target.x = ((float)(rnd.NextDouble() * 2.0f) - 1);
            target.y = ((float)(rnd.NextDouble() * 2.0f) - 1);
            target.z = rnd.Next(100, 200);

            speed = rnd.Next((int)System.Math.Abs(position.x) * 1, (int)System.Math.Abs(position.x) * 100);
        }

        public Boolean hasReachedTarget()
        {
            if (target.z <= 0)
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
