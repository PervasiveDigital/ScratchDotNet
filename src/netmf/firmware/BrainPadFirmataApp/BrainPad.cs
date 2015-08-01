/*
Copyright 2015 GHI Electronics LLC

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

Note: This code is a work in progress.
 * rev 0: initial release.
 * rev 0.01: improved display and changed classes to static
 * rev 0.02: PlayFrequency() only plays values greater than zero
 *           simplified the Color class
*/

using GHI.IO;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.IO.Ports;

/// <summary>
/// The BrainPad class used with GHI Electronics' BrainPad.
/// </summary>
public static class BrainPad
{
    /// <summary>
    /// A constant value that is always true for endless looping.
    /// </summary>
    public const bool LOOPING = true;

    /// <summary>
    /// Provides colors for LEDs and displays.
    /// </summary>
    public static class Color
    {
        /// <summary>
        /// A palette of colors to choose from.
        /// </summary>
        public enum Palette : ushort
        {
            Aqua = 2047,
            Black = 0,
            Blue = 31,
            Fushcia = 63519,
            Gray = 21130,
            Green = 1024,
            Lime = 2016,
            Maroon = 32768,
            Navy = 16,
            Olive = 33792,
            Orange = 64480,
            Purple = 32784,
            Red = 63488,
            Silver = 50712,
            Teal = 1040,
            Violet = 30751,
            White = 65535,
            Yellow = 65504
        }

        /// <summary>
        /// Get the Red, Green and Blue values from a Palette color.
        /// </summary>
        /// <param name="color">Palette color.</param>
        /// <returns>The Red, Green and Blue values.</returns>
        public static byte[] RgbFromPalette(Color.Palette color)
        {
            var ushortColor = (ushort)color;
            var red = (byte)(((ushortColor & 0xF800) >> 11) * 8);
            var green = (byte)(((ushortColor & 0x7E0) >> 5) * 4);
            var blue = (byte)((ushortColor & 0x1F) * 8);
            return new byte[3] { red, green, blue };
        }
    }

    /// <summary>
    /// Prints a message to the output window.
    /// </summary>
    /// <param name="msg">The message you want displayed.</param>
    public static void DebugOutput(string msg)
    {
        Debug.Print(msg);
    }

    public static void DebugOutput(int i)
    {
        Debug.Print(i.ToString());
    }

    public static void DebugOutput(double d)
    {
        Debug.Print(d.ToString());
    }

    /// <summary>
    /// Controls the DC Motor on the Brainpad.
    /// </summary>
    public static class DcMotor
    {
        private static PWM _pwm = new PWM(Cpu.PWMChannel.PWM_3, 2175, 0, false);
        private static bool _started = false;

        /// <summary>
        /// Sets the speed at which the DC Motor rotates.
        /// </summary>
        /// <param name="speed">A value between -1 and 1.</param>
        public static void SetSpeed(double speed)
        {
            if ((speed > 1 | speed < -1))
                throw new ArgumentOutOfRangeException("speed", "speed must be between -1 and 1.");

            if ((speed == 1.0))
                speed = 0.99;

            if ((speed == -1.0))
                speed = -0.99;


            if ((_started))
                _pwm.Stop();

            _pwm.DutyCycle = speed;
            _pwm.Start();
            _started = true;
        }

        public static void Stop()
        {
            if (_started)
            {
                _pwm.Stop();
                _started = false;
            }
        }

    }

    /// <summary>
    /// Controls the Servo Motor on the BrainPad.
    /// </summary>
    public static class ServoMotor
    {

        private static PWM _pwm = new PWM(Cpu.PWMChannel.PWM_0, 20000, 1250, PWM.ScaleFactor.Microseconds, false);
        private static bool _started = false;

        /// <summary>
        /// Sets the position of the Servo Motor.
        /// </summary>
        /// <param name="degrees">A value between 0 and 180.</param>
        public static void SetPosition(int degrees)
        {
            if (degrees < 0 || degrees > 180)
                throw new Exception("The value " + degrees.ToString() + " is not valid, use 0 to 180 degrees only.");

            Deactivate();

            _pwm.Period = 20000;
            _pwm.Duration = (uint)(750 + ((degrees / 180.0) * 1500));

            Reactivate();

        }

        /// <summary>
        /// Stops the Servo Motor.
        /// </summary>
        public static void Deactivate()
        {
            if (_started)
            {
                _pwm.Stop();
                _started = false;
            }
        }

        /// <summary>
        /// Starts the Sero Motor.
        /// </summary>
        public static void Reactivate()
        {
            if (!_started)
            {
                _pwm.Start();
                _started = true;
            }
        }
    }


    /// <summary>
    /// Controls the Light Bulb on the BrainPad.
    /// </summary>
    public static class LightBulb
    {
        private static PWM[] _rgb = new PWM[3]{ 
            new PWM((Cpu.PWMChannel)10, 10000, 1, false),
            new PWM((Cpu.PWMChannel)9, 10000, 1, false),
            new PWM((Cpu.PWMChannel)8, 10000, 1, false)};
        private static bool _started = false;

        /// <summary>
        /// Sets the color of the Light Bulb.
        /// </summary>
        /// <param name="color">The <see cref="Palette" /> color to use.</param>
        public static void SetColor(Color.Palette color)
        {
            byte[] rgb = Color.RgbFromPalette(color);
            SetColor(rgb[0] / 255.0, rgb[1] / 255.0, rgb[2] / 255.0);
        }

        /// <summary>
        /// Sets the color of the Light Bulb.
        /// </summary>
        /// <param name="red">The red value between 0 and 1. Zero being off, 1 being 100% brightness.</param>
        /// <param name="green">The green value between 0 and 1. Zero being off, 1 being 100% brightness.</param>
        /// <param name="blue">The blue value between 0 and 1. Zero being off, 1 being 100% brightness.</param>
        public static void SetColor(double red, double green, double blue)
        {
            if (red < 0 | red > 1 | green < 0 | green > 1 | blue < 0 | blue > 1)
                throw new Exception("All values between 0 and 1.");

            if (_started)
                TurnOff();

            _rgb[0].DutyCycle = red;
            _rgb[1].DutyCycle = green;
            _rgb[2].DutyCycle = blue;

            TurnOn();
        }

        /// <summary>
        /// Turns on the Light Bulb.
        /// </summary>
        public static void TurnOn()
        {

            if (!_started)
            {
                _rgb[0].Start();
                _rgb[1].Start();
                _rgb[2].Start();
                _started = true;
            }
        }

        /// <summary>
        /// Turns off the Light Bulb.
        /// </summary>
        public static void TurnOff()
        {
            if (_started)
            {
                _rgb[0].Stop();
                _rgb[1].Stop();
                _rgb[2].Stop();
                _started = false;
            }
        }
    }

    /// <summary>
    /// Controls the touch pads on the BrainPad.
    /// </summary>
    public static class TouchPad
    {
        private static PulseFeedback[] _pins = new PulseFeedback[3];
        private static long[] _thresholds = { 0, 0, 0 };

        /// <summary>
        /// The three different pads.
        /// </summary>
        public enum Pad
        {
            Left = 0,
            Middle,
            Right
        }

        /// <summary>
        /// Determine if the pad has been touched.
        /// </summary>
        /// <param name="pad">The <see cref="Pad" /> to check.</param>
        /// <returns>If the pad has been touched (true) or not (false).</returns>
        public static bool IsTouched(Pad pad)
        {
            if ((RawRead(pad) > _thresholds[(int)pad]))
                return true;
            return false;
        }

        /// <summary>
        /// Reads the value of the pad.
        /// </summary>
        /// <param name="pad">The <see cref="Pad" /> to check.</param>
        /// <returns>The value of the pad.</returns>
        public static long RawRead(Pad pad)
        {
            if (_pins[0] == null)
            {
                _pins[0] = new PulseFeedback(PulseFeedback.Mode.DrainDuration, true, 10, (Cpu.Pin)32);
                _pins[1] = new PulseFeedback(PulseFeedback.Mode.DrainDuration, true, 10, (Cpu.Pin)33);
                _pins[2] = new PulseFeedback(PulseFeedback.Mode.DrainDuration, true, 10, (Cpu.Pin)34);
            }

            switch (pad)
            {
                case Pad.Left: return _pins[0].Read();
                case Pad.Middle: return _pins[1].Read();
                case Pad.Right: return _pins[2].Read();
                default: return 0;
            }
        }

        /// <summary>
        /// Sets the threshold (or value) that's considered a touch.
        /// </summary>
        /// <param name="pad">The <see cref="Pad" /> to check.</param>
        /// <param name="threshold">The value that's considered a touch.</param>
        public static void SetThreshold(Pad pad, long threshold)
        {
            _thresholds[(int)pad] = threshold;
        }
    }

    /// <summary>
    /// Controls the Buzzer on the BrainPad.
    /// </summary>
    public static class Buzzer
    {
        private static PWM _pwm = new PWM((Cpu.PWMChannel)13, 1, 0.5, false);
        private static bool _started;

        /// <summary>
        /// Notes the Buzzer can play.
        /// </summary>
        public enum Note
        {
            A = 880,
            Asharp = 932,
            B = 988,
            C = 1047,
            Csharp = 1109,
            D = 1175,
            Dsharp = 1244,
            E = 1319,
            F = 1397,
            Fsharp = 1480,
            G = 1568,
            Gsharp = 1661,
        }

        /// <summary>
        /// Plays a Note.
        /// </summary>
        /// <param name="note"></param>
        public static void PlayNote(Note note)
        {
            Stop();

            _pwm.Frequency = (double)note;
            _pwm.Start();
            _started = true;

        }

        /// <summary>
        /// Plays a Frequency.
        /// </summary>
        /// <param name="frequency">Frequency to play.</param>
        public static void PlayFrequency(int frequency)
        {
            Stop();

            if (frequency > 0)
            {
                _pwm.Frequency = frequency;
                _pwm.Start();
                _started = true;
            }
        }

        /// <summary>
        /// Stops the buzzer from playing.
        /// </summary>
        public static void Stop()
        {
            if (_started)
            {
                _pwm.Stop();
                _started = false;
            }
        }
    }

    // we don't need the button helpers for firmata, and in fact they interfere with direct IO processing
#if DEFINE_BUTTONS
    /// <summary>
    /// Controls the buttons on the BrainPad.
    /// </summary>
    public static class Button
    {
        private static int[] _pins = new int[4] { 15, 45, 26, 5 };
        private static InterruptPort[] _ports1 = new InterruptPort[4];

        /// <summary>
        /// Directional pad buttons.
        /// </summary>
        public enum DPad
        {
            Up = 0,
            Down,
            Left,
            Right
        }

        /// <summary>
        /// The state of a button.
        /// </summary>
        public enum State
        {
            Pressed = 0,
            NotPressed
        }

        static Button()
        {
            for (int i = 0; i < 4; i++)
            {
                _ports1[i] = new InterruptPort((Cpu.Pin)_pins[i], true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
                _ports1[i].OnInterrupt += Button_OnInterrupt;
            }
        }

        public static bool IsDownPressed()
        {
            return IsPressed(DPad.Down);
        }

        public static bool IsUpPressed()
        {
            return IsPressed(DPad.Up);
        }

        public static bool IsLeftPressed()
        {
            return IsPressed(DPad.Left);
        }

        public static bool IsRightPressed()
        {
            return IsPressed(DPad.Right);
        }

        public delegate void EventDelegate(DPad d, State state);
        public static event EventDelegate OnEvent;

        /// <summary>
        /// Determine if a button was pressed.
        /// </summary>
        /// <param name="button">The <see cref="DPad"/> button to check.</param>
        /// <returns></returns>
        public static bool IsPressed(DPad button)
        {

            int i = (int)button;
            return !_ports1[i].Read();

        }

        static void Button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            for (int i = 0; i < _pins.Length; i++)
            {
                if (_pins[i] == data1)
                {
                    if (OnEvent != null)
                        OnEvent.Invoke((DPad)i, (State)data2);
                }
            }
        }
    }
#endif

    /// <summary>
    /// Controls the Traffic Light.
    /// </summary>
    public static class TrafficLight
    {
        private static PWM[] _ryg = new PWM[3]{
            new PWM((Cpu.PWMChannel)4, 200, 1, false),
            new PWM(Cpu.PWMChannel.PWM_7, 200, 1, false),
            new PWM((Cpu.PWMChannel)14, 200, 1, false)
        };

        /// <summary>
        /// Turns the red light on.
        /// </summary>
        public static void RedLightOn()
        {
            TurnOn(Color.Palette.Red);
        }


        /// <summary>
        /// Turns the red light off.
        /// </summary>
        public static void RedLightOff()
        {
            TurnOff(Color.Palette.Red);
        }

        /// <summary>
        /// Turns the yellow light on.
        /// </summary>
        public static void YellowLightOn()
        {
            TurnOn(Color.Palette.Yellow);
        }

        /// <summary>
        /// Turns the yellow light off.
        /// </summary>
        public static void YellowLightOff()
        {
            TurnOff(Color.Palette.Yellow);
        }

        /// <summary>
        /// Turns the green light on.
        /// </summary>
        public static void GreenLightOn()
        {
            TurnOn(Color.Palette.Green);
        }

        /// <summary>
        /// Turns the green light off.
        /// </summary>
        public static void GreenLightOff()
        {
            TurnOff(Color.Palette.Green);
        }

        /// <summary>
        /// Turn on a color.
        /// </summary>
        /// <param name="color">Red, Yellow or Green</param>
        public static void TurnOn(BrainPad.Color.Palette color)
        {
            switch (color)
            {
                case BrainPad.Color.Palette.Red:
                    _ryg[0].DutyCycle = 1;
                    _ryg[0].Start();
                    break;
                case BrainPad.Color.Palette.Yellow:
                    _ryg[1].DutyCycle = 1;
                    _ryg[1].Start();
                    break;
                case BrainPad.Color.Palette.Green:
                    _ryg[2].DutyCycle = 1;
                    _ryg[2].Start();
                    break;
                default:
                    throw new Exception("color must be Red, Yellow or Green");
            }
        }

        /// <summary>
        /// Turn off a color.
        /// </summary>
        /// <param name="color">Red, Yellow or Green</param>
        public static void TurnOff(BrainPad.Color.Palette color)
        {

            switch (color)
            {
                case BrainPad.Color.Palette.Red:
                    _ryg[0].Stop();
                    break;
                case BrainPad.Color.Palette.Yellow:
                    _ryg[1].Stop();
                    break;
                case BrainPad.Color.Palette.Green:
                    _ryg[2].Stop();
                    break;
                default:
                    throw new Exception("color must be Red, Yellow or Green");
            }
        }

        /// <summary>
        /// Turns off all three lights.
        /// </summary>
        public static void TurnOffAll()
        {
            TurnOff(Color.Palette.Green);
            TurnOff(Color.Palette.Red);
            TurnOff(Color.Palette.Yellow);
        }

        /// <summary>
        /// Sets the brightness level of a color.
        /// </summary>
        /// <param name="color">The color to set.</param>
        /// <param name="level">Value between 0 and 1. Zero is off, 0.5 is half, one is full brightness.</param>
        public static void SetLevel(BrainPad.Color.Palette color, double level)
        {
            if (level < 0 | level > 1)
                throw new Exception("level must be a value between 0 and 1.");

            TurnOff(color);

            switch (color)
            {
                case BrainPad.Color.Palette.Red:
                    _ryg[0].DutyCycle = level;
                    _ryg[0].Start();
                    break;
                case BrainPad.Color.Palette.Yellow:
                    _ryg[1].DutyCycle = level;
                    _ryg[1].Start();
                    break;
                case BrainPad.Color.Palette.Green:
                    _ryg[2].DutyCycle = level;
                    _ryg[2].Start();
                    break;
                default:
                    throw new Exception("color must be Red, Yellow or Green");
            }
        }
    }

    ///// <summary>
    ///// Controls the Light Sensor on the BrainPad.
    ///// </summary>
    //public static class LightSensor
    //{
    //    private static AnalogInput _ain = new AnalogInput((Cpu.AnalogChannel)9);

    //    /// <summary>
    //    /// Get the level of brightness.
    //    /// </summary>
    //    /// <returns>A value respresenting the level of brightness.</returns>
    //    public static double GetLevel()
    //    {
    //        return _ain.Read();
    //    }
    //}

    ///// <summary>
    ///// Controls the Temperature Sensor on the BrainPad.
    ///// </summary>
    //public static class TemperatureSensor
    //{
    //    private static AnalogInput _ain = new AnalogInput((Cpu.AnalogChannel)8);

    //    /// <summary>
    //    /// Read the temperature.
    //    /// </summary>
    //    /// <returns>The temperature in Celsius.</returns>
    //    public static double ReadTemperature()
    //    {
    //        double sum = 0;
    //        for (int i = 0; i < 10; i++)
    //            sum += _ain.Read();
    //        double avg = sum / 10;

    //        double mVout = avg * 1000 * 3.3;
    //        double tempC = (mVout - 450.0) / 19.5;

    //        return tempC;
    //    }
    //}

    /// <summary>
    /// Controls the Accelerometer on the BrainPad.
    /// </summary>
    public static class Accelerometer
    {
        private static I2C _i2c;
        static Accelerometer()
        {
            _i2c = new I2C(0x1c);
            _i2c.Write(new byte[2] { 0x2a, 0x01 });
        }

        private static double ReadAxis(byte register)
        {

            byte[] data = new byte[2];
            _i2c.WriteRead(new byte[1] { register }, ref data);

            double value = data[0] << 2 | data[1] >> 6;
            if ((value > 511.0))
                value -= 1024.0;
            value /= 512.0;
            return value;
        }

        /// <summary>
        /// Reads the X axis.
        /// </summary>
        /// <returns>Value representing the X axis.</returns>
        public static double ReadX()
        {
            return ReadAxis(0x1);
        }

        /// <summary>
        /// Reads the Y axis.
        /// </summary>
        /// <returns>Value representing the Y axis.</returns>
        public static double ReadY()
        {
            return ReadAxis(0x3);
        }

        /// <summary>
        /// Reads the Z axis.
        /// </summary>
        /// <returns>Value representing the Z axis.</returns>
        public static double ReadZ()
        {
            return ReadAxis(0x5);
        }
    }

    /// <summary> 
    /// Controls the Display on the BrainPad.
    /// </summary>
    public static class Display
    {
        const int cs = (28);
        const int rs = 37;
        const int reset = 36;
        const int backlight = 4;

        static SPI.Configuration _spi_con;
        static SPI _spi;
        static OutputPort _rs;
        static OutputPort _reset;
        static OutputPort _backlight;
        private const byte ST7735_MADCTL = 0x36;
        private const byte MADCTL_MY = 0x80;
        private const byte MADCTL_MX = 0x40;
        private const byte MADCTL_MV = 0x20;
        private const byte MADCTL_BGR = 0x08;
        private static bool isBgr = false;

        static Display()
        {
            _rs = new OutputPort((Cpu.Pin)rs, false);

            _reset = new OutputPort((Cpu.Pin)reset, false);
            _backlight = new OutputPort((Cpu.Pin)backlight, true);
            _spi_con = new SPI.Configuration((Cpu.Pin)cs, false, 0, 0, false, true, 12000, SPI.SPI_module.SPI2);
            _spi = new SPI(_spi_con);

            Reset();

            //------------------------------------------------------------------//  
            //--------------------------Software Reset--------------------------//
            //------------------------------------------------------------------//

            WriteCommand(0x11);//Sleep exit 
            Thread.Sleep(120);

            // ST7735R Frame Rate
            WriteCommand(0xB1);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteCommand(0xB2);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteCommand(0xB3);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);

            WriteCommand(0xB4); // Column inversion 
            WriteData(0x07);

            // ST7735R Power Sequence
            WriteCommand(0xC0);
            WriteData(0xA2); WriteData(0x02); WriteData(0x84);
            WriteCommand(0xC1); WriteData(0xC5);
            WriteCommand(0xC2);
            WriteData(0x0A); WriteData(0x00);
            WriteCommand(0xC3);
            WriteData(0x8A); WriteData(0x2A);
            WriteCommand(0xC4);
            WriteData(0x8A); WriteData(0xEE);

            WriteCommand(0xC5); // VCOM 
            WriteData(0x0E);

            WriteCommand(0x36); // MX, MY, RGB mode
            WriteData(MADCTL_MX | MADCTL_MY | MADCTL_BGR);

            // ST7735R Gamma Sequence
            WriteCommand(0xe0);
            WriteData(0x0f); WriteData(0x1a);
            WriteData(0x0f); WriteData(0x18);
            WriteData(0x2f); WriteData(0x28);
            WriteData(0x20); WriteData(0x22);
            WriteData(0x1f); WriteData(0x1b);
            WriteData(0x23); WriteData(0x37); WriteData(0x00);

            WriteData(0x07);
            WriteData(0x02); WriteData(0x10);
            WriteCommand(0xe1);
            WriteData(0x0f); WriteData(0x1b);
            WriteData(0x0f); WriteData(0x17);
            WriteData(0x33); WriteData(0x2c);
            WriteData(0x29); WriteData(0x2e);
            WriteData(0x30); WriteData(0x30);
            WriteData(0x39); WriteData(0x3f);
            WriteData(0x00); WriteData(0x07);
            WriteData(0x03); WriteData(0x10);

            WriteCommand(0x2a);
            WriteData(0x00); WriteData(0x00);
            WriteData(0x00); WriteData(0x7f);
            WriteCommand(0x2b);
            WriteData(0x00); WriteData(0x00);
            WriteData(0x00); WriteData(0x9f);

            WriteCommand(0xF0); //Enable test command  
            WriteData(0x01);
            WriteCommand(0xF6); //Disable ram power save mode 
            WriteData(0x00);

            WriteCommand(0x3A); //65k mode 
            WriteData(0x05);

            // rotate
            WriteCommand(ST7735_MADCTL);
            WriteData((byte)(MADCTL_MV | MADCTL_MY | (isBgr ? MADCTL_BGR : 0)));

            WriteCommand(0x29); //Display on
            Thread.Sleep(50);
        }

        private static void WriteData(byte[] data)
        {
            _rs.Write(true);
            _spi.Write(data);
        }

        private static byte[] sb = new byte[1];

        private static void WriteCommand(byte c)
        {
            sb[0] = c;
            _rs.Write(false);
            _spi.Write(sb);
        }

        private static void WriteData(byte d)
        {
            sb[0] = d;
            _rs.Write(true);
            _spi.Write(sb);
        }

        private static void Reset()
        {
            _reset.Write(false);
            Thread.Sleep(300);
            _reset.Write(true);
            Thread.Sleep(1000);
        }

        private static byte[] b4 = new byte[4];

        private static void SetClip(int x, int y, int w, int h)
        {

            ushort x_end = (ushort)(x + w - 1);
            ushort y_end = (ushort)(y + h - 1);

            WriteCommand(0x2A);

            _rs.Write(true);
            // b4[0] = 0;
            b4[1] = (byte)x;
            // b4[2] = 0;
            b4[3] = (byte)x_end;
            _spi.Write(b4);

            WriteCommand(0x2B);
            _rs.Write(true);
            //b4[0] = 0;
            b4[1] = (byte)y;
            //b4[2] = 0;
            b4[3] = (byte)y_end;
            _spi.Write(b4);
        }

        static byte[] temp = new byte[160 * 2 * 16];

        /// <summary>
        /// Clears the Display.
        /// </summary>
        public static void Clear()
        {
            SetClip(0, 0, 160, 128);
            WriteCommand(0x2C);
            for (int i = 0; i < 128 / 16; i++)
                WriteData(temp);
        }

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="data">The image as a byte array.</param>
        public static void DrawImage(byte[] data)
        {
            WriteCommand(0x2C);
            WriteData(data);
        }

        public static void DrawImage(int x, int y, Image img)
        {
            SetClip(x, y, img._width, img._height);
            DrawImage(img.Pixels);
        }

        public class Image
        {
            public int _width;
            public int _height;
            public byte[] Pixels;

            public Image(int width, int height)
            {
                _width = width;
                _height = height;
                Pixels = new byte[width * height * 2];
            }

            public void SetPixel(int x, int y, Color.Palette color)
            {
                if (x > _width || x < 0)
                    return;
                if (y > _height || y < 0)
                    return;

                Pixels[(x * _width + y) * 2 + 0] = (byte)((ushort)color >> 8);
                Pixels[(x * _width + y) * 2 + 1] = (byte)color;
            }
        };

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="color">The color to use.</param>
        public static void DrawFillRect(int x, int y, int w, int h, Color.Palette color)
        {
            SetClip(x, y, w, h);

            byte[] colors = new byte[w * h * 2];
            for (int i = 0; i < colors.Length; i += 2)
            {
                colors[i] = (byte)(((ushort)color >> 8) & 0xFF);
                colors[i + 1] = (byte)(((ushort)color >> 0) & 0xFF);
            }

            DrawImage(colors);
        }

        /// <summary>
        /// Turns the backlight on.
        /// </summary>
        public static void TurnOn()
        {
            _backlight.Write(true);
        }

        /// <summary>
        /// Turns the backlight off.
        /// </summary>
        public static void TurnOff()
        {
            _backlight.Write(false);
        }

        static byte[] b2 = new byte[2];

        /// <summary>
        /// Draws a pixel.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="color">The color to use.</param>
        public static void SetPixel(int x, int y, Color.Palette color)
        {
            SetClip(x, y, 1, 1);
            b2[0] = (byte)((ushort)color >> 8);
            b2[1] = (byte)((ushort)color >> 0);
            DrawImage(b2);
        }

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="x0">Starting X coordinate.</param>
        /// <param name="y0">Starting Y coordinate.</param>
        /// <param name="x1">Ending X coordinate.</param>
        /// <param name="y1">Ending Y coordinate.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawLine(int x0, int y0, int x1, int y1, Color.Palette color)
        {
            int t;
            bool steep = System.Math.Abs(y1 - y0) > System.Math.Abs(x1 - x0);
            if (steep)
            {
                t = x0;
                x0 = y0;
                y0 = t;
                t = x1;
                x1 = y1;
                y1 = t;
            }

            if (x0 > x1)
            {
                t = x0;
                x0 = x1;
                x1 = t;

                t = y0;
                y0 = y1;
                y1 = t;
            }

            int dx, dy;
            dx = x1 - x0;
            dy = System.Math.Abs(y1 - y0);

            int err = (dx / 2);
            int ystep;

            if (y0 < y1)
            {
                ystep = 1;
            }
            else
            {
                ystep = -1;
            }

            for (; x0 < x1; x0++)
            {
                if (steep)
                {
                    SetPixel(y0, x0, color);
                }
                else
                {
                    SetPixel(x0, y0, color);
                }
                err -= dy;
                if (err < 0)
                {
                    y0 += (byte)ystep;
                    err += dx;
                }
            }
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="x0">Starting X coordinate.</param>
        /// <param name="y0">Starting Y coordinate.</param>
        /// <param name="r">The radius.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawCircle(int x0, int y0, int r, Color.Palette color)
        {
            int f = 1 - r;
            int ddF_x = 1;
            int ddF_y = -2 * r;
            int x = 0;
            int y = r;

            SetPixel(x0, y0 + r, color);
            SetPixel(x0, y0 - r, color);
            SetPixel(x0 + r, y0, color);
            SetPixel(x0 - r, y0, color);

            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;

                SetPixel(x0 + x, y0 + y, color);
                SetPixel(x0 - x, y0 + y, color);
                SetPixel(x0 + x, y0 - y, color);
                SetPixel(x0 - x, y0 - y, color);

                SetPixel(x0 + y, y0 + x, color);
                SetPixel(x0 - y, y0 + x, color);
                SetPixel(x0 + y, y0 - x, color);
                SetPixel(x0 - y, y0 - x, color);
            }
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="color">The color to use.</param>
        public static void DrawRect(int x, int y, int w, int h, Color.Palette color)
        {
            // chnage to be like the fill rect for speed
            for (int i = x; i < x + w; i++)
            {
                SetPixel(i, y, color);
                SetPixel(i, y + h - 1, color);
            }

            for (int i = y; i < y + h; i++)
            {
                SetPixel(x, i, color);
                SetPixel(x + w - 1, i, color);
            }
        }

        static byte[] font = new byte[95 * 5] {
	        0x00, 0x00, 0x00, 0x00, 0x00, /* Space	0x20 */
	        0x00, 0x00, 0x4f, 0x00, 0x00, /* ! */
	        0x00, 0x07, 0x00, 0x07, 0x00, /* " */
	        0x14, 0x7f, 0x14, 0x7f, 0x14, /* # */
	        0x24, 0x2a, 0x7f, 0x2a, 0x12, /* $ */
	        0x23, 0x13, 0x08, 0x64, 0x62, /* % */
	        0x36, 0x49, 0x55, 0x22, 0x20, /* & */
	        0x00, 0x05, 0x03, 0x00, 0x00, /* ' */
	        0x00, 0x1c, 0x22, 0x41, 0x00, /* ( */
	        0x00, 0x41, 0x22, 0x1c, 0x00, /* ) */
	        0x14, 0x08, 0x3e, 0x08, 0x14, /* // */
	        0x08, 0x08, 0x3e, 0x08, 0x08, /* + */
	        0x50, 0x30, 0x00, 0x00, 0x00, /* , */
	        0x08, 0x08, 0x08, 0x08, 0x08, /* - */
	        0x00, 0x60, 0x60, 0x00, 0x00, /* . */
	        0x20, 0x10, 0x08, 0x04, 0x02, /* / */
	        0x3e, 0x51, 0x49, 0x45, 0x3e, /* 0		0x30 */
	        0x00, 0x42, 0x7f, 0x40, 0x00, /* 1 */
	        0x42, 0x61, 0x51, 0x49, 0x46, /* 2 */
	        0x21, 0x41, 0x45, 0x4b, 0x31, /* 3 */
	        0x18, 0x14, 0x12, 0x7f, 0x10, /* 4 */
	        0x27, 0x45, 0x45, 0x45, 0x39, /* 5 */
	        0x3c, 0x4a, 0x49, 0x49, 0x30, /* 6 */
	        0x01, 0x71, 0x09, 0x05, 0x03, /* 7 */
	        0x36, 0x49, 0x49, 0x49, 0x36, /* 8 */
	        0x06, 0x49, 0x49, 0x29, 0x1e, /* 9 */
	        0x00, 0x36, 0x36, 0x00, 0x00, /* : */
	        0x00, 0x56, 0x36, 0x00, 0x00, /* ; */
	        0x08, 0x14, 0x22, 0x41, 0x00, /* < */
	        0x14, 0x14, 0x14, 0x14, 0x14, /* = */
	        0x00, 0x41, 0x22, 0x14, 0x08, /* > */
	        0x02, 0x01, 0x51, 0x09, 0x06, /* ? */
	        0x3e, 0x41, 0x5d, 0x55, 0x1e, /* @		0x40 */
	        0x7e, 0x11, 0x11, 0x11, 0x7e, /* A */
	        0x7f, 0x49, 0x49, 0x49, 0x36, /* B */
	        0x3e, 0x41, 0x41, 0x41, 0x22, /* C */
	        0x7f, 0x41, 0x41, 0x22, 0x1c, /* D */
	        0x7f, 0x49, 0x49, 0x49, 0x41, /* E */
	        0x7f, 0x09, 0x09, 0x09, 0x01, /* F */
	        0x3e, 0x41, 0x49, 0x49, 0x7a, /* G */
	        0x7f, 0x08, 0x08, 0x08, 0x7f, /* H */
	        0x00, 0x41, 0x7f, 0x41, 0x00, /* I */
	        0x20, 0x40, 0x41, 0x3f, 0x01, /* J */
	        0x7f, 0x08, 0x14, 0x22, 0x41, /* K */
	        0x7f, 0x40, 0x40, 0x40, 0x40, /* L */
	        0x7f, 0x02, 0x0c, 0x02, 0x7f, /* M */
	        0x7f, 0x04, 0x08, 0x10, 0x7f, /* N */
	        0x3e, 0x41, 0x41, 0x41, 0x3e, /* O */
	        0x7f, 0x09, 0x09, 0x09, 0x06, /* P		0x50 */
	        0x3e, 0x41, 0x51, 0x21, 0x5e, /* Q */
	        0x7f, 0x09, 0x19, 0x29, 0x46, /* R */
	        0x26, 0x49, 0x49, 0x49, 0x32, /* S */
	        0x01, 0x01, 0x7f, 0x01, 0x01, /* T */
	        0x3f, 0x40, 0x40, 0x40, 0x3f, /* U */
	        0x1f, 0x20, 0x40, 0x20, 0x1f, /* V */
	        0x3f, 0x40, 0x38, 0x40, 0x3f, /* W */
	        0x63, 0x14, 0x08, 0x14, 0x63, /* X */
	        0x07, 0x08, 0x70, 0x08, 0x07, /* Y */
	        0x61, 0x51, 0x49, 0x45, 0x43, /* Z */
	        0x00, 0x7f, 0x41, 0x41, 0x00, /* [ */
	        0x02, 0x04, 0x08, 0x10, 0x20, /* \ */
	        0x00, 0x41, 0x41, 0x7f, 0x00, /* ] */
	        0x04, 0x02, 0x01, 0x02, 0x04, /* ^ */
	        0x40, 0x40, 0x40, 0x40, 0x40, /* _ */
	        0x00, 0x00, 0x03, 0x05, 0x00, /* `		0x60 */
	        0x20, 0x54, 0x54, 0x54, 0x78, /* a */
	        0x7F, 0x44, 0x44, 0x44, 0x38, /* b */
	        0x38, 0x44, 0x44, 0x44, 0x44, /* c */
	        0x38, 0x44, 0x44, 0x44, 0x7f, /* d */
	        0x38, 0x54, 0x54, 0x54, 0x18, /* e */
	        0x04, 0x04, 0x7e, 0x05, 0x05, /* f */
	        0x08, 0x54, 0x54, 0x54, 0x3c, /* g */
	        0x7f, 0x08, 0x04, 0x04, 0x78, /* h */
	        0x00, 0x44, 0x7d, 0x40, 0x00, /* i */
	        0x20, 0x40, 0x44, 0x3d, 0x00, /* j */
	        0x7f, 0x10, 0x28, 0x44, 0x00, /* k */
	        0x00, 0x41, 0x7f, 0x40, 0x00, /* l */
	        0x7c, 0x04, 0x7c, 0x04, 0x78, /* m */
	        0x7c, 0x08, 0x04, 0x04, 0x78, /* n */
	        0x38, 0x44, 0x44, 0x44, 0x38, /* o */
	        0x7c, 0x14, 0x14, 0x14, 0x08, /* p		0x70 */
	        0x08, 0x14, 0x14, 0x14, 0x7c, /* q */
	        0x7c, 0x08, 0x04, 0x04, 0x00, /* r */
	        0x48, 0x54, 0x54, 0x54, 0x24, /* s */
	        0x04, 0x04, 0x3f, 0x44, 0x44, /* t */
	        0x3c, 0x40, 0x40, 0x20, 0x7c, /* u */
	        0x1c, 0x20, 0x40, 0x20, 0x1c, /* v */
	        0x3c, 0x40, 0x30, 0x40, 0x3c, /* w */
	        0x44, 0x28, 0x10, 0x28, 0x44, /* x */
	        0x0c, 0x50, 0x50, 0x50, 0x3c, /* y */
	        0x44, 0x64, 0x54, 0x4c, 0x44, /* z */
	        0x08, 0x36, 0x41, 0x41, 0x00, /* { */
	        0x00, 0x00, 0x77, 0x00, 0x00, /* | */
	        0x00, 0x41, 0x41, 0x36, 0x08, /* } */
	        0x08, 0x08, 0x2a, 0x1c, 0x08  /* ~ */
        };

        private static byte[] chartemp = new byte[8 * 2];
        private static void Draw8Pixels(int x, int y, byte c, ushort color)
        {
            Array.Clear(chartemp, 0, chartemp.Length);

            for (int i = 0; i < 8; i++)
            {
                if (((c >> i) & 1) > 0)
                {
                    chartemp[i * 2 + 0] = (byte)(color >> 8);
                    chartemp[i * 2 + 1] = (byte)(color >> 0);
                }
            }

            SetClip(x, y, 1, 8);
            DrawImage(chartemp);
        }

        private static byte[] xlargechartemp = new byte[8 * 2 * 2 * 2];
        private static void Draw8XLargePixels(int x, int y, byte c, ushort color)
        {
            Array.Clear(xlargechartemp, 0, xlargechartemp.Length);

            for (int i = 0; i < 8; i++)
            {
                if (((c >> i) & 1) > 0)
                {
                    byte upper = (byte)(color >> 8);
                    byte lower = (byte)(color >> 0);
                    xlargechartemp[i * 8 + 0] = upper;
                    xlargechartemp[i * 8 + 1] = lower;
                    xlargechartemp[i * 8 + 2] = upper;
                    xlargechartemp[i * 8 + 3] = lower;

                    xlargechartemp[i * 8 + 4] = upper;
                    xlargechartemp[i * 8 + 5] = lower;
                    xlargechartemp[i * 8 + 6] = upper;
                    xlargechartemp[i * 8 + 7] = lower;
                }
            }

            SetClip(x, y, 1, 32);
            DrawImage(xlargechartemp);

            SetClip(x + 1, y, 1, 32);
            DrawImage(xlargechartemp);

            SetClip(x + 2, y, 1, 32);
            DrawImage(xlargechartemp);

            SetClip(x + 3, y, 1, 32);
            DrawImage(xlargechartemp);
        }

        private static byte[] largechartemp = new byte[8 * 2 * 2];
        private static void Draw8LargePixels(int x, int y, byte c, ushort color)
        {
            Array.Clear(largechartemp, 0, largechartemp.Length);

            for (int i = 0; i < 8; i++)
            {
                if (((c >> i) & 1) > 0)
                {
                    byte upper = (byte)(color >> 8);
                    byte lower = (byte)(color >> 0);
                    largechartemp[i * 4 + 0] = upper;
                    largechartemp[i * 4 + 1] = lower;
                    largechartemp[i * 4 + 2] = upper;
                    largechartemp[i * 4 + 3] = lower;
                }
            }

            SetClip(x, y, 1, 16);
            DrawImage(largechartemp);

            SetClip(x + 1, y, 1, 16);
            DrawImage(largechartemp);
        }

        /// <summary>
        /// Draws a character in large font.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="c">The character to draw.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawLargeCharacter(int x, int y, char c, Color.Palette color)
        {
            if (c > 126 || c < 32)
                return;

            c = (char)(c - 32);

            for (int i = 0; i < 5; i++)
            {
                Draw8LargePixels(x + i * 2, y, font[5 * c + i], (ushort)color);
            }
        }

        public static void DrawXLargeCharacter(int x, int y, char c, Color.Palette color)
        {
            if (c > 126 || c < 32)
                return;

            c = (char)(c - 32);

            for (int i = 0; i < 5; i++)
            {
                Draw8XLargePixels(x + i * 4, y, font[5 * c + i], (ushort)color);
            }
        }

        /// <summary>
        /// Draws a string in large font.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="str">The string to draw.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawLargeString(int x, int y, string str, Color.Palette color)
        {
            for (int i = 0; i < str.Length; i++)
            {
                DrawLargeCharacter(x + i * 7 * 2, y, str[i], color);
            }
        }

        public static void DrawXLargeString(int x, int y, string str, Color.Palette color)
        {
            for (int i = 0; i < str.Length; i++)
            {
                DrawXLargeCharacter(x + i * 7 * 4, y, str[i], color);
            }
        }

        /// <summary>
        /// Draws a character in small font.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="c">The character to draw.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawCharacter(int x, int y, char c, Color.Palette color)
        {
            if (c > 126 || c < 32)
                return;

            c = (char)(c - 32);

            for (int i = 0; i < 5; i++)
                Draw8Pixels(x + i, y, font[5 * c + i], (ushort)color);
        }

        /// <summary>
        /// Draws a string in small font.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="str">The string to draw.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawString(int x, int y, string str, Color.Palette color)
        {
            for (int i = 0; i < str.Length; i++)
            {
                DrawCharacter(x + i * 6, y, str[i], color);
            }
        }
    }

    /// <summary>
    /// Controls how long the BrainPad waits.
    /// </summary>
    public static class Wait
    {
        /// <summary>
        /// Tells the BrainPad to wait for a number of seconds.
        /// </summary>
        /// <param name="sec">Seconds to wait.</param>
        public static void Seconds(double sec)
        {
            Thread.Sleep((int)(sec * 1000));
        }

        /// <summary>
        /// Tells the BrainPad to wait for number of milliseconds.
        /// </summary>
        /// <param name="ms">Milliseconds to wait.</param>
        public static void Milliseconds(int ms)
        {
            Thread.Sleep(ms);
        }
    }

    /// <summary>
    /// Allows ease-of-use with I2C.
    /// </summary>
    private class I2C
    {

        private I2CDevice _device;
        private I2CDevice.Configuration _configuration;
        private int _clockRateKhz = 400;
        private int _timeout = 1000;

        public I2C(byte address)
        {
            _configuration = new I2CDevice.Configuration(address, _clockRateKhz);
            _device = new I2CDevice(_configuration);
        }

        public int Write(byte[] buffer)
        {
            I2CDevice.I2CTransaction[] xaction = new I2CDevice.I2CTransaction[1];
            xaction[0] = I2CDevice.CreateWriteTransaction(buffer);
            return _device.Execute(xaction, _timeout);
        }

        public int Read(ref byte[] buffer)
        {
            I2CDevice.I2CTransaction[] xaction = new I2CDevice.I2CTransaction[1];
            xaction[0] = I2CDevice.CreateReadTransaction(buffer);
            return _device.Execute(xaction, _timeout);
        }

        public int WriteRead(byte[] writeBuffer, ref byte[] readBuffer)
        {
            I2CDevice.I2CTransaction[] xaction = new I2CDevice.I2CTransaction[2];
            xaction[0] = I2CDevice.CreateWriteTransaction(writeBuffer);
            xaction[1] = I2CDevice.CreateReadTransaction(readBuffer);
            return _device.Execute(xaction, _timeout);
        }
    }
}