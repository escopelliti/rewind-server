using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace NativeStructure
{
    class InputFactory
    {
        public static INPUT CreateKeyUpInput(Keys key)
        {
            INPUT keyboard_input = new INPUT();
            keyboard_input.type = TYPE.INPUT_KEYBOARD;
            keyboard_input.ki = new KEYBDINPUT();
            keyboard_input.ki.wVk = (VirtualKeyCode)key;
            keyboard_input.ki.wScan = 0;
            keyboard_input.ki.dwFlags = KeyboardFlag.KeyUp;
            keyboard_input.ki.dwExtraInfo = IntPtr.Zero;
            return keyboard_input;
        }

    }
}
