using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(JoyAxisWriter))]
    public class CustomAxisInputPasser : MonoBehaviour
    {
        public string AxisName;
        private JoyAxisWriter joyAxisWriter;

        private void Start()
        {
            joyAxisWriter = GetComponent<JoyAxisWriter>();
        }
        
        private void Update()
        {
            joyAxisWriter.Write(CustomInput.GetAxis(AxisName));
        }
    }
}