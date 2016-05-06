using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private float _alpha = 0.001f;
        private float _beta;

        private Renderer _renderer;

        private Wuggy wuggy;
        private Camera cam;
        

        // Init is called on startup. 
        public override void Init()
        {
            _renderer = new Renderer(RC);

            wuggy = new Wuggy(AssetStorage.Get<SceneContainer>("Wuggy.fus"));
            wuggy.WuggyObjDict = createTransformDictFromSceneNode(wuggy.Children);
            wuggy.rootElement.Translation = new float3(0,0,10);

            cam = new Camera();
            cam.Translation.y += 10;
            adjustProjectionMatrice();


            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0, 0, 0, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            //if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            //{
            //    _alpha -= speed.x*0.0001f;
            //    _beta  -= speed.y*0.0001f;
            //}

            _alpha += 0.01f;
            _beta += 0.01f;
            double sin = System.Math.Sin(_alpha);
            double cos = System.Math.Cos(_beta);

            //Update Light Position
            _renderer.newLightPos(new float3((float)sin * 5, 0, (float)cos * 5));

            //Input Controlls & calculation for Wuggy
            wuggy.accelerate(Keyboard.WSAxis);
            wuggy.steer(Keyboard.ADAxis);
            wuggy.camerasLookAt(cam.Translation);

            //Input controlls and calculation for camera
            cam.pivotPoint = cam.Translation;
            //cam.move(Keyboard.UpDownAxis, Keyboard.LeftRightAxis);
            //Cam look at target not working properly
            cam.lookAtTarget(wuggy.rootElement.Translation);
            cam.FieldOfView += Keyboard.LeftRightAxis/100.0f;

            adjustProjectionMatrice();

            //Set Render Settigns
            _renderer.Shininess -= Keyboard.UpDownAxis;
            _renderer.View = cam.View;
            _renderer.Traverse(wuggy.Children);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            adjustProjectionMatrice();

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
        }

        //MY METHODS
        //--------------------------------------------------------------

        public void adjustProjectionMatrice()
        {
            var aspectRatio = Width / (float)Height;
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(cam.FieldOfView, aspectRatio, 0.01f, 20);
        }

        private Dictionary<string, TransformComponent> createTransformDictFromSceneNode(List<SceneNodeContainer> elemList, Dictionary<string, TransformComponent> tempDict = null)
        {
            if (tempDict == null)
            {
                tempDict = new Dictionary<string, TransformComponent>();
            }

            foreach (var el in elemList)
            {
                if (el != null)
                {
                    Debug.WriteLine(el.Name);
                    try
                    {
                        tempDict.Add(el.Name, el.GetTransform());
                    }
                    catch (System.ArgumentException)
                    {
                        
                        string newIdentifier = el.Name + "_n";
                        Debug.WriteLine("An duplicate identifier found. Changed from '" + el.Name + "' to '" + newIdentifier);
                        tempDict.Add(newIdentifier, el.GetTransform());
                    }

                    if (el.Children != null)
                    {
                        createTransformDictFromSceneNode(el.Children, tempDict);
                    }
                }
            }
            
            return tempDict;
        }
    }
}