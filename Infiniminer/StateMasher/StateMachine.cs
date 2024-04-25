using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices; 
using System.Reflection;
using Infiniminer;
using LibreLancer;
using LibreLancer.Media;

namespace StateMasher
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class StateMachine : Game
    {
        public AudioManager Audio;
        public ContentManager Content;

        public Infiniminer.PropertyBag propertyBag = null;

        private string currentStateType = "";
        public string CurrentStateType
        {
            get { return currentStateType; }
        }

        private State currentState = null;
        private bool needToRenderOnEnter = false;

        //private Dictionary<Keys, bool> keysDown = new Dictionary<Keys, bool>();

        public StateMachine(int width, int height, bool fullscreen) : base(width, height, fullscreen)
        {
        }


        static MouseButton ConvertButton(MouseButtons input)
        {
            if ((input & MouseButtons.Left) == MouseButtons.Left)
                return MouseButton.LeftButton;
            if ((input & MouseButtons.Right) == MouseButtons.Right)
                return MouseButton.RightButton;
            if ((input & MouseButtons.Middle) == MouseButtons.Middle)
                return MouseButton.MiddleButton;
            return (MouseButton)0xFF;
        }
        
        protected override void Load()
        {
            Content = new ContentManager(RenderContext);
            Services.Add(Content);
            FLLog.Info("Audio", "Initialising Audio");
            Audio = new AudioManager(this);
            Audio.WaitReady();            
            Content.AudioManager = Audio;
            Keyboard.KeyDown += a => currentState?.OnKeyDown(a.Key);
            Keyboard.KeyUp += a => currentState?.OnKeyUp(a.Key);
            Keyboard.TextInput += a => currentState?.OnTextEntered(a);
            Mouse.MouseDown += a =>
            {
                var c = ConvertButton(a.Buttons);
                if(c != (MouseButton)0xFF)
                    currentState?.OnMouseDown(c, a.X, a.Y);
            };
            Mouse.MouseUp += a =>
            {
                var c = ConvertButton(a.Buttons);
                if(c != (MouseButton)0xFF)
                    currentState?.OnMouseUp(c, a.X, a.Y);
            };
            Mouse.MouseMove += a =>
                currentState?.OnMouseMove(a.X, a.Y);
            Mouse.MouseWheel += (_, wheely) =>
            {
                if (wheely < 0)
                    currentState?.OnMouseDown(MouseButton.WheelDown, Mouse.X, Mouse.Y);
                else if(wheely > 0)
                    currentState?.OnMouseDown(MouseButton.WheelUp, Mouse.X, Mouse.Y);
            };
        }

        protected void ChangeState(string newState)
        {
            // Call OnLeave for the old state.
            if (currentState != null)
                currentState.OnLeave(newState);

            // Instantiate and set the new state.
            Assembly a = Assembly.GetExecutingAssembly();
            Type t = a.GetType(newState);
            currentState = Activator.CreateInstance(t) as State;

            // Set up the new state.
            currentState._P = propertyBag;
            currentState._SM = this;
            currentState.OnEnter(currentStateType);
            currentStateType = newState;
            needToRenderOnEnter = true;
        }
        

        protected override void Update(double gameTime)
        {
            if (currentState != null && propertyBag != null)
            {
                // Call OnUpdate.
                string newState = currentState.OnUpdate(gameTime);
                if (newState != null)
                    ChangeState(newState);
            }

            base.Update(gameTime);
        }

        protected override void Draw(double gameTime)
        {
            RenderContext.ReplaceViewport(0, 0, Width, Height);
            RenderContext.ClearColor = Color4.Black;
            RenderContext.ClearAll();

            // Call OnRenderAtUpdate.
            if (currentState != null && propertyBag != null)
            {
                currentState.OnRenderAtUpdate(gameTime);
            }

            // If we have one queued, call OnRenderAtEnter.
            if (currentState != null && needToRenderOnEnter && propertyBag != null)
            {
                needToRenderOnEnter = false;
                currentState.OnRenderAtEnter();
            }
            
            base.Draw(gameTime);
        }

        protected override void Cleanup()
        {
            Audio.Dispose();
        }
    }
}
