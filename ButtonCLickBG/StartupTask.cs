using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.System.Threading;
using System.Diagnostics;     //allows me to use Write to Debug Output console via Debug.WriteLine("....");

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ButtonCLickBG
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral mydeferral;
        private ThreadPoolTimer mytimer;
        private const int BUTTONPINNBR = 16;
        private const int LEDPINNBR = 6;
        private GpioPin buttonPin, ledPin;
        private GpioPinValue buttonPinValCurrent, buttonPinValPrior, ledPinVal;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to start one or more asynchronous methods 
            //
            mydeferral = taskInstance.GetDeferral();   //prevents program from Exiting after one run
            InitGPIO();
            mytimer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(20));

        }

        private void InitGPIO()
        {
            var mygpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (mygpio == null)
            {
                buttonPin = null;
                ledPin = null;
                return;
            }
            ledPin = mygpio.OpenPin(LEDPINNBR);
            ledPin.Write(GpioPinValue.Low); //initialize Led to On as wired in active Low config (+3.3-Led-GPIO)
            ledPin.SetDriveMode(GpioPinDriveMode.Output);
            

            buttonPin = mygpio.OpenPin(BUTTONPINNBR);
            buttonPin.Write(GpioPinValue.High);
            buttonPin.SetDriveMode(GpioPinDriveMode.Output);
            buttonPinValCurrent = buttonPin.Read();
            buttonPin.SetDriveMode(GpioPinDriveMode.Input);
            buttonPinValPrior = GpioPinValue.High;
            
            Debug.WriteLine("ButtonPin Value at Init: " + buttonPin.Read() + ",      with Pin ID = " + buttonPin.PinNumber);

            //buttonPinVal = buttonPin.Read();
            // Set a debounce timeout to filter out switch bounce noise from a button press
            buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(20);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            buttonPin.ValueChanged += buttonPressAction;
            
        }

        private void Timer_Tick(ThreadPoolTimer mytimer)
        {
            buttonPinValCurrent = buttonPin.Read();
            //Debug.WriteLine("ButtonPin Current Val = " + buttonPinValCurrent + ",      with prior value = " + buttonPinValPrior);
            if (buttonPinValCurrent == GpioPinValue.High && buttonPinValPrior == GpioPinValue.Low)    // that is a Rising edge, meaning button was Pressed in current setup 
            {
                //Debug.WriteLine("ButtonPin Current Val = " + buttonPinValCurrent + ",      with prior value = " + buttonPinValPrior);
                /*ledPinVal = (ledPinVal == GpioPinValue.Low) ?
                    GpioPinValue.High : GpioPinValue.Low;
                ledPin.Write(ledPinVal);*/
            }
            buttonPinValPrior = buttonPinValCurrent;
        }
        private void buttonPressAction(GpioPin mycallerPin, GpioPinValueChangedEventArgs myevent)
        {
            //Debug.WriteLine("Event handler detected ButtonPin Change : " + myevent.Edge);
            if (myevent.Edge == GpioPinEdge.RisingEdge) //RisingEdge)
            {
                Debug.WriteLine("Event handler detected RISING Edge");
                ledPinVal = (ledPinVal == GpioPinValue.Low) ?
                    GpioPinValue.High : GpioPinValue.Low;
                ledPin.Write(ledPinVal);
            }
            else
            {
                Debug.WriteLine("Event handler detected FALLING Edge");
            }
            
        }
    }
}
