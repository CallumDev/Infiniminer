using System;
using System.Numerics;
using LibreLancer;
using LibreLancer.Graphics;


namespace Infiniminer
{
    public class Camera : ICamera
    {
        public float Pitch, Yaw;
        public Vector3 Position { get; set; }
        public bool FrustumCheck(BoundingSphere sphere) => new BoundingFrustum(ViewProjection).Intersects(sphere);

        public bool FrustumCheck(BoundingBox box) => new BoundingFrustum(ViewProjection).Intersects(box);

        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 Projection { get; set; } = Matrix4x4.Identity;

        public Matrix4x4 ViewProjection => View * Projection;

        private RenderContext rcontext;
        public Camera(RenderContext rcontext)
        {
            Pitch = 0;
            Yaw = 0;
            Position = Vector3.Zero;
            this.rcontext = rcontext;
            UpdateProjection();
        }

        void UpdateProjection()
        {
            float aspectRatio = rcontext.CurrentViewport.AspectRatio;
            this.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(70), aspectRatio, 0.01f, 1000.0f);
        }
        

        // Returns a unit vector pointing in the direction that we're looking.
        public Vector3 GetLookVector()
        {
            Matrix4x4 rotation = Matrix4x4.CreateRotationX(Pitch) * Matrix4x4.CreateRotationY(Yaw);
            return Vector3.Transform(Vectors.Forward, rotation);
        }

        public Vector3 GetRightVector()
        {
            Matrix4x4 rotation = Matrix4x4.CreateRotationX(Pitch) * Matrix4x4.CreateRotationY(Yaw);
            return Vector3.Transform(Vectors.Right, rotation);
        }

        public void Update()
        {
            Vector3 target = Position + GetLookVector();
            this.View = Matrix4x4.CreateLookAt(Position, target, Vectors.Up);
        }
    }
}
