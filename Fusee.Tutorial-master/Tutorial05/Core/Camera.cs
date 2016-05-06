using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Fusee.Math.Core;
using Fusee.Serialization;

namespace Fusee.Tutorial.Core
{
    class Camera: TransformComponent
    {

        private float4x4 view;
        public float3 pivotPoint;
        private float fieldOfView;
        private float speed;

        public Camera()
        {
            Translation = new float3(0,0,0);
            Rotation = new float3(0,0,0);
            Scale = new float3(0,0,0);
            pivotPoint = new float3(0,0,0);

            fieldOfView = (float) System.Math.PI * 0.25f;
            speed = 0.1f;

            view = float4x4.Zero;
        }

        public void lookAtTarget(float3 target)
        {
            float ggk = target.x;
            float ank = target.z;
            double rot = System.Math.Atan2(ggk, ank);

            Rotation.y = (float) -rot;

            ggk = target.Length;
            ank = Translation.y;
            rot = System.Math.Atan2(ggk, ank);

            Rotation.x = (float) rot - 1.5f;

        }

        public void move(float amountZ, float amountX)
        {
            float cos_y = (float) System.Math.Cos(Rotation.y);
            float sin_y = (float) System.Math.Sin(Rotation.y);

            Translation.z += cos_y * amountZ * speed;
            Translation.x -= sin_y * amountZ * speed;

            Translation.x += cos_y * amountX * speed;
            Translation.z += sin_y * amountX * speed;
        }

        //PROPERTIES
        public float4x4 View
        {
            get
            {
                view =
                float4x4.CreateTranslation(pivotPoint - Translation) *
                float4x4.CreateRotationY(Rotation.y) *
                float4x4.CreateRotationX(Rotation.x) *
                float4x4.CreateTranslation(float3.Multiply(pivotPoint, -1));
                return view;
            }
            set { view = value; }
        }

        public float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                fieldOfView = value;
                fieldOfView = (float) System.Math.Max(fieldOfView, 0.15f * System.Math.PI);
                fieldOfView = (float) System.Math.Min(fieldOfView, 0.75f * System.Math.PI);
            }
        }

        private double DegreeToRadian(double degree)
        {
            return System.Math.PI * degree / 180.0;
        }

        private double RadianToDegree(double radian)
        {
            return radian * (180.0 / System.Math.PI);
        }
    }
}
