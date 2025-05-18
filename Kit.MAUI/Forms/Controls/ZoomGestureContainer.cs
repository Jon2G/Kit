

namespace Kit.Forms.Controls
{
    public class ZoomGestureContainer : ContentView
    {
        public double CurrentScale { get;private set; } = 1;
        double startScale = 1;
        double xOffset = 0;
        double yOffset = 0;
        public event EventHandler OnSwiped;
        public ZoomGestureContainer()
        {
            var pinchGesture = new PinchGestureRecognizer();
            var swipeGestureUp = new SwipeGestureRecognizer()
            {
                Direction =
                    SwipeDirection.Up,
                Threshold = 10
            };
            var swipeGestureDown = new SwipeGestureRecognizer()
            {
                Direction =
                    SwipeDirection.Down,
                Threshold = 10
            };
            var swipeGestureLeft = new SwipeGestureRecognizer()
            {
                Direction =
                    SwipeDirection.Left,
                Threshold = 10
            };
            var swipeGestureRight = new SwipeGestureRecognizer()
            {
                Direction =
                    SwipeDirection.Right,
                Threshold = 10
            };

            swipeGestureUp.Swiped += SwipeGesture_Swiped;
            swipeGestureDown.Swiped += SwipeGesture_Swiped;
            swipeGestureLeft.Swiped += SwipeGesture_Swiped;
            swipeGestureRight.Swiped += SwipeGesture_Swiped;

            pinchGesture.PinchUpdated += OnPinchUpdated;

            GestureRecognizers.Add(pinchGesture);
            GestureRecognizers.Add(swipeGestureUp);
            GestureRecognizers.Add(swipeGestureDown);
            GestureRecognizers.Add(swipeGestureLeft);
            GestureRecognizers.Add(swipeGestureRight);
        }

        private void SwipeGesture_Swiped(object sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Down:
                    Content.TranslationY += 70;
                    break;
                case SwipeDirection.Up:
                    Content.TranslationY -= 70;
                    break;
                case SwipeDirection.Left:
                    Content.TranslationX -= 70;
                    break;
                case SwipeDirection.Right:
                    Content.TranslationX += 70;
                    break;
            }

            OnSwiped?.Invoke(sender, e);
        }

        void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                startScale = Content.Scale;
                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }
            if (e.Status == GestureStatus.Running)
            {
                CurrentScale += (e.Scale - 1) * startScale;
                CurrentScale = Math.Max(1, CurrentScale);

                double renderedX = Content.X + xOffset;
                double deltaX = renderedX / Width;
                double deltaWidth = Width / (Content.Width * startScale);
                double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                double renderedY = Content.Y + yOffset;
                double deltaY = renderedY / Height;
                double deltaHeight = Height / (Content.Height * startScale);
                double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                double targetX = xOffset - (originX * Content.Width) * (CurrentScale - startScale);
                double targetY = yOffset - (originY * Content.Height) * (CurrentScale - startScale);

                Content.TranslationX = Clamp(targetX, -Content.Width * (CurrentScale - 1), 0);
                Content.TranslationY = Clamp(targetY, -Content.Height * (CurrentScale - 1), 0);

                Content.Scale = CurrentScale;
            }
            if (e.Status == GestureStatus.Completed)
            {
                xOffset = Content.TranslationX;
                yOffset = Content.TranslationY;
            }
        }
        public static double Clamp(double self, double min, double max)
        {
            return Math.Min(max, Math.Max(self, min));
        }
    }
}
