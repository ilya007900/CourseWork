using System;
using System.Drawing;

namespace AppDomain.Events
{
    public class ImageGrabbedEvent : EventArgs
    {
        public Bitmap Image { get; }

        public ImageGrabbedEvent(Bitmap image)
        {
            Image = image;
        }
    }
}