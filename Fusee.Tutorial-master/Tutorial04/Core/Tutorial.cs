using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        private Mesh _mesh;
        private Random rnd;
        private const string _vertexShader = @"
            attribute vec3 fuVertex;
            attribute vec3 fuNormal;
            uniform mat4 FUSEE_MVP;
            uniform mat4 FUSEE_MV;
            uniform mat4 FUSEE_ITMV;

            varying vec3 modelpos;
            varying vec3 normal;
            varying vec3 normal_model;

            void main()
            {
                modelpos = fuVertex;
                normal_model = fuNormal;
                normal = normalize(mat3(FUSEE_MVP) * fuNormal);
                gl_Position = FUSEE_MVP * vec4(fuVertex, 1.0);
            }";

        private const string _pixelShader = @"
            #ifdef GL_ES
                precision highp float;
            #endif
            varying vec3 modelpos;
            varying vec3 normal;
            varying vec3 normal_model;

            uniform vec3 albedo;

            void main()
            {
                float intensity = dot(normal, vec3(0, 0, -1));
                gl_FragColor = vec4(intensity * albedo, 1);
            }";


        private float _alpha;
        private float _beta;

        private IShaderParam _albedoParam;
        private SceneOb _humanModel_root;
        private Dictionary<string, SceneOb> _humanModelChilds = new Dictionary<string, SceneOb>();
        private List <SceneOb> _humanModel_movableChilds;

        // Init is called on startup. 
        public override void Init()
        {
            // Initialize the shader(s)
            var shader = RC.CreateShader(_vertexShader, _pixelShader);
            RC.SetShader(shader);
            _albedoParam = RC.GetShaderParam(shader, "albedo");

            // Load some meshes
            Mesh cone = LoadMesh("Cone.fus");
            Mesh cube = LoadMesh("Cube.fus");
            Mesh cylinder = LoadMesh("Cylinder.fus");
            Mesh pyramid = LoadMesh("Pyramid.fus");
            Mesh sphere = LoadMesh("Sphere.fus");

            // Setup a list of objects
            _humanModel_root = new SceneOb {
                Children = new List<SceneOb>(new[] {
                    // Body
                    new SceneOb { Mesh = cylinder,     Pos = new float3(0, 3.05f, 0),     ModelScale = new float3(0.65f, 0.45f, 0.35f), Albedo = new float3(0.8f, 0.6f, 0.7f), ob_name = "upper_body"},
                    new SceneOb { Mesh = cube,     Pos = new float3(0, 2.35f, 0),     ModelScale = new float3(0.4f, 0.45f, 0.2f), Albedo = new float3(0.8f, 0.6f, 0.7f), ob_name = "lower_body"},
                    new SceneOb { Mesh = pyramid,     Pos = new float3(0, 2.1f, 0),     ModelScale = new float3(0.7f, 0.3f, 0.35f), Albedo = new float3(0.8f, 0.6f, 0.7f), ob_name = "hipp"},
                    new SceneOb { Mesh = cube,     Pos = new float3(0, 1.7f, 0),     ModelScale = new float3(0.7f, 0.1f, 0.35f), Albedo = new float3(0.8f, 0.6f, 0.7f), ob_name = "hipp2"},
                
                    //Breast - 4
                    new SceneOb { Mesh = sphere,     Pos = new float3(-0.3f, 3.1f, -0.35f),     ModelScale = new float3(0.2f, 0.3f, 0.2f), Albedo = new float3(0.8f, 0.6f, 0.7f), ob_name = "left_breast"},
                    new SceneOb { Mesh = sphere,     Pos = new float3(0.3f, 3.1f, -0.35f),     ModelScale = new float3(0.2f, 0.3f, 0.2f), Albedo = new float3(0.8f, 0.6f, 0.7f), ob_name = "right_breast"},
                
                    // LEFT LEG
                    new SceneOb
                    {
                        Children = new List<SceneOb>(new[] {
                            new SceneOb { Mesh = sphere, Pos = new float3(0.0f, -0.7f, 0),    ModelScale = new float3(0.25f, 0.25f, 0.25f), Albedo = new float3(0.3f, 0.4f, 0.75f), ob_name = "left_knee"},
                            new SceneOb
                            {
                                Children = new List<SceneOb>(new[] {
                                    new SceneOb { Mesh = cube, Pos = new float3(0.0f, -0.45f, -0.2f),    ModelScale = new float3(0.2f, 0.1f, 0.4f), Albedo = new float3(0.6f, 0.3f, 0.1f), ob_name = "left_foot"},
                                }),
                                Mesh = cylinder, Pos = new float3(0.0f, -1.2f, 0),    ModelScale = new float3(0.2f, 0.45f, 0.2f), Pivot = new float3(0, 0.5f, 0), Rotbounds = new float3x3(-2.5f, 0.1f, 0.0f, -0, 0, 0.0f, 0.0f, 0.0f, 0.0f), Albedo = new float3(0.3f, 0.4f, 0.75f), ob_name = "lower_left_leg"
                            },
                        }),
                        Mesh = cylinder, Pos = new float3(-0.35f, 1.1f, 0),    ModelScale = new float3(0.3f, 0.6f, 0.25f), Pivot = new float3(0, 0.6f, 0), Rotbounds = new float3x3(-0.5f, 1.5f, 0.0f, 0, 0, 0.0f, -1.0f, 0.0f, 0.0f), Albedo = new float3(0.3f, 0.4f, 0.75f), ob_name = "upper_left_leg"
                    },

                    //RIGHT LEG
                    new SceneOb
                    {
                        Children = new List<SceneOb>(new[] {
                            new SceneOb { Mesh = sphere, Pos = new float3(0.0f, -0.7f, 0),    ModelScale = new float3(0.25f, 0.25f, 0.25f), Albedo = new float3(0.3f, 0.4f, 0.75f), ob_name = "right_knee"},
                            new SceneOb
                            {
                                Children = new List<SceneOb>(new[] {
                                    new SceneOb { Mesh = cube, Pos = new float3(0.0f, -0.45f, -0.2f),    ModelScale = new float3(0.2f, 0.1f, 0.4f), Albedo = new float3(0.6f, 0.3f, 0.1f), ob_name = "right_foot"},
                                }), 
                                Mesh = cylinder, Pos = new float3(0.0f, -1.2f, 0),    ModelScale = new float3(0.2f, 0.45f, 0.2f), Pivot = new float3(0, 0.5f, 0), Rotbounds = new float3x3(-2.5f, 0.1f, 0.0f, -0, 0, 0.0f, 0.0f, 0.0f, 0.0f), Albedo = new float3(0.3f, 0.4f, 0.75f), ob_name = "lower_right_leg"
                            },
                        }),
                        Mesh = cylinder, Pos = new float3(0.35f, 1.1f, 0),    ModelScale = new float3(0.3f, 0.6f, 0.25f), Pivot = new float3(0, 0.6f, 0), Rotbounds = new float3x3(-0.5f, 1.5f, 0.0f, -0, 0, 0.0f, 0.0f, 1.0f, 0.0f), Albedo = new float3(0.3f, 0.4f, 0.75f), ob_name = "upper_right_leg"
                    },
                    
                    // Shoulders
                    new SceneOb { Mesh = sphere,   Pos = new float3(-0.7f, 3.3f, 0), ModelScale = new float3(0.3f, 0.3f, 0.3f), Albedo = new float3(0.8f, 0.6f, 0.7f), ob_name = "left_shoulder"},
                    new SceneOb { Mesh = sphere,   Pos = new float3( 0.7f, 3.3f, 0), ModelScale = new float3(0.3f, 0.3f, 0.3f), Albedo = new float3(0.8f, 0.6f, 0.7f), ob_name = "right_shoulder"},
                    new SceneOb { Mesh = cone, Pos = new float3(0, 3.7f, 0), ModelScale = new float3(0.65f, 0.2f, 0.35f), Albedo = new float3(0.8f, 0.7f, 0.6f), ob_name = "neck"},
               
                    // Arms - 17
                    new SceneOb
                    {
                        Children = new List<SceneOb>(new[] {
                            new SceneOb { Mesh = sphere, Pos = new float3( 0.0f, -0.4f, 0), ModelScale = new float3(0.2f, 0.2f, 0.2f), Albedo = new float3(0.8f, 0.7f, 0.6f), ob_name = "left_elbow"},
                            new SceneOb { Mesh = cylinder, Pos = new float3(0.0f, -0.9f, 0), ModelScale = new float3(0.12f, 0.4f, 0.12f), Pivot = new float3(0, 0.5f, 0), Rotbounds = new float3x3(-0.5f, 2.5f, 0.0f, -0, 0, 0.0f, 0.0f, 0f, 0.0f), Albedo = new float3(0.8f, 0.7f, 0.6f), ob_name = "lower_left_arm"},
                        }),
                        Mesh = cylinder, Pos = new float3(-0.85f, 2.8f, 0), ModelScale = new float3(0.15f, 0.5f, 0.15f), Pivot = new float3(0, 0.5f, 0), Rotbounds = new float3x3(-0.5f, 2.5f, 0.0f, -0, 0, 0.0f, -2.5f, 0f, 0.0f), Albedo = new float3(0.8f, 0.7f, 0.6f), ob_name = "upper_left_arm"
                    },
                    new SceneOb
                    {
                        Children = new List<SceneOb>(new[] {
                            new SceneOb { Mesh = sphere, Pos = new float3( 0.0f, -0.4f, 0), ModelScale = new float3(0.2f, 0.2f, 0.2f), Albedo = new float3(0.8f, 0.7f, 0.6f), ob_name = "right_elbow"},

                            new SceneOb { Mesh = cylinder, Pos = new float3( 0.0f, -0.9f, 0), ModelScale = new float3(0.12f, 0.4f, 0.12f), Pivot = new float3(0, 0.5f, 0), Rotbounds = new float3x3(-0.5f, 2.5f, 0.0f, -0, 0, 0.0f, 0.0f, 0.0f, 0.0f), Albedo = new float3(0.8f, 0.7f, 0.6f), ob_name = "lower_right_arm"},
                        }),
                        Mesh = cylinder, Pos = new float3( 0.85f, 2.8f, 0), ModelScale = new float3(0.15f, 0.5f, 0.15f), Pivot = new float3(0, 0.5f, 0), Rotbounds = new float3x3(-0.5f, 2.5f, 0.0f, -0, 0, 0.0f, 0.0f, 2.5f, 0.0f), Albedo = new float3(0.8f, 0.7f, 0.6f), ob_name = "upper_right_arm"
                    },
                    
                    
                
                    // Head
                    new SceneOb
                    {
                        Children = new List<SceneOb>(new[] {
                             new SceneOb { Mesh = cone, Pos = new float3(0, 0.5f, 0), ModelScale = new float3(1, 0.2f, 1), Albedo = new float3(0.45f, 0.4f, 0.2f), ob_name = "hat"},
                        }),
                        Mesh = sphere,   Pos = new float3(0, 4.2f, 0), ModelScale = new float3(0.35f, 0.5f, 0.35f), Pivot = new float3(0, -0.4f, 0), Rotbounds = new float3x3(-1f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.0f), Albedo = new float3(0.8f, 0.7f, 0.6f), ob_name = "head"
                    },
                   
                })
            };

            //create associative dictionary from sceneObchilds and their string names
            _humanModelChilds = createDictFromSceneObChilds(_humanModel_root);

            //add movable sceneOb Childs to movable List by name
            _humanModel_movableChilds = new List<SceneOb>();
            _humanModel_movableChilds.Add(_humanModelChilds["head"]);
            _humanModel_movableChilds.Add(_humanModelChilds["lower_right_arm"]);
            _humanModel_movableChilds.Add(_humanModelChilds["lower_left_arm"]);
            _humanModel_movableChilds.Add(_humanModelChilds["upper_right_arm"]);
            _humanModel_movableChilds.Add(_humanModelChilds["upper_left_arm"]);
            _humanModel_movableChilds.Add(_humanModelChilds["lower_right_leg"]);
            _humanModel_movableChilds.Add(_humanModelChilds["lower_left_leg"]);
            _humanModel_movableChilds.Add(_humanModelChilds["upper_right_leg"]);
            _humanModel_movableChilds.Add(_humanModelChilds["upper_left_leg"]);

            rnd = new Random();

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(1, 1, 1, 1);
        }

        

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            //Mouse interaction camera view
            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x*0.0001f;
                _beta  -= speed.y*0.0001f;
            }

            //Update movable Childs of SceneOb
            foreach (SceneOb child in _humanModel_movableChilds)
            {
                if (child.hasReachedTarget())
                {
                    child.rndNewTarget(rnd);
                }
                else
                {
                    child.update();
                    child.adjustBoundsOfRotation();
                }
            }

            // Setup View and Projection
            var aspectRatio = Width / (float)Height;
            var view = float4x4.CreateTranslation(0, 0, 8)*float4x4.CreateRotationY(_alpha)*float4x4.CreateRotationX(_beta)* float4x4.CreateTranslation(0, -2, 0);
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 0.01f, 20);

            RenderSceneOb(_humanModel_root, view);

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

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

        //None override functions
        //-------------------------------------------------------------------------------
        private static Mesh LoadMesh(string assetName)
        {
            SceneContainer sc = AssetStorage.Get<SceneContainer>(assetName);
            MeshComponent mc = sc.Children.FindComponents<MeshComponent>(c => true).First();
            return new Mesh
            {
                Vertices = mc.Vertices,
                Normals = mc.Normals,
                Triangles = mc.Triangles
            };
        }

        private static float4x4 ModelXForm(float3 pos, float3 rot, float3 pivot)
        {
            return float4x4.CreateTranslation(pos + pivot)
                   * float4x4.CreateRotationY(rot.y)
                   * float4x4.CreateRotationX(rot.x)
                   * float4x4.CreateRotationZ(rot.z)
                   * float4x4.CreateTranslation(-pivot);
        }

        private void RenderSceneOb(SceneOb so, float4x4 modelView)
        {
            modelView = modelView * ModelXForm(so.Pos, so.Rot, so.Pivot) * float4x4.CreateScale(so.Scale);
            if (so.Mesh != null)
            {
                RC.ModelView = modelView * float4x4.CreateScale(so.ModelScale);
                RC.SetShaderParam(_albedoParam, so.Albedo);
                RC.Render(so.Mesh);
            }

            if (so.Children != null)
            {
                foreach (var child in so.Children)
                {
                    RenderSceneOb(child, modelView);
                }
            }
        }

        private Dictionary<string, SceneOb> createDictFromSceneObChilds(SceneOb so, Dictionary<string, SceneOb> tempDict = null)
        {
            if (tempDict == null)
            {
                tempDict = new Dictionary<string, SceneOb>();
            }

            if (so.Children != null)
            {
                foreach (var child in so.Children)
                {
                    tempDict.Add(child.ob_name, child);
                    createDictFromSceneObChilds(child, tempDict);
                }
            }
            return tempDict;
        }
    }
}