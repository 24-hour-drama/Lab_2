using Library;
using Xunit;
using System;

namespace Tests
{
    public class UnitTest1
    {
        private readonly NightlightController _controller = new();
        [Fact]
        public void SetBrightnessZero()
        {
            var state = new NightlightState { isOn = true, Brightness = 50 };

            _controller.SetBrightness(state, 0);

            Assert.Equal(0, state.Brightness);
            Assert.False(state.isOn);
        }

        [Fact]
        public void SetBrightnessHundred()
        {
            var state = new NightlightState { isOn = true, Brightness = 50 };

            _controller.SetBrightness(state, 100);

            Assert.Equal(100, state.Brightness);
            Assert.True(state.isOn);
        }

        [Fact]
        public void SetBrightness_OutOfRange_More()
        {
            var state = new NightlightState { isOn = true, Brightness = 50 };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _controller.SetBrightness(state, 101));
        }

        [Fact]
        public void SetBrightness_OutOfRange_Less()
        {
            var state = new NightlightState { isOn = true, Brightness = 50 };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _controller.SetBrightness(state, -1));

        }

        [Fact]
        public void TurnItOn()
        {
            var state = new NightlightState { isOn = false, Brightness = 0 };
            _controller.TurnOn(state);
            Assert.True(state.isOn);
            Assert.Equal(50, state.Brightness);
        }

        [Fact] 
        public void TurnOn()
        {
            var state = new NightlightState { isOn = true };
            Assert.Throws<InvalidOperationException>(() => _controller.TurnOn(state));
        }


        [Fact]
        public void TurnItOff()
        {
            var state = new NightlightState { isOn = true };
            _controller.TurnOff(state);
            Assert.False(state.isOn);
        }

        [Fact]
        public void TurnOff()
        {
            var state = new NightlightState { isOn = false };
            Assert.Throws<InvalidOperationException>(() => _controller.TurnOff(state));
        }

        [Fact]
        public void SetTimer_Min_DeactivatesTimer()
        {
            var state = new NightlightState
            {
                isOn = true,
                TimerMinutes = 30,
                isTimerActive = true
            };

            _controller.SetTimer(state, 0);

            Assert.Equal(0, state.TimerMinutes);
            Assert.False(state.isTimerActive);
        }

        [Fact]
        public void SetTimer_Max_ActivatesTimer()
        {
            var state = new NightlightState
            {
                isOn = true,

                TimerMinutes = 30,
                isTimerActive = true
            };

            _controller.SetTimer(state, 60);

            Assert.Equal(60, state.TimerMinutes);
            Assert.True(state.isTimerActive);
        }

        [Fact]
        public void SetTimer_OutOfRange_More()
        {
            var state = new NightlightState
            {
                isOn = true,
                TimerMinutes = 30,
                isTimerActive = true
            };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _controller.SetTimer(state, 61));
        }

        [Fact]
        public void SetTimer_OutOfRange_Less()
        {
            var state = new NightlightState
            {
                isOn = true,
                TimerMinutes = 30,
                isTimerActive = true
            };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _controller.SetTimer(state, -1));
        }

    }
}
