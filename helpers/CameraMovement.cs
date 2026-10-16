using System;
using System.Collections.Generic;
using System.Text;
using Void.Engine.Cameras;
using Void.Engine.Inputs.Keyboards;
using Void.Engine.Systems;

namespace VoidGame.helpers
{
    public class CameraMovement
    {
        private Camera _camera;
        private const float MoveSpeed = 200f;
        private const float RotationSpeed = 1.5f;

        public CameraMovement(Camera camera) => _camera = camera;

        public  void OnUpdate(FrameTime frameTime)
        {
            var state = Keyboard.GetState();
            var movement = Vect2.Zero;

            if (state.IsKeyDown(KeyboardKey.S)) movement.Y += 1f;
            if (state.IsKeyDown(KeyboardKey.W)) movement.Y -= 1f;
            if (state.IsKeyDown(KeyboardKey.D)) movement.X += 1f;
            if (state.IsKeyDown(KeyboardKey.A)) movement.X -= 1f;

            _camera.Position += movement * MoveSpeed * frameTime.DeltaTime;

            float rotation = 0f;
            if (state.IsKeyDown(KeyboardKey.Q)) rotation -= 1f;
            if (state.IsKeyDown(KeyboardKey.E)) rotation += 1f;
            if (state.IsKeyDown(KeyboardKey.Home)) _camera.Rotation = 0f;

            _camera.Rotation += rotation * RotationSpeed * frameTime.DeltaTime;
        }
    }
}
