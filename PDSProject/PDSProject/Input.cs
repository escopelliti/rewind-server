using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
/*
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.IO;
 */

using System.Runtime.InteropServices;

//struttura di riferimento
//http://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx

namespace Protocol
{
            
        [StructLayout(LayoutKind.Explicit)]
        struct INPUT
        {
            [FieldOffset(0)]
            public UInt32 type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

   
        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public UInt32 dx;
            public UInt32 dy;
            public UInt32 mouseData;
            public MouseFlag dwFlags;
            public UInt32 time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public VirtualKeyCode wVk;
            public UInt16 wScan;
            public KeyboardFlag dwFlags;
            public UInt32 time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public UInt32 uMsg;
            public UInt16 wParamL;
            public UInt16 wParamH;
        }
    }


    public enum VirtualKeyCode : ushort
    {
        //Mouse button const
        // Left mouse button
        LBUTTON = 0x01,
        // Right mouse button
        RBUTTON = 0x02,
        // Control-break processing
        CANCEL = 0x03,
        // Middle mouse button (three-button mouse) - NOT contiguous with LBUTTON and RBUTTON
        MBUTTON = 0x04,
        // Windows 2000/XP: X1 mouse button - NOT contiguous with LBUTTON and RBUTTON
        XBUTTON1 = 0x05,
        // Windows 2000/XP: X2 mouse button - NOT contiguous with LBUTTON and RBUTTON
        XBUTTON2 = 0x06,

        //Keyboard button const 
        // BACKSPACE key
        BACK = 0x08,
        // TAB key
        TAB = 0x09,
        // CLEAR key
        CLEAR = 0x0C,
        // ENTER key
        RETURN = 0x0D,
        // SHIFT key
        SHIFT = 0x10,
        // CTRL key
        CONTROL = 0x11,
        // ALT key
        MENU = 0x12,
        // PAUSE key
        PAUSE = 0x13,
        // CAPS LOCK key
        CAPITAL = 0x14,

//=====================================================================
        
        // Input Method Editor (IME) Kana mode
        KANA = 0x15,
        // IME Hanguel mode (maintained for compatibility; use HANGUL)
        HANGEUL = 0x15,
        // IME Hangul mode
        HANGUL = 0x15,
        // IME Junja mode
        JUNJA = 0x17,
        // IME final mode
        FINAL = 0x18,
        // IME Hanja mode
        HANJA = 0x19,
        // IME Kanji mode
        KANJI = 0x19,
 
//=====================================================================

        // ESC key
        ESCAPE = 0x1B,

//=====================================================================

        // IME convert
        CONVERT = 0x1C,
        // IME nonconvert
        NONCONVERT = 0x1D,
        // IME accept
        ACCEPT = 0x1E,
        // IME mode change request
        MODECHANGE = 0x1F,
       
//=====================================================================
       
        // SPACEBAR
        SPACE = 0x20,
        // PAGE UP key
        PRIOR = 0x21,
        // PAGE DOWN key
        NEXT = 0x22,
        // END key
        END = 0x23,
        // HOME key
        HOME = 0x24,
        // LEFT ARROW key
        LEFT = 0x25,
        // UP ARROW key
        UP = 0x26,
        // RIGHT ARROW key
        RIGHT = 0x27,
        // DOWN ARROW key
        DOWN = 0x28,
        // SELECT key
        SELECT = 0x29,
        // PRINT key
        PRINT = 0x2A,
        // EXECUTE key
        EXECUTE = 0x2B,
        // PRINT SCREEN key
        SNAPSHOT = 0x2C,
        // INS key
        INSERT = 0x2D,
        // DEL key
        DELETE = 0x2E,
        // HELP key
        HELP = 0x2F,
        // 0 key
        VK_0 = 0x30,
        // 1 key
        VK_1 = 0x31,
        // 2 key
        VK_2 = 0x32,
        // 3 key
        VK_3 = 0x33,
        // 4 key
        VK_4 = 0x34,
        // 5 key
        VK_5 = 0x35,
        // 6 key
        VK_6 = 0x36,
        // 7 key
        VK_7 = 0x37,
        // 8 key
        VK_8 = 0x38,
        // 9 key
        VK_9 = 0x39,
        // A key
        VK_A = 0x41,
        // B key
        VK_B = 0x42,
        // C key
        VK_C = 0x43,
        // D key
        VK_D = 0x44,
        // E key
        VK_E = 0x45,
        // F key
        VK_F = 0x46,
        // G key
        VK_G = 0x47,
        // H key
        VK_H = 0x48,
        // I key
        VK_I = 0x49,
        // J key
        VK_J = 0x4A,
        // K key
        VK_K = 0x4B,
        // L key
        VK_L = 0x4C,
        // M key
        VK_M = 0x4D,
        // N key
        VK_N = 0x4E,
        // O key
        VK_O = 0x4F,
        // P key
        VK_P = 0x50,
        // Q key
        VK_Q = 0x51,
        // R key
        VK_R = 0x52,
        // S key
        VK_S = 0x53,
        // T key
        VK_T = 0x54,
        // U key
        VK_U = 0x55,
        // V key
        VK_V = 0x56,
        // W key
        VK_W = 0x57,
        // X key
        VK_X = 0x58,
        // Y key
        VK_Y = 0x59,
        // Z key
        VK_Z = 0x5A,
        // Left Windows key (Microsoft Natural keyboard)
        LWIN = 0x5B,
        // Right Windows key (Natural keyboard)
        RWIN = 0x5C,
        // Applications key (Natural keyboard)
        APPS = 0x5D,
        // Computer Sleep key
        SLEEP = 0x5F,
        // Numeric keypad 0 key
        NUMPAD0 = 0x60,
        // Numeric keypad 1 key
        NUMPAD1 = 0x61,
        // Numeric keypad 2 key
        NUMPAD2 = 0x62,
        // Numeric keypad 3 key
        NUMPAD3 = 0x63,
        // Numeric keypad 4 key
        NUMPAD4 = 0x64,
        // Numeric keypad 5 key
        NUMPAD5 = 0x65,
        // Numeric keypad 6 key
        NUMPAD6 = 0x66,
        // Numeric keypad 7 key
        NUMPAD7 = 0x67,
        // Numeric keypad 8 key
        NUMPAD8 = 0x68,
        // Numeric keypad 9 key
        NUMPAD9 = 0x69,
        // Multiply key
        MULTIPLY = 0x6A,
        // Add key
        ADD = 0x6B,
        // Separator key
        SEPARATOR = 0x6C,
        // Subtract key
        SUBTRACT = 0x6D,
        // Decimal key
        DECIMAL = 0x6E,
        // Divide key
        DIVIDE = 0x6F,
        // F1 key
        F1 = 0x70,
        // F2 key
        F2 = 0x71,
        // F3 key
        F3 = 0x72,
        // F4 key
        F4 = 0x73,
        // F5 key
        F5 = 0x74,
        // F6 key
        F6 = 0x75,
        // F7 key
        F7 = 0x76,
        // F8 key
        F8 = 0x77,
        // F9 key
        F9 = 0x78,
        // F10 key
        F10 = 0x79,
        // F11 key
        F11 = 0x7A,
        // F12 key
        F12 = 0x7B,

//=====================================================================

        // F13 key
        F13 = 0x7C,
        // F14 key
        F14 = 0x7D,
        // F15 key
        F15 = 0x7E,
        // F16 key
        F16 = 0x7F,
        // F17 key
        F17 = 0x80,
        // F18 key
        F18 = 0x81,
        // F19 key
        F19 = 0x82,
        // F20 key
        F20 = 0x83,
        // F21 key
        F21 = 0x84,
        // F22 key
        F22 = 0x85,
        // F23 key
        F23 = 0x86,
        // F24 key
        F24 = 0x87,

//=====================================================================

        // NUM LOCK key
        NUMLOCK = 0x90,
        // SCROLL LOCK key
        SCROLL = 0x91,

        // L* & R* - left and right Alt, Ctrl and Shift virtual keys.
        // Used only as parameters to GetAsyncKeyState() and GetKeyState().
        // No other API or message will distinguish left and right keys in this way.
        
        // Left SHIFT key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        LSHIFT = 0xA0,
        // Right SHIFT key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        RSHIFT = 0xA1,
        // Left CONTROL key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        LCONTROL = 0xA2,
        // Right CONTROL key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        RCONTROL = 0xA3,
        // Left MENU key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        LMENU = 0xA4,
        // Right MENU key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        RMENU = 0xA5,
        // Windows 2000/XP: Browser Back key
        BROWSER_BACK = 0xA6,
        // Windows 2000/XP: Browser Forward key
        BROWSER_FORWARD = 0xA7,
        // Windows 2000/XP: Browser Refresh key
        BROWSER_REFRESH = 0xA8,
        // Windows 2000/XP: Browser Stop key
        BROWSER_STOP = 0xA9,
        // Windows 2000/XP: Browser Search key
        BROWSER_SEARCH = 0xAA,
        // Windows 2000/XP: Browser Favorites key
        BROWSER_FAVORITES = 0xAB,
        // Windows 2000/XP: Browser Start and Home key
        BROWSER_HOME = 0xAC,
        // Windows 2000/XP: Volume Mute key
        VOLUME_MUTE = 0xAD,
        // Windows 2000/XP: Volume Down key
        VOLUME_DOWN = 0xAE,
        // Windows 2000/XP: Volume Up key
        VOLUME_UP = 0xAF,
        // Windows 2000/XP: Next Track key
        MEDIA_NEXT_TRACK = 0xB0,
        // Windows 2000/XP: Previous Track key
        MEDIA_PREV_TRACK = 0xB1,
        // Windows 2000/XP: Stop Media key
        MEDIA_STOP = 0xB2,
        // Windows 2000/XP: Play/Pause Media key
        MEDIA_PLAY_PAUSE = 0xB3,
        // Windows 2000/XP: Start Mail key
        LAUNCH_MAIL = 0xB4,
        // Windows 2000/XP: Select Media key
        LAUNCH_MEDIA_SELECT = 0xB5,
        // Windows 2000/XP: Start Application 1 key
        LAUNCH_APP1 = 0xB6,
        // Windows 2000/XP: Start Application 2 key
        LAUNCH_APP2 = 0xB7,

//=====================================================================

        // Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the ';:' key 
        OEM_1 = 0xBA,
        // Windows 2000/XP: For any country/region, the '+' key
        OEM_PLUS = 0xBB,
        // Windows 2000/XP: For any country/region, the ',' key
        OEM_COMMA = 0xBC,
        // Windows 2000/XP: For any country/region, the '-' key
        OEM_MINUS = 0xBD,
        // Windows 2000/XP: For any country/region, the '.' key
        OEM_PERIOD = 0xBE,
        // Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '/?' key 
        OEM_2 = 0xBF,
        // Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '`~' key 
        OEM_3 = 0xC0,
        // Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '[{' key
        OEM_4 = 0xDB,
        // Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '\|' key
        OEM_5 = 0xDC,
        // Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the ']}' key
        OEM_6 = 0xDD,
        // Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the 'single-quote/double-quote' key
        OEM_7 = 0xDE,
        // Used for miscellaneous characters; it can vary by keyboard.
        OEM_8 = 0xDF,
        // Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
        OEM_102 = 0xE2,
        // Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
        PROCESSKEY = 0xE5,
        // Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes. The PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
        PACKET = 0xE7,
        // Attn key
        ATTN = 0xF6,
        // CrSel key
        CRSEL = 0xF7,
        // ExSel key
        EXSEL = 0xF8,
        // Erase EOF key
        EREOF = 0xF9,
        // Play key
        PLAY = 0xFA,
        // Zoom key
        ZOOM = 0xFB,
        // Reserved
        NONAME = 0xFC,
        // PA1 key
        PA1 = 0xFD,
        // Clear key
        OEM_CLEAR = 0xFE,
   }

    [Flags]
    internal enum KeyboardFlag : uint // UInt32
    {
        // KEYEVENTF_EXTENDEDKEY = 0x0001 (If specified, the scan code was preceded by a prefix byte that has the value 0xE0 (224).)
        ExtendedKey = 0x0001,
        // KEYEVENTF_KEYUP = 0x0002 (If specified, the key is being released. If not specified, the key is being pressed.)
        KeyUp = 0x0002,
        // KEYEVENTF_UNICODE = 0x0004 (If specified, wScan identifies the key and wVk is ignored.)
        Unicode = 0x0004,
        // KEYEVENTF_SCANCODE = 0x0008 (Windows 2000/XP: If specified, the system synthesizes a VK_PACKET keystroke. The wVk parameter must be zero. This flag can only be combined with the KEYEVENTF_KEYUP flag. For more information, see the Remarks section.)
        ScanCode = 0x0008,
    }

    // The mouse button
    public enum MouseButton
    {
        // Left mouse button
        LeftButton,

        // Middle mouse button
        MiddleButton,

        // Right moust button
        RightButton,
    }

    [Flags]
    internal enum MouseFlag : uint // UInt32
    {
        // Specifies that movement occurred.
        Move = 0x0001,
        // Specifies that the left button was pressed.
        LeftDown = 0x0002,
        // Specifies that the left button was released.
        LeftUp = 0x0004,
        // Specifies that the right button was pressed.
        RightDown = 0x0008,       
        // Specifies that the right button was released.
        RightUp = 0x0010,
        // Specifies that the middle button was pressed.
        MiddleDown = 0x0020,
        // Specifies that the middle button was released.
        MiddleUp = 0x0040,
        // Windows 2000/XP: Specifies that an X button was pressed.
        XDown = 0x0080,
        // Windows 2000/XP: Specifies that an X button was released.
        XUp = 0x0100,
        // Windows NT/2000/XP: Specifies that the wheel was moved, if the mouse has a wheel. The amount of movement is specified in mouseData. 
        VerticalWheel = 0x0800,
        // Specifies that the wheel was moved horizontally, if the mouse has a wheel. The amount of movement is specified in mouseData. Windows 2000/XP:  Not supported.
        HorizontalWheel = 0x1000,
        // Windows 2000/XP: Maps coordinates to the entire desktop. Must be used with MOUSEEVENTF_ABSOLUTE.
        VirtualDesk = 0x4000,
        // Specifies that the dx and dy members contain normalized absolute coordinates. If the flag is not set, dxand dy contain relative data (the change in position since the last reported position). This flag can be set, or not set, regardless of what kind of mouse or other pointing device, if any, is connected to the system. For further information about relative mouse motion, see the following Remarks section.
        Absolute = 0x8000,
    }

