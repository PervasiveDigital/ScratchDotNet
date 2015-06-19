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
public class BrainPad
{
    /// <summary>
    /// A constant value that is always true for endless looping.
    /// </summary>
    public const bool LOOPING = true;

    /// <summary>
    /// Provides colors for LEDs and displays.
    /// </summary>
    public class Color
    {
        private static ushort _white = FromRGB(255, 255, 255);
        private static ushort _black = FromRGB(0, 0, 0);
        private static ushort _red = FromRGB(255, 0, 0);
        private static ushort _green = FromRGB(0, 255, 0);
        private static ushort _blue = FromRGB(0, 0, 255);
        private static ushort _yellow = FromRGB(255, 255, 0);
        private static ushort _fuchsia = FromRGB(255, 0, 255);
        private static ushort _cyan = FromRGB(0, 255, 255);
        private static ushort _brown = FromRGB(0xA5, 0x2A, 0x2A);
        private static ushort _orange = FromRGB(0xFF, 0xA5, 0x00);
        private static ushort _pink = FromRGB(0xFF, 0xC0, 0xCB);
        private static ushort _gray = FromRGB(0x80, 0x80, 0x80);
        private static ushort _purple = FromRGB(0x80, 0x00, 0x80);

        /// <summary>
        /// A palette of colors to choose from.
        /// </summary>
        public enum Palette
        {
            White = 0,
            Black,
            Red,
            Green,
            Blue,
            Yellow,
            Fuchsia,
            Cyan,
            Brown,
            Orange,
            Pink,
            Gray,
            Purple
        }

        private static ushort FromRGB(byte R, byte G, byte B)
        {
            R >>= (8 - 5);
            G >>= (8 - 6);
            B >>= (8 - 5);

            ushort color = (ushort)(R << (5 + 6));
            color |= (ushort)(G << 5);
            color |= B;

            return color;
        }

        private static byte[] UShortToRGB(ushort value)
        {
            byte R = (byte)(((value & 0xF800) >> 11) * 8);
            byte G = (byte)(((value & 0x7E0) >> 5) * 4);
            byte B = (byte)((value & 0x1F) * 8);

            return new byte[3] { R, G, B };
        }

        /// <summary>
        /// Converts a palette color into it's numeric value.
        /// </summary>
        /// <param name="color">The <see cref="Palette" /> you wish to convert.</param>
        /// <returns>A numeric value represeting the color.</returns>
        public static ushort FromPalette(Palette color)
        {
            switch (color)
            {
                case Palette.White: return _white;
                case Palette.Black: return _black;
                case Palette.Red: return _red;
                case Palette.Green: return _green;
                case Palette.Blue: return _blue;
                case Palette.Yellow: return _yellow;
                case Palette.Fuchsia: return _fuchsia;
                case Palette.Cyan: return _cyan;
                case Palette.Brown: return _brown;
                case Palette.Orange: return _orange;
                case Palette.Pink: return _pink;
                case Palette.Gray: return _gray;
                case Palette.Purple: return _purple;
                default: return 0;
            }
        }
    }

    /// <summary>
    /// The font size to use when drawing strings and characters to the display.
    /// </summary>
    public enum FontSize
    {
        Small,
        Large
    }

    /// <summary>
    /// Prints a message to the output window.
    /// </summary>
    /// <param name="msg">The message you want displayed.</param>
    public static void DebugPrint(string msg)
    {
        Debug.Print(msg);
    }

    /// <summary>
    /// Controls the DC Motor on the Brainpad.
    /// </summary>
    public class DcMotor
    {
        private static PWM _pwm;
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

            if (_pwm == null)
            {
                _pwm = new PWM(Cpu.PWMChannel.PWM_3, 2175, speed, false);
                _pwm.Start();
                _started = true;
            }
            else
            {
                if ((_started))
                    _pwm.Stop();

                _pwm.DutyCycle = speed;
                _pwm.Start();
                _started = true;
            }
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
    public class ServoMotor
    {

        private static PWM _pwm;
        private static bool _started = false;

        private static void Initialize()
        {
            if (_pwm == null)
                _pwm = new PWM(Cpu.PWMChannel.PWM_4, 20000, 1250, PWM.ScaleFactor.Microseconds, false);
        }

        /// <summary>
        /// Sets the position of the Servo Motor.
        /// </summary>
        /// <param name="degrees">A value between 0 and 180.</param>
        public static void SetPosition(int degrees)
        {
            if (degrees < 0)
                throw new Exception("The value " + degrees.ToString() + " is not valid, use 0 to 180 degrees only.");

            if (degrees > 180)
                throw new Exception("The value " + degrees.ToString() + " is not valid, use 0 to 180 degrees only.");

            Initialize();
            Deacticvate();

            _pwm.Period = 20000;
            _pwm.Duration = (uint)(1250 + ((degrees / 180) * 500));

            Reactivate();

        }

        /// <summary>
        /// Stops the Servo Motor.
        /// </summary>
        public static void Deacticvate()
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
            Initialize();

            if (!_started)
            {
                _pwm.Start();
                _started = true;
            }
        }
    }

    /// <summary>
    /// Controls the communication on the BrainPad.
    /// </summary>
    public class Communication
    {
        public static SerialPort _port;

        /// <summary>
        /// Write a string to be sent over the communication port.
        /// </summary>
        /// <param name="str">The string you wish to write.</param>
        public static void Write(string str)
        {
            if (_port == null)
                _port = new SerialPort("COM2");
                
            if (!_port.IsOpen)
                _port.Open();

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
            _port.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Reads the communication port.
        /// </summary>
        /// <returns>The string you wrote.</returns>
        public static string Read()
        {
            byte[] buffer = new byte[10];
            _port.Read(buffer, 0, buffer.Length);
            _port.Flush();

            return new string(System.Text.UTF8Encoding.UTF8.GetChars(buffer));
        }
    }

    /// <summary>
    /// Provides expansion header pins.
    /// </summary>
    public class Expansion
    {
        /// <summary>
        /// PWM pins.
        /// </summary>
        public enum PWMPin
        {
            E2 = 11,
            E3 = 12,
            E4 = 6,
            E5 = 5
        }

        /// <summary>
        /// Analog input pins.
        /// </summary>
        public enum AnalogInputPin
        {
            E4 = 3,
            E5 = 2,
            E6 = 7,
            E7 = 6,
            E15 = 13
        }

        /// <summary>
        /// SPI pins.
        /// </summary>
        public enum SPIPin
        {
            E11_CS = 50,
            E12_SCK = 19,
            E13_MISO = 20,
            E14_MOSI = 21
        }
    }

    /// <summary>
    /// Controls the Light Bulb on the BrainPad.
    /// </summary>
    public class LightBulb
    {
        private static PWM[] _rgb = new PWM[3];
        private static bool _started = false;

        private static void Initialize()
        {
            if (_rgb[0] == null)
            {
                _rgb[0] = new PWM((Cpu.PWMChannel)10, 10000, 1, false);
                _rgb[1] = new PWM((Cpu.PWMChannel)9, 10000, 1, false);
                _rgb[2] = new PWM(Cpu.PWMChannel.PWM_0, 10000, 1, false);
            }

        }

        /// <summary>
        /// Sets the color of the Light Bulb.
        /// </summary>
        /// <param name="color">The <see cref="Palette" /> color to use.</param>
        public static void SetColor(Color.Palette color)
        {
            switch (color)
            {
                case Color.Palette.Red:
                    SetColor(1, 0, 0);
                    break;
                case Color.Palette.Green:
                    SetColor(0, 1, 0);
                    break;
                case Color.Palette.Blue:
                    SetColor(0, 0, 1);
                    break;
                case Color.Palette.Yellow:
                    SetColor(1, 1, 0);
                    break;
                case Color.Palette.Fuchsia:
                    SetColor(1, 0, 1);
                    break;
                case Color.Palette.Cyan:
                    SetColor(0, 1, 1);
                    break;
            }
        }

        /// <summary>
        /// Sets the color of the Light Bulb.
        /// </summary>
        /// <param name="red">The red value between 0 and 1. Zero being off, 1 being 100% brightness.</param>
        /// <param name="green">The green value between 0 and 1. Zero being off, 1 being 100% brightness.</param>
        /// <param name="blue">The blue value between 0 and 1. Zero being off, 1 being 100% brightness.</param>
        public static void SetColor(double red, double green, double blue)
        {
            if (red < 0 | red > 1)
                throw new Exception("red must be a value between 0 and 1.");
            if (green < 0 | green > 1)
                throw new Exception("green must be a value between 0 and 1.");
            if (blue < 0 | blue > 1)
                throw new Exception("blue must be a value between 0 and 1.");

            Initialize();

            if (_started)
                TurnOff();

            _rgb[0].DutyCycle = red;
            _rgb[1].DutyCycle = green;
            _rgb[2].DutyCycle = blue;

            if (!_started)
                TurnOn();
        }

        /// <summary>
        /// Turns on the Light Bulb.
        /// </summary>
        public static void TurnOn()
        {
            Initialize();

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
    public class TouchPad
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
    public class Buzzer
    {
        private static PWM _pwm;
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
            None = 0
        }

        /// <summary>
        /// Instructions the buzzer to play a frequency for a duration, followed by a delay.
        /// </summary>
        public class Tone
        {
            /// <summary>
            /// Creates a Tone.
            /// </summary>
            /// <param name="freq">Frequency to play.</param>
            /// <param name="duration">How long to play the Frequency.</param>
            /// <param name="delay">How long to pause after the Frequency finishes playing.</param>
            public Tone(int freq, int duration, int delay)
            {
                Frequency = freq;
                Duration = duration;
                Delay = delay;
            }

            /// <summary>
            /// Frequency to play.
            /// </summary>
            public int Frequency { get; set; }

            /// <summary>
            /// How long to play the Frequency.
            /// </summary>
            public int Duration { get; set; }

            /// <summary>
            /// How long to pause after the Frequency finishes playing.
            /// </summary>
            public int Delay { get; set; }
        }

        private static void Initialize()
        {
            if (_pwm == null)
                _pwm = new PWM((Cpu.PWMChannel)13, 1, 0.5, false);
        }

        /// <summary>
        /// Plays a Note.
        /// </summary>
        /// <param name="note"></param>
        public static void PlayNote(Note note)
        {
            Initialize();
            Stop();

            _pwm.Frequency = (double)note;
            _pwm.Start();
            _started = true;

        }

        /// <summary>
        /// Plays a Tone.
        /// </summary>
        /// <param name="tone">The <see cref="Tone"/> to play.</param>
        public static void PlayTone(Tone tone)
        {
            Initialize();
            Stop();

            _pwm.Frequency = tone.Frequency;
            _pwm.Start();
            _started = true;

            Thread.Sleep(tone.Duration);

            if (tone.Delay > 0)
            {
                Stop();
                Thread.Sleep(tone.Delay);
            }
        }

        /// <summary>
        /// Plays a Frequency.
        /// </summary>
        /// <param name="frequency">Frequency to play.</param>
        public static void PlayFrequency(int frequency)
        {
            Initialize();
            Stop();

            _pwm.Frequency = frequency;
            _pwm.Start();
            _started = true;
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

    /// <summary>
    /// Controls the buttons on the BrainPad.
    /// </summary>
    public class Button
    {
        private static Mode _mode = Mode.Interrupt;
        private static int[] _pins = new int[4] {15,45,4,5};
        private static InputPort[] _ports1 = new InputPort[4];
        private static InterruptPort[] _ports2 = new InterruptPort[4];
        private static bool[] _pressed = new bool[4];

        private enum Mode
        {
            Input,
            Interrupt
        }

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
        /// Determine if a button was pressed.
        /// </summary>
        /// <param name="button">The <see cref="DPad"/> button to check.</param>
        /// <returns></returns>
        public static bool IsPressed(DPad button)
        {
            int i = (int)button;

            if (_mode == Mode.Input & _ports1[i] == null)
            {
                _ports1[i] = new InputPort((Cpu.Pin)_pins[i], true, Port.ResistorMode.PullUp);
            }
            else if (_mode == Mode.Interrupt & _ports2[i] == null)
            {
                _ports2[i] = new InterruptPort((Cpu.Pin)_pins[i], true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
                _ports2[i].OnInterrupt += OnInterrupt_ButtonPressed;
            }

            if (_mode == Mode.Input)
            {
                return (_ports1[i].Read() == false);
            }
            else
            {
                return _pressed[i];
            }

        }

        private static void OnInterrupt_ButtonPressed(uint data1, uint data2, System.DateTime time)
        {
            for (int i = 0; i <= _pins.Length - 1; i += 1)
            {
                if (_pins[i] == data1)
                    _pressed[i] = (data2 == 0);
            }
        }
    }

    /// <summary>
    /// Controls the Traffic Light.
    /// </summary>
    public class TrafficLight
    {
        private static PWM[] _ryg = new PWM[3];

        private static void Initialize()
        {
            if (_ryg[0] == null)
            {
                _ryg[0] = new PWM((Cpu.PWMChannel)8, 200, 1, false);
                _ryg[1] = new PWM(Cpu.PWMChannel.PWM_7, 200, 1, false);
                _ryg[2] = new PWM((Cpu.PWMChannel)14, 200, 1, false);
            }
        }

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
            Initialize();
            TurnOff(color);

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
            Initialize();

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
            Initialize();

            _ryg[0].Stop();
            _ryg[1].Stop();
            _ryg[2].Stop();
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

    /// <summary>
    /// Controls the Light Sensor on the BrainPad.
    /// </summary>
    public class LightSensor
    {
        private static AnalogInput _ain;

        /// <summary>
        /// Get the level of brightness.
        /// </summary>
        /// <returns>A value respresenting the level of brightness.</returns>
        public static double GetLevel()
        {
            if (_ain == null)
                _ain = new AnalogInput((Cpu.AnalogChannel)9);

            return _ain.Read();
        }
    }

    /// <summary>
    /// Controls the Temperature Sensor on the BrainPad.
    /// </summary>
    public class TemperatureSensor
    {
        private static AnalogInput _ain;

        /// <summary>
        /// Read the temperature.
        /// </summary>
        /// <returns>The temperature in Celsius.</returns>
        public static double ReadTemperature()
        {
            if (_ain == null)
                _ain = new AnalogInput((Cpu.AnalogChannel)8);

            double sum = 0;
            for (int i = 0; i < 10; i++)
                sum += _ain.Read() ;
            double avg = sum / 10;

            double mVout = avg * 1000 * 3.3;
            double tempC = (mVout - 450.0) / 19.5;

            return tempC;
        }
    }

    /// <summary>
    /// Controls the Accelerometer on the BrainPad.
    /// </summary>
    public class Accelerometer
    {
        private static I2C _i2c;

        private static void Initialize()
        {
            if (_i2c == null)
            {
                _i2c = new I2C(0x1c);
                _i2c.Write(new byte[2] { 0x2a, 0x01 });
            }
        }

        private static double ReadAxis(byte register)
        {
            Initialize();

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
        const int backlight = 9;

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

        /// <summary>
        /// Prepares the Display for use.
        /// </summary>
        public static void Initialize()
        {
            _rs = new OutputPort((Cpu.Pin)rs, false);

            _reset = new OutputPort((Cpu.Pin)reset, false);
            _backlight = new OutputPort((Cpu.Pin)backlight, true);
            _spi_con = new SPI.Configuration((Cpu.Pin)cs, false, 0, 0, false, true, 4000, SPI.SPI_module.SPI2);
            _spi = new SPI(_spi_con);

            Reset();
            //------------------------------------------------------------------//  
            //-------------------Software Reset-------------------------------//
            //------------------------------------------------------------------//

            WriteCommand(0x11);//Sleep exit 
            Thread.Sleep(120);

            //ST7735R Frame Rate
            WriteCommand(0xB1);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteCommand(0xB2);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteCommand(0xB3);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);

            WriteCommand(0xB4); //Column inversion 
            WriteData(0x07);

            //ST7735R Power Sequence
            WriteCommand(0xC0);
            WriteData(0xA2); WriteData(0x02); WriteData(0x84);
            WriteCommand(0xC1); WriteData(0xC5);
            WriteCommand(0xC2);
            WriteData(0x0A); WriteData(0x00);
            WriteCommand(0xC3);
            WriteData(0x8A); WriteData(0x2A);
            WriteCommand(0xC4);
            WriteData(0x8A); WriteData(0xEE);

            WriteCommand(0xC5); //VCOM 
            WriteData(0x0E);

            WriteCommand(0x36); //MX, MY, RGB mode 
            //WriteData(0xC8);
            WriteData(MADCTL_MX | MADCTL_MY | MADCTL_BGR);

            //ST7735R Gamma Sequence
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

            //rotate
            WriteCommand(ST7735_MADCTL);
            //WriteData((byte)(MADCTL_MV | MADCTL_MX | (this.isBgr ? MADCTL_BGR : 0)));
            WriteData((byte)(MADCTL_MV | MADCTL_MY | (isBgr ? MADCTL_BGR : 0)));

            WriteCommand(0x29);//Display on
            Thread.Sleep(500);
        }

        private static void SetClip(int x, int y, int w, int h)
        {
            if (w < 1 || h < 1)
                throw new Exception();

            ushort x_end = (ushort)(x + w - 1);
            ushort y_end = (ushort)(y + h - 1);
            WriteCommand(0x2A);
            WriteData((byte)((x >> 8) & 0xFF));
            WriteData((byte)(x & 0xFF));
            WriteData((byte)((x_end >> 8) & 0xFF));
            WriteData((byte)(x_end & 0xFF));
            WriteCommand(0x2B);
            WriteData((byte)((y >> 8) & 0xFF));
            WriteData((byte)(y & 0xFF));
            WriteData((byte)((y_end >> 8) & 0xFF));
            WriteData((byte)(y_end & 0xFF));
        }

        /// <summary>
        /// Clears the Display.
        /// </summary>
        public static void Clear()
        {
            byte[] temp = new byte[180 * 2];
            SetClip(0, 0, 180, 128);
            WriteCommand(0x2C);
            for (int i = 0; i < 128; i++)
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

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="color">The color to use.</param>
        public static void DrawFillRect(int x, int y, int w, int h, BrainPad.Color.Palette color)
        {
            ushort _color = BrainPad.Color.FromPalette(color);
            SetClip(x, y, w, h);
            byte[] colors = new byte[w * h * 2];
            for (int i = 0; i < colors.Length; i += 2)
            {
                colors[i] = (byte)((_color >> 8) & 0xFF);
                colors[i + 1] = (byte)((_color >> 0) & 0xFF);
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
        public static void SetPixel(int x, int y, ushort color)
        {
            SetClip(x, y, 1, 1);
            b2[0] = (byte)(color >> 8);
            b2[1] = (byte)(color >> 0);
            WriteCommand(0x2C);
            WriteData(b2);
        }

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="x0">Starting X coordinate.</param>
        /// <param name="y0">Starting Y coordinate.</param>
        /// <param name="x1">Ending X coordinate.</param>
        /// <param name="y1">Ending Y coordinate.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawLine(int x0, int y0, int x1, int y1, ushort color)
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
        public static void DrawCircle(int x0, int y0, int r, ushort color)
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
        public static void DrawRect(int x, int y, int w, int h, ushort color)
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

        /*
        private static void Draw8PixelsSlow(int x, int y, byte c, ushort color)
        {
            for (int i = 0; i < 8; i++)
            {
                if (((c >> i) & 1) > 0)
                    SetPixel(x, y + i, color);
                else
                    SetPixel(x, y + i, 0);
            }

        }
        */

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
            WriteCommand(0x2C);
            WriteData(chartemp);
        }

        private static void Draw8LargePixelsSlow(int x, int y, byte c, ushort color)
        {
            for (int i = 0; i < 8; i++)
            {
                if (((c >> i) & 1) > 0)
                {
                    SetPixel(x, y + i * 2, color);
                    SetPixel(x + 1, y + i * 2, color);
                    SetPixel(x, y + i * 2 + 1, color);
                    SetPixel(x + 1, y + i * 2 + 1, color);

                }
                else
                {
                    SetPixel(x, y + i * 2, 0);
                    SetPixel(x + 1, y + i * 2, 0);
                    SetPixel(x, y + i * 2 + 1, 0);
                    SetPixel(x + 1, y + i * 2 + 1, 0);
                }
            }

        }

        /// <summary>
        /// Draw a character.
        /// </summary>
        /// <param name="size">The font size to use.</param>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="c">The character to draw.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawCharacter(FontSize size, int x, int y, char c, BrainPad.Color.Palette color)
        {
            if (size == FontSize.Small)
                DrawCharacter(x, y, c, color);
            else
                DrawLargeCharacter(x, y, c, color);
        }

        /// <summary>
        /// Draws a string.
        /// </summary>
        /// <param name="size">The font size to use.</param>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="str">The string to draw.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawString(FontSize size, int x, int y, string str, BrainPad.Color.Palette color)
        {
            if (size == FontSize.Small)
                DrawString(x, y, str, color);
            else
                DrawLargeString(x, y, str, color);
        }

        /// <summary>
        /// Draws a character in large font.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="c">The character to draw.</param>
        /// <param name="color">The color to use.</param>
        private static void DrawLargeCharacter(int x, int y, char c, BrainPad.Color.Palette color)
        {
            ushort _color = BrainPad.Color.FromPalette(color);
            if (c > 126 || c < 32)
                return;
            c = (char)(c - 32);
            for (int i = 0; i < 5; i++)
            {
                Draw8LargePixelsSlow(x + i * 2, y, font[5 * c + i], _color);
            }
        }

        /// <summary>
        /// Draws a string in large font.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="str">The string to draw.</param>
        /// <param name="color">The color to use.</param>
        private static void DrawLargeString(int x, int y, string str, BrainPad.Color.Palette color)
        {
            for (int i = 0; i < str.Length; i++)
            {
                DrawLargeCharacter(x + i * 7 * 2, y, str[i], color);
            }
        }

        /// <summary>
        /// Draws a character in small font.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="c">The character to draw.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawCharacter(int x, int y, char c, BrainPad.Color.Palette color)
        {
            ushort _color = BrainPad.Color.FromPalette(color);
            if (c > 126 || c < 32)
                return;
            c = (char)(c - 32);
            for (int i = 0; i < 5; i++)
                Draw8Pixels(x + i, y, font[5 * c + i], _color);
        }

        /// <summary>
        /// Draws a string in small font.
        /// </summary>
        /// <param name="x">Starting X coordinate.</param>
        /// <param name="y">Starting Y coordinate.</param>
        /// <param name="str">The string to draw.</param>
        /// <param name="color">The color to use.</param>
        public static void DrawString(int x, int y, string str, BrainPad.Color.Palette color)
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
    public class Wait
    {
        /// <summary>
        /// Tells the BrainPad to wait for a number of seconds.
        /// </summary>
        /// <param name="sec">Seconds to wait.</param>
        public static void Seconds(int sec)
        {
            Thread.Sleep(sec * 1000);
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
