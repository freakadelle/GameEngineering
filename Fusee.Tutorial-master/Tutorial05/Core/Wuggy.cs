using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Base.Core;
using Fusee.Math.Core;
using Fusee.Serialization;

namespace Fusee.Tutorial.Core
{
    class Wuggy: SceneContainer
    {

        //public SceneContainer sceneContainer;
        private Dictionary<string, TransformComponent> wuggyObjDict;
        public TransformComponent rootElement;

        public float speed = 0.05f;
        public bool isMoving = false;
        public float movingDirection;

        public Wuggy(SceneContainer sc)
        {
            base.Children = sc.Children;
            base.Header = sc.Header;
        }

        public void steer(float amount)
        {
            if (isMoving)
            {
                rootElement.Rotation.y += amount*0.05f;
            }

            //Rotate Wheels
            float rotAngle = amount/2.5f;

            if (movingDirection < 0)
            {
                rotAngle *= -1;
            }

            wuggyObjDict["WheelBigL"].Rotation.y = rotAngle;
            wuggyObjDict["WheelBigR"].Rotation.y = rotAngle;
            wuggyObjDict["WheelSmallL"].Rotation.y = -rotAngle;
            wuggyObjDict["WheelSmallR"].Rotation.y = -rotAngle;
        }

        public void accelerate(float amount)
        {
            if (amount != 0)
            {
                isMoving = true;
                movingDirection = amount;

                //Wheel Rotation
                wuggyObjDict["WheelBigL"].Rotation.x -= speed*amount;
                wuggyObjDict["WheelBigR"].Rotation.x -= speed*amount;

                wuggyObjDict["WheelSmallL"].Rotation.x -= speed*amount * 1.5f;
                wuggyObjDict["WheelSmallR"].Rotation.x -= speed*amount * 1.5f;

                //Forward/Backward movement
                wuggyObjDict["Wuggy"].Translation.x -= (float) System.Math.Sin(wuggyObjDict["Wuggy"].Rotation.y)*amount*
                                                       speed;
                wuggyObjDict["Wuggy"].Translation.z -= (float) System.Math.Cos(wuggyObjDict["Wuggy"].Rotation.y)*amount*
                                                       speed;
            }
            else
            {
                isMoving = false;
            }
        }

        public void camerasLookAt(float3 target)
        {
            float ggk = rootElement.Translation.x;
            float ank = rootElement.Translation.z;
            double rot = System.Math.Atan2(ggk, ank);

            wuggyObjDict["NeckHi"].Rotation.y = (float)rot - rootElement.Rotation.y;
        }

        //PROPERTIES
        public Dictionary<string, TransformComponent> WuggyObjDict
        {
            get { return wuggyObjDict; }
            set
            {
                wuggyObjDict = value;
                rootElement = wuggyObjDict["Wuggy"];
            }
        }
    }
}
