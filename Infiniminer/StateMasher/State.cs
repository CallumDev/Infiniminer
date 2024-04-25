using Infiniminer;
using LibreLancer;

namespace StateMasher
{
    public class State
    {
        public StateMachine _SM = null;
        public Infiniminer.PropertyBag _P = null;

        public virtual void OnEnter(string oldState)
        {
        }

        public virtual void OnLeave(string newState)
        {
        }

        public virtual string OnUpdate(double elapsed)
        {
            return null;
        }

        public virtual void OnRenderAtEnter()
        {
        }

        public virtual void OnRenderAtUpdate(double elapsed)
        {
        }

        public virtual void OnTextEntered(string e)
        {
        }

        public virtual void OnKeyDown(Keys key)
        {
        }

        public virtual void OnKeyUp(Keys key)
        {
        }

        public virtual void OnMouseMove(int x, int y)
        {
        }
        
        public virtual void OnMouseDown(MouseButton button, int x, int y)
        {
        }

        public virtual void OnMouseUp(MouseButton button, int x, int y)
        {
        }

        public virtual void OnMouseScroll(int scrollWheelValue)
        {
        }
    }
}
