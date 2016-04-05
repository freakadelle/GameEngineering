using System;
using System.Diagnostics;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.GUI;
using Fusee.Math.Core;
using Fusee.Serialization;


namespace Fusee.Tutorial.Core
{
    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        private const string _vertexShader = @"
     attribute vec3 fuVertex;

    void main()
    {
        gl_Position = vec4(fuVertex, 1.0);
    }";

        private const string pixelShader_pink = @"
    #ifdef GL_ES
        precision highp float;
    #endif

    void main()
    {
        gl_FragColor = vec4(1, 0, 1, 1);
    }";

        private const string pixelShader_blue = @"
    #ifdef GL_ES
        precision highp float;
    #endif

    void main()
    {
        gl_FragColor = vec4(0.2f, 0.2f, 1, 1);
    }";

        private Mesh mesh_rectangle;
        private Mesh mesh_triangle;

        private ShaderProgram shader_pink;
        private ShaderProgram shader_blue;

        private Random rnd;

        // Init is called on startup. 
        public override void Init()
        {
            rnd = new Random();
            
            // Set the clear color for the backbuffer to random color.
            RC.ClearColor = new float4(rnd.Next(0, 100) / 100.0f, rnd.Next(0, 100) / 100.0f, rnd.Next(0, 100) / 100.0f, 1);
            

            //Create my shaders and save them into variables
            shader_pink = RC.CreateShader(_vertexShader, pixelShader_pink);
            shader_blue = RC.CreateShader(_vertexShader, pixelShader_blue);

            //create new mesh random triangle
            mesh_triangle = new Mesh();
            mesh_triangle.Vertices = getRandomVerticesArray(3);
            //Sometimes not visible because of wrong indexing/culling/direction
            mesh_triangle.Triangles = new ushort[] {0, 1, 2};

            //create new mesh manual rectangle
            mesh_rectangle = new Mesh
            {
                Vertices = new[]
                {
                    new float3(-1f, 1f, 0),
                    new float3(-1f, 0.5f, 0),
                    new float3(-0.5f, 0.5f, 0),
                    new float3(-0.5f, 1, 0)
                },

                Triangles = new ushort[] { 0, 1, 2, 2, 3, 0},
            };
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            //Set blue shader and render rectangle
            RC.SetShader(shader_blue);
            RC.Render(mesh_rectangle);

            //Set pink shader and render triangle
            RC.SetShader(shader_pink);
            RC.Render(mesh_triangle);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rerndered farame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width/(float) Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

        //MyFunctions

        //Generate an array consisting of random Vertices
        private float3[] getRandomVerticesArray(int _amount)
        {
            float3[] verticesArray = new float3[_amount];

            //generate some random vertices
            for (int i = 0; i < verticesArray.Length; i++)
            {
                float rndX = rnd.Next(-100, 100) / 100.0f;
                float rndY = rnd.Next(-100, 100) / 100.0f;
                float rndZ = rnd.Next(-100, 100) / 100.0f;
                verticesArray.SetValue(new float3(rndX, rndY, rndZ), i);
            }

            return verticesArray;
        }
    }
}