using System;
using Microsoft.Azure.Kinect.BodyTracking;

namespace SignLanguageRecognitionWpfDemo
{
    public class VisualFrameData : IDisposable
    {
        private Frame frame;

        public Frame Frame {
            set {
                lock (this)
                {
                    frame?.Dispose();
                    frame = value;
                }
            }
        }

        public Frame TakeFrameWithOwnership()
        {
            lock (this)
            {
                var result = frame;
                frame = null;
                return result;
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                frame?.Dispose();
                frame = null;
            }
        }
    }
}
