using System;
using System.Diagnostics;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.GUI;
using Fusee.Math.Core;
using Fusee.Serialization;
using static Fusee.Engine.Core.Input;


namespace Fusee.Tutorial.Core
{
    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        private Mesh _mesh;

        //Shader Params
        private IShaderParam _alphaParam_Vertex;
        private IShaderParam _alphaParam_Pixel;
        private IShaderParam _mousePosParam_Vertex;

        //Uniform shader Values
        private float3 _alpha;
        private float steps;
        private float2 mousePos;
        private float2 mouseVel;

        private Random rnd = new Random();

        private const string _vertexShader = @"
            attribute vec3 fuVertex;

            uniform vec3 alpha_v;
            varying vec3 modelpos;
            varying vec3 position;

            varying mat4 rotX;
            varying mat4 rotY;
            varying mat4 rotZ;

            void main()
            {
                modelpos = fuVertex;

                float sa = sin(alpha_v.x);
                float ca = cos(alpha_v.x);
                float sb = sin(alpha_v.y);
                float cb = cos(alpha_v.y);
                float sg = sin(alpha_v.z);
                float cg = cos(alpha_v.z);
                
                rotX = mat4(1.0f, 0.0f, 0.0f, 0.0f,
                            0.0f, ca,   -sa,  0.0f, 
                            0.0f, sa,   ca,   0.0f, 
                            0.0f, 0.0f, 0.0f, 1.0f);

                rotY = mat4(cb,   0.0f, sb,   0.0f,
                            0.0f, 1.0f, 0.0f, 0.0f, 
                            -sb,  0.0f, cb,   0.0f, 
                            0.0f, 0.0f, 0.0f, 1.0f);

                rotZ = mat4(cg,   -sg,   0.0f,  0.0f,
                            sg,    cg,   0.0f,  0.0f, 
                            0.0f,  0.0f, 0.0f,  0.0f, 
                            0.0f,  0.0f,  0.0f, 1.0f);

                gl_Position = rotZ * rotY * rotX * vec4(fuVertex, 1.0f);
                position = gl_Position;
            }";

        private const string _pixelShader = @"
            #ifdef GL_ES
            precision highp float;
            #endif

            varying vec3 modelpos;
            varying vec3 position;
            uniform vec2 mousePos_p;
            uniform vec3 alpha_p;    

            void main()
            {

                vec2 modelPos2D = vec2(position.x, position.y);

                float r = (modelpos.x + 0.7) - (distance (mousePos_p, modelPos2D));
                float g = (modelpos.y + 0.3) - (distance (mousePos_p, modelPos2D));
                float b = (modelpos.z + 0.7) - (distance (mousePos_p, modelPos2D)) * (sin(alpha_p.y)/sin(alpha_p.y));
                gl_FragColor = vec4(r, g, b, 1.0f);            
            }";


        // Init is called on startup. 
        public override void Init()
        {
            _mesh = new Mesh
            {
                Vertices = new[]
                {
                    new float3(0f, -0.7f, 0f), // Vertex 0
                    new float3(0f, 0.7f, 0), // Vertex 1
                    new float3(0.3f, 0, 0.3f), // Vertex 
                    new float3(-0.3f, 0f, 0.3f), // Vertex 3
                    new float3(-0.3f, 0f, -0.3f), // Vertex 4
                    new float3(0.3f, 0f, -0.3f) // Vertex 5
                },
                Triangles = new ushort[]
                {
                    0, 2, 5,
                    0, 5, 4,
                    0, 4, 3,
                    0, 3, 2,
                    1, 5, 2,
                    1, 4, 5,
                    1, 3, 4,
                    1, 2, 3
                },
            };

            var shader = RC.CreateShader(_vertexShader, _pixelShader);
            RC.SetShader(shader);

            //Initialize Shader Params
            _alphaParam_Vertex = RC.GetShaderParam(shader, "alpha_v");
            _alphaParam_Pixel = RC.GetShaderParam(shader, "alpha_p");
            _mousePosParam_Vertex = RC.GetShaderParam(shader, "mousePos_p");

            //Initialize Uniform Shader Values
            _alpha = new float3(0, 0, 0);
            mousePos = new float2(0, 0);
            steps = 0;

            // Set the clear color for the backbuffer.
            RC.ClearColor = new float4(0.1f, 0.3f, 0.2f, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            //Get Mouse Data
            mousePos = Mouse.Position;
            mouseVel = Mouse.Velocity;

            //Input Interaction rotation
            if (Mouse.LeftButton)
            {
                _alpha.y += mouseVel.x * 0.00001f;
                _alpha.x += mouseVel.y * 0.00001f;
                //_alpha.z -= System.Math.Abs((mouseVel.x * 0.00001f) * (mouseVel.y * 0.00001f));
            }
            else if (Keyboard.UpDownAxis != 0)
            {
                steps = Keyboard.UpDownAxis / 100.0f;
                _alpha.x -= steps;
            }
            else if (Keyboard.LeftRightAxis != 0)
            {
                steps = Keyboard.LeftRightAxis / 100.0f;
                _alpha.z -= steps;
            }

            //Umrechnung auf Screen Koordinaten -1/1
            mousePos.x = ((1.0f / (Width / 2.0f)) * mousePos.x) - 1.0f;
            mousePos.y = (((1.0f / (Height / 2.0f)) * mousePos.y) - 1.0f) * -1.0f;

            //Set values to the specific uniform shader value
            RC.SetShaderParam(_alphaParam_Vertex, _alpha);
            RC.SetShaderParam(_alphaParam_Pixel, _alpha);
            RC.SetShaderParam(_mousePosParam_Vertex, mousePos);

            RC.Render(_mesh);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rerndered farame) on the front buffer.
            Present();
        }

        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }
    }
}