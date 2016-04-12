using System;
using System.Diagnostics;
using Fusee.Base.Common;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        private const string _vertexShader = @"
            attribute vec3 fuVertex;
            attribute vec3 fuNormal;

            uniform mat4 xform;

            varying vec3 modelpos;
            varying vec3 normal;

            void main()
            {
                modelpos = fuVertex;
                normal = fuNormal;

                //modelpos = modelpos / gl_Position.w;
                gl_Position = xform * vec4(modelpos, 1.0);
            }";

        private const string _pixelShader = @"
            #ifdef GL_ES
                precision highp float;
            #endif

            varying vec3 modelpos;
            varying vec3 normal;
            uniform vec4 params;

            void main()
            {
                float r = ((normal.x / 2.0) + 0.5) * (params.w / 2.0);
                float g = ((normal.y / 2.0) + 0.5) * (params.w / 7.0);
                float b = ((normal.z / 2.0) + 0.5) * (params.w / 15.0);
                gl_FragColor = vec4(r, g, b, 1.0);
            }";


        public static float ASPECT_RATIO;
        private IShaderParam _xformParam;
        private IShaderParam _shaderParams;
        private float4 _params;
        private CraneArm[] craneArms;
        private float3 _alpha;

        private Random rnd;

        public float4x4 _xform;

        // Init is called on startup. 
        public override void Init()
        {
            var shader = RC.CreateShader(_vertexShader, _pixelShader);
            RC.SetShader(shader);

            _xformParam = RC.GetShaderParam(shader, "xform");
            _shaderParams = RC.GetShaderParam(shader, "params");

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.1f, 0.1f, 0.1f, 1);

            Tutorial.ASPECT_RATIO = Width/(float) Height;

            craneArms = new CraneArm[50];

            for (int i = 0; i < craneArms.Length; i++)
            {
                craneArms[i] = new CraneArm();
                float length = 0.05f;
                craneArms[i].scaling = new float3(length, 0.5f / i, 0.5f / i);
                //craneArms[i].position = new float3((i * length) * 1f, 0, 0);
                craneArms[i].position = new float3( 0.5f + (i * length) * 2f, 0, 0);
            }

            _alpha = new float3(0, 0, 0);
            _params = new float4(0,0,0,0);
            rnd = new Random();

        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 speed = Mouse.Velocity;

            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                //Rotate arounf x and y axis by dragging the mouse/touch Input
                _alpha.x += speed.x * -0.0001f;
                _alpha.y += speed.y*-0.0001f;
            }

            craneArms[0].yaw += Keyboard.ADAxis * 0.1f;
            craneArms[0].pitch += Keyboard.WSAxis * 0.1f;
            craneArms[1].yaw += Keyboard.LeftRightAxis * 0.1f;
            craneArms[1].pitch += Keyboard.UpDownAxis * 0.1f;

            // Setup View
            var projection = float4x4.CreatePerspectiveFieldOfView((float) System.Math.PI * 0.5f, Tutorial.ASPECT_RATIO, 0.01f, 50);

            _alpha.x = System.Math.Max(_alpha.x, (float) -System.Math.PI);
            _alpha.x = System.Math.Min(_alpha.x, (float) System.Math.PI);
            var view = float4x4.CreateTranslation(-1.5f, 0, 3f) * float4x4.CreateRotationY(_alpha.x) * float4x4.CreateRotationX(_alpha.y);

            //Apply Matrice Changes to CraneArms
            float4x4 parent_xForm = float4x4.Identity;

            //First manual Arm
            craneArms[0].rotateAroundPivot(new float3(craneArms[0].pitch, craneArms[0].yaw, 0), new float3(-craneArms[0].scaling.x, 0, 0));
            _xform = projection * view * parent_xForm * craneArms[0].xForm * craneArms[0].getScalingMat();
            RC.SetShaderParam(_xformParam, _xform);
            RC.Render(craneArms[0].mesh);
            parent_xForm = parent_xForm * craneArms[0].xForm * float4x4.CreateTranslation(-craneArms[0].position);

            //Second Manual Arm
            craneArms[1].rotateAroundPivot(new float3(craneArms[1].pitch, craneArms[1].yaw, 0), new float3(-craneArms[1].scaling.x, 0, 0));
            _xform = projection * view * parent_xForm * craneArms[1].xForm * craneArms[1].getScalingMat();
            RC.SetShaderParam(_xformParam, _xform);
            RC.Render(craneArms[1].mesh);
            parent_xForm = parent_xForm * craneArms[1].xForm * float4x4.CreateTranslation(-craneArms[1].position);

            for (int i = 2; i < craneArms.Length; i++)
            {
                if (craneArms[i].hasReachedTarget())
                {
                    craneArms[i].rndNewTarget(rnd);
                }
                else
                {
                    craneArms[i].update();
                }

                craneArms[i].rotateAroundPivot(new float3(craneArms[i].pitch, craneArms[i].yaw, 0),
                    new float3(-craneArms[i].scaling.x, 0, 0));
                _xform = projection*view*parent_xForm*craneArms[i].xForm*craneArms[i].getScalingMat();

                _params.w = craneArms[i].position.x;
                _params.xyz = craneArms[i].target;

                RC.SetShaderParam(_xformParam, _xform);
                RC.SetShaderParam(_shaderParams, _params);
                RC.Render(craneArms[i].mesh);

                parent_xForm = parent_xForm*craneArms[i].xForm*float4x4.CreateTranslation(-craneArms[i].position);
            }

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered farame) on the front buffer.
            Present();
        }

        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width/(float) Height;
            Tutorial.ASPECT_RATIO = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

        static float NextFloat(Random random)
        {
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            double exponent = System.Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }

    }
}