////////////////////////////////////////////////////////////////
//                                                            //
//  Neoforce Controls                                         //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//         File: InputSystem.cs                               //
//                                                            //
//      Version: 0.7                                          //
//                                                            //
//         Date: 11/09/2010                                   //
//                                                            //
//       Author: Tom Shane                                    //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//  Copyright (c) by Tom Shane                                //
//                                                            //
////////////////////////////////////////////////////////////////

#region //// Using /////////////

////////////////////////////////////////////////////////////////////////////
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

////////////////////////////////////////////////////////////////////////////

#endregion

namespace TomShane.Neoforce.Controls
{

    #region //// Enums /////////////

    ////////////////////////////////////////////////////////////////////////////
    [Flags]
    public enum InputMethods
    {
        None = 0x00,
        Keyboard = 0x01,
        Mouse = 0x02,
        GamePad = 0x04,
        All = Keyboard | Mouse | 0x04
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    public enum MouseButton
    {
        None = 0,
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }
    ////////////////////////////////////////////////////////////////////////////

    public enum MouseScrollDirection
    {
        None = 0,
        Down = 1,
        Up = 2
    }

    ////////////////////////////////////////////////////////////////////////////
    public enum GamePadButton
    {
        None = 0,
        Start = 6,
        Back,
        Up,
        Down,
        Left,
        Right,
        A,
        B,
        X,
        Y,
        BigButton,
        LeftShoulder,
        RightShoulder,
        LeftTrigger,
        RightTrigger,
        LeftStick,
        RightStick,
        LeftStickLeft,
        LeftStickRight,
        LeftStickUp,
        LeftStickDown,
        RightStickLeft,
        RightStickRight,
        RightStickUp,
        RightStickDown
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    public enum ActivePlayer
    {
        None = -1,
        One = 0,
        Two = 1,
        Three = 2,
        Four = 3
    }
    ////////////////////////////////////////////////////////////////////////////

    #endregion

    #region //// Structs ///////////

    ////////////////////////////////////////////////////////////////////////////
    public struct GamePadVectors
    {
        public Vector2 LeftStick;
        public Vector2 RightStick;
        public float LeftTrigger;
        public float RightTrigger;
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    public struct InputOffset
    {
        public int X;
        public int Y;
        public float RatioX;
        public float RatioY;

        public InputOffset(int x, int y, float rx, float ry)
        {
            X = x;
            Y = y;
            RatioX = rx;
            RatioY = ry;
        }
    }
    ////////////////////////////////////////////////////////////////////////////

    #endregion

    #region //// Classes ///////////

    ////////////////////////////////////////////////////////////////////////////
    public class InputSystem : Disposable
    {

        #region //// Classes ///////////

        ////////////////////////////////////////////////////////////////////////////
        private class InputKey
        {
            public Keys Key = Keys.None;
            public bool Pressed = false;
            public double Countdown = RepeatDelay;
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        private class InputMouseButton
        {
            public MouseButton Button = MouseButton.None;
            public bool Pressed = false;
            public double Countdown = RepeatDelay;

            public InputMouseButton()
            {
            }

            public InputMouseButton(MouseButton button)
            {
                Button = button;
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////    
        private class InputMouse
        {
            public MouseState State = new MouseState();
            public Point Position = new Point(0, 0);
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        private class InputGamePadButton
        {
            public GamePadButton Button = GamePadButton.None;
            public bool Pressed = false;
            public double Countdown = RepeatDelay;

            public InputGamePadButton()
            {
            }

            public InputGamePadButton(GamePadButton button)
            {
                Button = button;
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Consts ////////////

        ////////////////////////////////////////////////////////////////////////////
        private const int RepeatDelay = 500;
        private const int RepeatRate = 50;
        private float ClickThreshold = 0.5f;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Fields ////////////

        ////////////////////////////////////////////////////////////////////////////    
        private List<InputKey> keys = new List<InputKey>();
        private List<InputMouseButton> mouseButtons = new List<InputMouseButton>();
        private List<InputGamePadButton> gamePadButtons = new List<InputGamePadButton>();
        private MouseState mouseState = new MouseState();
        //private GamePadState gamePadState = new GamePadState();
        private Manager manager = null;
        private InputOffset inputOffset = new InputOffset(0, 0, 1.0f, 1.0f);
        private InputMethods inputMethods = InputMethods.All;
        private ActivePlayer activePlayer = ActivePlayer.None;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Properties ////////

        ////////////////////////////////////////////////////////////////////////////                
        /// <summary>
        /// Sets or gets input offset and ratio when rescaling controls in render target.
        /// </summary>
        public virtual InputOffset InputOffset
        {
            get { return inputOffset; }
            set { inputOffset = value; }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////                
        /// <summary>
        /// Sets or gets input methods allowed for navigation.
        /// </summary>
        public virtual InputMethods InputMethods
        {
            get { return inputMethods; }
            set { inputMethods = value; }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public virtual ActivePlayer ActivePlayer
        {
            get { return activePlayer; }
            set { activePlayer = value; }
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Events ////////////

        ////////////////////////////////////////////////////////////////////////////		        
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyPress;
        public event KeyEventHandler KeyUp;

        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MousePress;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
        /// <summary>
        /// Occurs when the mouse is scrolled.
        /// </summary>
        public event MouseEventHandler MouseScroll;

        public event GamePadEventHandler GamePadUp;
        public event GamePadEventHandler GamePadDown;
        public event GamePadEventHandler GamePadPress;
        public event GamePadEventHandler GamePadMove;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Constructors //////

        ////////////////////////////////////////////////////////////////////////////
        public InputSystem(Manager manager, InputOffset offset)
        {
            this.inputOffset = offset;
            this.manager = manager;
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public InputSystem(Manager manager) : this(manager, new InputOffset(0, 0, 1.0f, 1.0f))
        {
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Methods ///////////

        ////////////////////////////////////////////////////////////////////////////
        public virtual void Initialize()
        {
            keys.Clear();
            mouseButtons.Clear();
            gamePadButtons.Clear();

#if (!XBOX && !XBOX_FAKE)
            foreach (string str in Enum.GetNames(typeof(Keys)))
            {
                InputKey key = new InputKey();
                key.Key = (Keys)Enum.Parse(typeof(Keys), str);
                keys.Add(key);
            }

            foreach (string str in Enum.GetNames(typeof(MouseButton)))
            {
                InputMouseButton btn = new InputMouseButton();
                btn.Button = (MouseButton)Enum.Parse(typeof(MouseButton), str);
                mouseButtons.Add(btn);
            }

            foreach (string str in Enum.GetNames(typeof(GamePadButton)))
            {
                InputGamePadButton btn = new InputGamePadButton();
                btn.Button = (GamePadButton)Enum.Parse(typeof(GamePadButton), str);
                gamePadButtons.Add(btn);
            }
#else
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.None));
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.Start));
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.Back));
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.Up));        
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.Down));
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.Left));
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.Right));       
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.A));
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.B));
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.X));
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.Y));         
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.BigButton)); 
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.LeftShoulder)); 
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.RightShoulder)); 
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.LeftTrigger)); 
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.RightTrigger));         
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.LeftStick)); 
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.RightStick));         
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.LeftStickLeft));         
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.LeftStickRight));                 
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.LeftStickUp));
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.LeftStickDown));                         
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.RightStickLeft));                 
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.RightStickRight));                 
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.RightStickUp));                 
        gamePadButtons.Add(new InputGamePadButton(GamePadButton.RightStickDown));                 
#endif
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public virtual void SendMouseState(MouseState state, GameTime gameTime)
        {
            UpdateMouse(state, gameTime);
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public virtual void SendKeyboardState(KeyboardState state, GameTime gameTime)
        {
            UpdateKeys(state, gameTime);
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public virtual void SendGamePadState(PlayerIndex playerIndex, GamePadState state, GameTime gameTime)
        {
            UpdateGamePad(playerIndex, state, gameTime);
        }
        ////////////////////////////////////////////////////////////////////////////   

        ////////////////////////////////////////////////////////////////////////////
        public virtual void Update(GameTime gameTime, KeyboardState ks, MouseState ms)
        {
            {
#if (!XBOX && !XBOX_FAKE)
                if ((inputMethods & InputMethods.Mouse) == InputMethods.Mouse) UpdateMouse(ms, gameTime);
                if ((inputMethods & InputMethods.Keyboard) == InputMethods.Keyboard) UpdateKeys(ks, gameTime);
#endif

            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        private ButtonState GetVectorState(GamePadButton button, GamePadState state)
        {
            ButtonState ret = ButtonState.Released;
            bool down = false;
            float t = ClickThreshold;

            switch (button)
            {
                case GamePadButton.LeftStickLeft: down = state.ThumbSticks.Left.X < -t; break;
                case GamePadButton.LeftStickRight: down = state.ThumbSticks.Left.X > t; break;
                case GamePadButton.LeftStickUp: down = state.ThumbSticks.Left.Y > t; break;
                case GamePadButton.LeftStickDown: down = state.ThumbSticks.Left.Y < -t; break;

                case GamePadButton.RightStickLeft: down = state.ThumbSticks.Right.X < -t; break;
                case GamePadButton.RightStickRight: down = state.ThumbSticks.Right.X > t; break;
                case GamePadButton.RightStickUp: down = state.ThumbSticks.Right.Y > t; break;
                case GamePadButton.RightStickDown: down = state.ThumbSticks.Right.Y < -t; break;

                case GamePadButton.LeftTrigger: down = state.Triggers.Left > t; break;
                case GamePadButton.RightTrigger: down = state.Triggers.Right > t; break;
            }

            ret = down ? ButtonState.Pressed : ButtonState.Released;

            return ret;
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        private void UpdateGamePad(PlayerIndex playerIndex, GamePadState state, GameTime gameTime)
        {
            return; // Monogame 3.4           
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        private void BuildGamePadEvent(GamePadState state, GamePadButton button, ref GamePadEventArgs e)
        {
            e.State = state;
            e.Button = button;
            e.Vectors.LeftStick = new Vector2(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y);
            e.Vectors.RightStick = new Vector2(state.ThumbSticks.Right.X, state.ThumbSticks.Right.Y);
            e.Vectors.LeftTrigger = state.Triggers.Left;
            e.Vectors.RightTrigger = state.Triggers.Right;
        }
        ////////////////////////////////////////////////////////////////////////////    

        ////////////////////////////////////////////////////////////////////////////
        private void UpdateKeys(KeyboardState state, GameTime gameTime)
        {
#if (!XBOX && !XBOX_FAKE)

            KeyEventArgs e = new KeyEventArgs();

            //e.Caps = (((ushort)NativeMethods.GetKeyState(0x14)) & 0xffff) != 0;      

            foreach (Keys key in state.GetPressedKeys())
            {
                if (key == Keys.LeftAlt || key == Keys.RightAlt) e.Alt = true;
                else if (key == Keys.LeftShift || key == Keys.RightShift) e.Shift = true;
                else if (key == Keys.LeftControl || key == Keys.RightControl) e.Control = true;
            }

            foreach (InputKey key in keys)
            {
                if (key.Key == Keys.LeftAlt || key.Key == Keys.RightAlt ||
                    key.Key == Keys.LeftShift || key.Key == Keys.RightShift ||
                    key.Key == Keys.LeftControl || key.Key == Keys.RightControl)
                {
                    continue;
                }

                bool pressed = state.IsKeyDown(key.Key);

                double ms = gameTime.ElapsedGameTime.TotalMilliseconds;
                if (pressed) key.Countdown -= ms;

                if ((pressed) && (!key.Pressed))
                {
                    key.Pressed = true;
                    e.Key = key.Key;

                    if (KeyDown != null) KeyDown.Invoke(this, e);
                    if (KeyPress != null) KeyPress.Invoke(this, e);
                }
                else if ((!pressed) && (key.Pressed))
                {
                    key.Pressed = false;
                    key.Countdown = RepeatDelay;
                    e.Key = key.Key;

                    if (KeyUp != null) KeyUp.Invoke(this, e);
                }
                else if (key.Pressed && key.Countdown < 0)
                {
                    key.Countdown = RepeatRate;
                    e.Key = key.Key;

                    if (KeyPress != null) KeyPress.Invoke(this, e);
                }
            }
#endif
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        private Point RecalcPosition(Point pos)
        {
            return new Point((int)((pos.X - InputOffset.X) / InputOffset.RatioX), (int)((pos.Y - InputOffset.Y) / InputOffset.RatioY));
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////    
        private void AdjustPosition(ref MouseEventArgs e)
        {

        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        private void BuildMouseEvent(MouseState state, MouseButton button, ref MouseEventArgs e)
        {
            e.State = state;
            e.Button = button;

            e.Position = new Point(state.X, state.Y);
            AdjustPosition(ref e);

            e.Position = RecalcPosition(e.Position);
            e.State = new MouseState(e.Position.X, e.Position.Y, e.State.ScrollWheelValue, e.State.LeftButton, e.State.MiddleButton, e.State.RightButton, e.State.XButton1, e.State.XButton2);

            Point pos = RecalcPosition(new Point(mouseState.X, mouseState.Y));
            e.Difference = new Point(e.Position.X - pos.X, e.Position.Y - pos.Y);
        }
        ////////////////////////////////////////////////////////////////////////////    

        private void BuildMouseEvent(MouseState state, MouseButton button, MouseScrollDirection direction, ref MouseEventArgs e)
        {
            BuildMouseEvent(state, button, ref e);

            e.ScrollDirection = direction;
        }

        ////////////////////////////////////////////////////////////////////////////
        private void UpdateMouse(MouseState state, GameTime gameTime)
        {
#if (!XBOX && !XBOX_FAKE)

            if ((state.X != mouseState.X) || (state.Y != mouseState.Y))
            {
                MouseEventArgs e = new MouseEventArgs();

                MouseButton btn = MouseButton.None;
                if (state.LeftButton == ButtonState.Pressed) btn = MouseButton.Left;
                else if (state.RightButton == ButtonState.Pressed) btn = MouseButton.Right;
                else if (state.MiddleButton == ButtonState.Pressed) btn = MouseButton.Middle;
                else if (state.XButton1 == ButtonState.Pressed) btn = MouseButton.XButton1;
                else if (state.XButton2 == ButtonState.Pressed) btn = MouseButton.XButton2;

                BuildMouseEvent(state, btn, ref e);
                if (MouseMove != null)
                {
                    MouseMove.Invoke(this, e);
                }
            }

            // Mouse wheel position changed
            if (state.ScrollWheelValue != mouseState.ScrollWheelValue)
            {
                MouseEventArgs e = new MouseEventArgs();
                MouseScrollDirection direction = state.ScrollWheelValue < mouseState.ScrollWheelValue ? MouseScrollDirection.Down : MouseScrollDirection.Up;

                BuildMouseEvent(state, MouseButton.None, direction, ref e);

                if (MouseScroll != null)
                {
                    MouseScroll.Invoke(this, e);
                }
            }

            UpdateButtons(state, gameTime);

            mouseState = state;

#endif
        }
        ////////////////////////////////////////////////////////////////////////////        

        ////////////////////////////////////////////////////////////////////////////
        private void UpdateButtons(MouseState state, GameTime gameTime)
        {
#if (!XBOX && !XBOX_FAKE)

            MouseEventArgs e = new MouseEventArgs();

            foreach (InputMouseButton btn in mouseButtons)
            {
                ButtonState bs = ButtonState.Released;

                if (btn.Button == MouseButton.Left) bs = state.LeftButton;
                else if (btn.Button == MouseButton.Right) bs = state.RightButton;
                else if (btn.Button == MouseButton.Middle) bs = state.MiddleButton;
                else if (btn.Button == MouseButton.XButton1) bs = state.XButton1;
                else if (btn.Button == MouseButton.XButton2) bs = state.XButton2;
                else continue;

                bool pressed = (bs == ButtonState.Pressed);
                if (pressed)
                {
                    double ms = gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (pressed) btn.Countdown -= ms;
                }

                if ((pressed) && (!btn.Pressed))
                {
                    btn.Pressed = true;
                    BuildMouseEvent(state, btn.Button, ref e);

                    if (MouseDown != null) MouseDown.Invoke(this, e);
                    if (MousePress != null) MousePress.Invoke(this, e);
                }
                else if ((!pressed) && (btn.Pressed))
                {
                    btn.Pressed = false;
                    btn.Countdown = RepeatDelay;
                    BuildMouseEvent(state, btn.Button, ref e);

                    if (MouseUp != null) MouseUp.Invoke(this, e);
                }
                else if (btn.Pressed && btn.Countdown < 0)
                {
                    e.Button = btn.Button;
                    btn.Countdown = RepeatRate;
                    BuildMouseEvent(state, btn.Button, ref e);

                    if (MousePress != null) MousePress.Invoke(this, e);
                }
            }

#endif
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion

    }
    ////////////////////////////////////////////////////////////////////////////

    #endregion

}