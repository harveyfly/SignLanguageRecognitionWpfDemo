using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Kinect.BodyTracking;

namespace SignLanguageRecognitionWpfDemo
{
    public static class SLRPostJoint
    {
        public static readonly IReadOnlyList<JointId> PostJointsList = new List<JointId>
        {
            JointId.ShoulderLeft,
            JointId.ElbowLeft,
            JointId.WristLeft,
            JointId.HandLeft,
            JointId.ShoulderRight,
            JointId.ElbowRight,
            JointId.WristRight,
            JointId.HandRight,
            JointId.HandTipLeft,
            JointId.ThumbLeft,
            JointId.HandTipRight,
            JointId.ThumbRight
        };
    }
}
